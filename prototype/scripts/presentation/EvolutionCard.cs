using Godot;

namespace EvolutionTwo.Presentation;

/// <summary>The approved 300x420 card, with budget controls hosted separately by the preparation panel.</summary>
public partial class EvolutionCard : Control
{
    public static readonly Vector2 DesignSize = new(300, 420);
    public BehaviorChoice Choice { get; set; }
    public Button FlipButton { get; private set; } = null!;
    public bool IsFlipped { get; private set; }
    public Texture2D IllustrationTexture => _illustration;
    private Button _artButton = null!;
    private Texture2D _frame = null!, _illustration = null!;
    private readonly SystemFont _titleFont = new() { FontNames = new[] { "Baskerville Old Face" } };
    private readonly SystemFont _bodyFont = new() { FontNames = new[] { "Book Antiqua" } };
    private readonly SystemFont _uiFont = new() { FontNames = new[] { "Tahoma" } };
    private bool _ownsVectors;
    private static int _vectorOwners;
    private static readonly Dictionary<string, Texture2D> Vectors = new();
    private sealed record Face(string Title, string Effect, string Asset, string Heading, string[] Lines, string Summary);
    private Face Content => Choice switch
    {
        BehaviorChoice.Throttle => new(L10n.Text("card.thrust.title"), L10n.Text("card.thrust.effect"), "poussee",
            L10n.Text("card.thrust.heading"), new[] { L10n.Text("card.thrust.line1"), L10n.Text("card.thrust.line2") }, L10n.Text("card.thrust.summary")),
        BehaviorChoice.Turn => new(L10n.Text("card.turn.title"), L10n.Text("card.turn.effect"), "virage",
            L10n.Text("card.turn.heading"), new[] { L10n.Text("card.turn.line1"), L10n.Text("card.turn.line2") }, L10n.Text("card.turn.summary")),
        _ => new(L10n.Text("card.target.title"), L10n.Text("card.target.effect"), "cible",
            L10n.Text("card.target.heading"), new[] { L10n.Text("card.target.line1"), L10n.Text("card.target.line2") }, L10n.Text("card.target.summary"))
    };

    public override void _Ready()
    {
        Size = DesignSize;
        MouseFilter = MouseFilterEnum.Pass;
        var face = Content;
        _frame = Vector("cadre");
        _illustration = Vector(face.Asset);
        _ownsVectors = true;
        _vectorOwners++;
        _artButton = MakeButton("", new Vector2(16, 10), new Vector2(268, 333));
        _artButton.TooltipText = L10n.Text("card.flip_tip");
        FlipButton = MakeButton(L10n.Text("card.back"), new Vector2(207, 352), new Vector2(70, 21));
        FlipButton.AddThemeFontOverride("font", _uiFont);
        FlipButton.AddThemeFontSizeOverride("font_size", 13);
        FlipButton.AddThemeColorOverride("font_color", new Color("adc5d0"));
    }

    private Button MakeButton(string text, Vector2 position, Vector2 size)
    {
        var button = new Button { Text = text, Position = position, Size = size, Flat = true,
            MouseDefaultCursorShape = CursorShape.PointingHand };
        foreach (var state in new[] { "normal", "hover", "pressed" })
            button.AddThemeStyleboxOverride(state, new StyleBoxEmpty());
        button.Pressed += Flip;
        AddChild(button);
        return button;
    }

    public void Flip()
    {
        IsFlipped = !IsFlipped;
        _artButton.Visible = true;
        FlipButton.Text = IsFlipped ? L10n.Text("card.front") : L10n.Text("card.back");
        QueueRedraw();
    }

    public void ShowFront()
    {
        if (IsFlipped) Flip();
    }

    public override void _Notification(int what)
    {
        if (what != NotificationPredelete) return;
        _titleFont.Dispose(); _bodyFont.Dispose(); _uiFont.Dispose();
        if (!_ownsVectors) return;
        _ownsVectors = false;
        if (--_vectorOwners != 0) return;
        foreach (var texture in Vectors.Values) texture.Dispose();
        Vectors.Clear();
    }

    public override void _Draw()
    {
        if (_frame is null) return;
        var face = Content;
        DrawTextureRect(_frame, new Rect2(Vector2.Zero, DesignSize), false);
        var titleWidth = _titleFont.GetStringSize(face.Title, fontSize: 33).X;
        DrawString(_titleFont, new Vector2((300 - titleWidth) * .5f, 43), face.Title, fontSize: 33, modulate: new Color("ece7d6"));
        if (!IsFlipped)
        {
            DrawTextureRect(_illustration, new Rect2(50, 79, 200, 200), false);
            DrawString(_bodyFont, new Vector2(23, 333), face.Effect, fontSize: 20, modulate: new Color("142a3b"));
        }
        else
        {
            DrawTextureRect(_illustration, new Rect2(29, 84, 42, 42), false);
            DrawString(_bodyFont, new Vector2(29, 157), face.Heading, fontSize: 21, modulate: new Color("e8e0ca"));
            for (var i = 0; i < face.Lines.Length; i++)
                DrawString(_bodyFont, new Vector2(29, 190 + i * 26), face.Lines[i], fontSize: 18, modulate: new Color("d2e0e4"));
            DrawString(_bodyFont, new Vector2(23, 333), face.Summary, fontSize: 19, modulate: new Color("142a3b"));
        }
    }

    private static Texture2D Vector(string name)
    {
        if (Vectors.TryGetValue(name, out var texture)) return texture;
        using var image = new Image();
        var error = image.LoadSvgFromString(Godot.FileAccess.GetFileAsString($"res://assets/cards-v12/{name}.svg"), 2);
        if (error != Error.Ok) throw new InvalidOperationException($"Cannot load card {name}: {error}");
        texture = ImageTexture.CreateFromImage(image);
        Vectors.Add(name, texture);
        return texture;
    }
}
