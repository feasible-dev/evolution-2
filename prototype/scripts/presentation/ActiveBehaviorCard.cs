using Godot;

namespace EvolutionTwo.Presentation;

/// <summary>A compact card face. Title, art and allocation remain separate content slots.</summary>
public partial class ActiveBehaviorCard : Control
{
    public BehaviorChoice Choice { get; set; }
    public Color TeamColor { get; set; }
    public int Allocation { get; private set; }
    private Font _font = null!;
    private Texture2D _art = null!;
    private bool _hover;
    public override void _Ready()
    {
        MouseFilter = MouseFilterEnum.Stop;
        _font = GD.Load<Font>("res://assets/fonts/RacingSansOne-Regular.ttf");
        _art = GD.Load<Texture2D>($"res://assets/studio-v49/{Choice.ToString().ToLowerInvariant()}.svg");
        MouseEntered += () => { _hover = true; QueueRedraw(); };
        MouseExited += () => { _hover = false; QueueRedraw(); };
        Resized += QueueRedraw;
        SetAllocation(0);
    }
    public void SetAllocation(int points)
    {
        if (_art is null) return;
        Allocation = points;
        var definition = BehaviorCatalog.Get(Choice);
        TooltipText = definition.Name + "\n" + definition.Help + "\n" +
            L10n.Text("ui.card_points", points) + "\n" + L10n.Text("plan.locked");
        QueueRedraw();
    }
    public override void _Draw()
    {
        if (_art is null) return;
        var face = new Rect2(1, 1, Size.X - 5, Size.Y - 6);
        using var paper = new StyleBoxFlat { BgColor = new Color(_hover ? "242b2b" : "171c1d"),
            BorderColor = new Color(Allocation == 0 ? "626760" : "c9bb97"),
            BorderWidthLeft = 1, BorderWidthRight = 1, BorderWidthTop = 2, BorderWidthBottom = 2,
            CornerRadiusTopLeft = 6, CornerRadiusTopRight = 6, CornerRadiusBottomLeft = 6, CornerRadiusBottomRight = 6,
            ShadowColor = Colors.Black, ShadowSize = 2, ShadowOffset = new Vector2(2, 3) };
        DrawStyleBox(paper, face);
        DrawLine(new Vector2(6, Size.Y - 3), new Vector2(Size.X - 4, Size.Y - 3), new Color("796f59"), 1);
        DrawColoredPolygon(new[] { new Vector2(1, 6), new Vector2(6, 1), new Vector2(25, 1), new Vector2(22, 7) }, TeamColor);
        var title = BehaviorCatalog.Get(Choice).Name;
        var fontSize = 14;
        while (fontSize > 8 && _font.GetStringSize(title, fontSize: fontSize).X > face.Size.X - 8) fontSize--;
        var ink = new Color(Allocation == 0 ? "92968b" : "efe3c6");
        DrawString(_font, new Vector2((face.Size.X - _font.GetStringSize(title, fontSize: fontSize).X) / 2, 23), title,
            fontSize: fontSize, modulate: ink);
        var artSize = MathF.Min(Size.X - 12, Size.Y - 52);
        DrawTextureRect(_art, new Rect2((Size.X - artSize) / 2 - 2, 28, artSize, artSize), false,
            new Color(1, 1, 1, Allocation == 0 ? .48f : 1));
        var value = L10n.Text("ui.card_points", Allocation);
        var font = GetThemeDefaultFont();
        DrawString(font, new Vector2((face.Size.X - font.GetStringSize(value, fontSize: 10).X) / 2, Size.Y - 12),
            value, fontSize: 10, modulate: TeamColor);
    }
}
