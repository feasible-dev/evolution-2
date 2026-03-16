using Godot;

namespace EvoPrototype.Evolution;

public readonly record struct CircleSurvivalZone(Vector2 Center, float Radius) : ISurvivalZone
{
    public bool Contains(Vector2 position)
    {
        return Center.DistanceSquaredTo(position) <= Radius * Radius;
    }
}
