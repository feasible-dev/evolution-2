using Godot;

namespace EvolutionTwo.Presentation;

public partial class TickerStrip : Control
{
    public string Message { get; set; } = "";
    private double _time;

    public override void _Ready()
    {
        ClipContents = true;
        MouseFilter = MouseFilterEnum.Ignore;
    }

    public override void _Process(double delta) { _time += delta; QueueRedraw(); }

    public override void _Draw()
    {
        var font = GetThemeDefaultFont();
        var text = Message + "   •   ";
        var width = MathF.Max(1, font.GetStringSize(text, fontSize: 13).X);
        var offset = (float)(_time * 28 % width);
        var start = -offset;
        for (var x = start; x < Size.X; x += width)
            DrawString(font, new Vector2(x, (Size.Y + 13) / 2 - 2), text, fontSize: 13, modulate: BroadcastUi.Paper);
    }
}
