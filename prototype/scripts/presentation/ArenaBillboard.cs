using Godot;

namespace EvolutionTwo.Presentation;

/// <summary>One uninterrupted physical matrix with a rotating broadcast program.</summary>
public partial class ArenaBillboard : Control
{
    public LevelOneSession Session { get; set; } = null!;
    public MarqueeProgram Program { get; } = new();
    public float RedImpact => Program.Impact;
    public TickerStrip Strip { get; private set; } = null!;
    private int _revision = -1;
    private int _sequence = -1;

    public override void _Ready()
    {
        MouseFilter = MouseFilterEnum.Ignore;
        Strip = new TickerStrip();
        AddChild(Strip);
        Resized += Arrange;
        Arrange();
    }

    private void Arrange()
    {
        Strip.Position = new Vector2(48, 26);
        Strip.Size = new Vector2(MathF.Max(1, Size.X - 96), MathF.Max(1, Size.Y - 34));
        QueueRedraw();
    }

    public override void _Draw()
    {
        // The desk sits above the sign; its front lip also covers the old sprite desk pixels.
        DrawRect(new Rect2(Size.X / 2 - 194, -10, 388, 14), new Color("171c20"));
        DrawLine(new Vector2(Size.X / 2 - 194, -10), new Vector2(Size.X / 2 + 194, -10), new Color("879194"), 2);
        DrawRect(new Rect2(0, 0, Size.X, 13), new Color("1a1c1d"));
        for (var x = 4f; x < Size.X - 6; x += 8)
            DrawLine(new Vector2(x, 3), new Vector2(x + 4, 10), new Color("343a3b"), 1);
        DrawLine(new Vector2(0, 0), new Vector2(Size.X, 0), new Color("646762"), 2);
        DrawLine(new Vector2(0, 13), new Vector2(Size.X, 13), new Color("4b443a"), 2);
        foreach (var x in new[] { Size.X * .27f, Size.X * .73f })
        {
            DrawLine(new Vector2(x, -4), new Vector2(x, 25), new Color("92938a"), 4);
            DrawLine(new Vector2(x + 1, -4), new Vector2(x + 1, 25), new Color("303738"), 2);
            DrawCircle(new Vector2(x, 3), 3, new Color("b4b19e"));
        }
        using var shell = new StyleBoxFlat { BgColor = new Color("333632"), BorderColor = new Color("88867a"),
            BorderWidthTop = 2, BorderWidthLeft = 2, BorderWidthRight = 2, BorderWidthBottom = 3,
            CornerRadiusTopLeft = 10, CornerRadiusTopRight = 10, CornerRadiusBottomLeft = 10, CornerRadiusBottomRight = 10,
            ShadowColor = Colors.Black, ShadowSize = 3, ShadowOffset = new Vector2(0, 2) };
        DrawStyleBox(shell, new Rect2(32, 20, Size.X - 64, Size.Y - 22));
        using var recess = new StyleBoxFlat { BgColor = Colors.Black, BorderColor = new Color("53584f"),
            BorderWidthTop = 1, BorderWidthLeft = 1, BorderWidthRight = 1, BorderWidthBottom = 1,
            CornerRadiusTopLeft = 4, CornerRadiusTopRight = 4, CornerRadiusBottomLeft = 4, CornerRadiusBottomRight = 4 };
        DrawStyleBox(recess, new Rect2(44, 24, Size.X - 88, Size.Y - 30));
        foreach (var x in new[] { 38f, Size.X - 39 })
        {
            BroadcastHardware.Bolt(this, new Vector2(x, 29));
            BroadcastHardware.Bolt(this, new Vector2(x, Size.Y - 11));
        }
    }

    public override void _Process(double delta)
    {
        Program.Update(delta, Session, Strip.Size.X);
        if (_revision != Program.Revision)
        {
            _revision = Program.Revision;
            Strip.SetCue(Program.Cue, _sequence != Program.Sequence);
            _sequence = Program.Sequence;
        }
        Strip.Impact = Program.Impact;
    }
}
