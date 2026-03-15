using Godot;

namespace EvoPrototype.Evolution;

public readonly record struct SurvivalRegion(Rect2 Bounds)
{
    public bool Contains(Vector2 position)
    {
        return Bounds.HasPoint(position);
    }
}
