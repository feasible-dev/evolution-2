using Godot;
using static EvolutionTwo.Presentation.BroadcastUi;

namespace EvolutionTwo.Presentation;

public partial class ArenaBillboard : Control
{
    public LevelOneSession Session { get; set; } = null!;
    private TickerStrip _left = null!, _right = null!;
    private Label _score = null!, _caption = null!;

    public override void _Ready()
    {
        MouseFilter = MouseFilterEnum.Ignore;
        _left = new TickerStrip { Message = L10n.Text("billboard.audience") };
        _right = new TickerStrip();
        AddChild(_left); AddChild(_right);
        _caption = Text(this, L10n.Text("billboard.caption"), 9, Muted);
        _score = Text(this, "", 16, Orange);
        _caption.HorizontalAlignment = _score.HorizontalAlignment = HorizontalAlignment.Center;
    }

    public override void _Process(double delta)
    {
        var centerWidth = MathF.Min(270, Size.X * .32f);
        var leftWidth = (Size.X - centerWidth) / 2;
        Place(_left, 10, 0, leftWidth - 20, Size.Y);
        Place(_right, leftWidth + centerWidth + 10, 0, leftWidth - 20, Size.Y);
        Place(_caption, leftWidth, 1, centerWidth, 13);
        Place(_score, leftWidth, 14, centerWidth, Size.Y - 15);
        var survival = Session.LastResult is { } last ? (last.SurvivalRate * 100).ToString("0.0", L10n.Culture) + "%" : "—";
        var opponent = Session.Opponent!.LastResult is { } rival ? (rival.SurvivalRate * 100).ToString("0.0", L10n.Culture) + "%" : "—";
        _score.Text = L10n.Text("billboard.duel_score", Session.Completed, opponent, survival);
        _right.Message = Session.Speech;
        QueueRedraw();
    }

    public override void _Draw()
    {
        DrawRect(new Rect2(Vector2.Zero, Size), new Color("071929"));
        DrawLine(Vector2.Zero, new Vector2(Size.X, 0), Accent, 1);
        DrawLine(new Vector2(0, Size.Y - 1), Size - Vector2.One, new Color("29445d"), 1);
        var centerWidth = MathF.Min(270, Size.X * .32f);
        var x = (Size.X - centerWidth) / 2;
        DrawRect(new Rect2(x, 0, centerWidth, Size.Y), new Color("102b42"));
        for (var y = 2; y < Size.Y; y += 4)
            DrawLine(new Vector2(0, y), new Vector2(Size.X, y), new Color(0, 0, 0, .12f));
        DrawLine(new Vector2(x, 0), new Vector2(x, Size.Y), Accent, 1);
        DrawLine(new Vector2(x + centerWidth, 0), new Vector2(x + centerWidth, Size.Y), Accent, 1);
    }
}
