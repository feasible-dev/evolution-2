using Godot;

namespace EvoPrototype.Evolution;

public readonly record struct CircleSurvivalZone(Vector2 Center, float Radius) : ISurvivalZone
{
    public bool Contains(Vector2 position)
    {
        return Center.DistanceSquaredTo(position) <= Radius * Radius;
    }

    public float GetSelectionScore(Vector2 position)
    {
        if (Radius <= 0.0001f)
        {
            return 0.0f;
        }

        var normalizedDistance = Mathf.Clamp(position.DistanceTo(Center) / Radius, 0.0f, 1.0f);
        return 1.0f - normalizedDistance;
    }
}
