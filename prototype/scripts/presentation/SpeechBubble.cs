using Godot;

namespace EvolutionTwo.Presentation;

public partial class SpeechBubble : Control
{
    public bool PointsRight { get; set; }
    private double _age = 100;
    private int _length;
    public static (Vector2 Size, int FontSize) Measure(Font font, string text, float maxWidth)
    {
        var size = 17;
        var width = Math.Clamp(font.GetStringSize(text, fontSize: size).X + 24, 72, maxWidth);
        var measured = font.GetMultilineStringSize(text, width: width - 24, fontSize: size);
        while (size > 12 && measured.Y > 112)
        {
            size--;
            measured = font.GetMultilineStringSize(text, width: width - 24, fontSize: size);
        }
        return (new Vector2(MathF.Ceiling(width), MathF.Ceiling(measured.Y) + 20), size);
    }
    public static double ReadingSeconds(int length) => Math.Max(4.5, length / 20.0 + 1.5);
    public static float Opacity(double age, int length)
    {
        var hold = ReadingSeconds(length);
        return age <= hold ? 1 : Math.Clamp(1 - (float)(age - hold) / .6f, 0, 1);
    }
    public override void _Ready()
    {
        MouseFilter = MouseFilterEnum.Ignore;
        Visible = false;
        Resized += QueueRedraw;
    }
    public void ShowLine(string line)
    {
        _age = 0; _length = line.Length;
        Modulate = Colors.White; Visible = _length > 0;
    }
    public void Clear() { _age = 100; _length = 0; Visible = false; }
    public override void _Process(double delta)
    {
        _age += delta;
        var alpha = Opacity(_age, _length);
        Modulate = new Color(1, 1, 1, alpha);
        Visible = _length > 0 && alpha > 0;
    }
    public override void _Draw()
    {
        var fill = new Color("e7e2d4");
        using var style = new StyleBoxFlat { BgColor = fill, BorderColor = new Color("6d736e"),
            BorderWidthLeft = 1, BorderWidthRight = 1, BorderWidthTop = 1, BorderWidthBottom = 2,
            CornerRadiusTopLeft = 12, CornerRadiusTopRight = 12, CornerRadiusBottomLeft = 12, CornerRadiusBottomRight = 12,
            ShadowColor = new Color(0, 0, 0, .6f), ShadowSize = 3, ShadowOffset = new Vector2(0, 2) };
        DrawStyleBox(style, new Rect2(Vector2.Zero, Size));
        var x = PointsRight ? Size.X - 1 : 1;
        var sign = PointsRight ? 1 : -1;
        var y = MathF.Min(Size.Y - 26, Size.Y * .65f);
        DrawColoredPolygon(new[] { new Vector2(x, y - 9), new Vector2(x + sign * 13, y + 8), new Vector2(x, y + 9) }, fill);
    }
}
