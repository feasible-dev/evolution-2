using Godot;

namespace EvoPrototype.Evolution;

public readonly record struct RectangleSurvivalZone(Rect2 Bounds) : ISurvivalZone
{
    public bool Contains(Vector2 position)
    {
        return Bounds.HasPoint(position);
    }
}
