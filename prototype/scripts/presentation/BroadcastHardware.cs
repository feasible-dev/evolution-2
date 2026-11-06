using Godot;

namespace EvolutionTwo.Presentation;

/// <summary>Painted steel and simple fasteners shared by the broadcast hardware.</summary>
internal static class BroadcastHardware
{
    public static readonly Color Coal = new("181916");
    public static readonly Color Steel = new("393c37");
    public static readonly Color Edge = new("77796a");
    public static readonly Color Paint = new("f58b23");
    public static readonly Color Ink = new("171710");
    public static readonly Color Cream = new("f4e6be");

    public static void Bolt(Control canvas, Vector2 center)
    {
        canvas.DrawRect(new Rect2(center - new Vector2(3, 3), new Vector2(7, 7)), Ink);
        canvas.DrawRect(new Rect2(center - new Vector2(2, 2), new Vector2(5, 5)), Edge);
        canvas.DrawLine(center + new Vector2(-1, 1), center + new Vector2(1, -1), Ink, 1);
    }

    public static void Stripes(Control canvas, Rect2 area)
    {
        canvas.DrawRect(area, Ink);
        for (var x = area.Position.X; x + 10 <= area.End.X; x += 18)
            canvas.DrawColoredPolygon(new[] {
                new Vector2(x, area.End.Y), new Vector2(x + 5, area.Position.Y),
                new Vector2(x + 10, area.Position.Y), new Vector2(x + 5, area.End.Y)
            }, Paint);
    }
}
