using Godot;

namespace EvoPrototype.Evolution;

public readonly record struct RectangleSurvivalZone(Rect2 Bounds) : ISurvivalZone
{
    public bool Contains(Vector2 position)
    {
        return Bounds.HasPoint(position);
    }

    public float GetSelectionScore(Vector2 position)
    {
        var center = Bounds.GetCenter();
        var half = Bounds.Size * 0.5f;
        if (half.X <= 0.0001f || half.Y <= 0.0001f)
        {
            return 0.0f;
        }

        var offset = (position - center).Abs();
        var normalized = new Vector2(
            Mathf.Clamp(offset.X / half.X, 0.0f, 1.0f),
            Mathf.Clamp(offset.Y / half.Y, 0.0f, 1.0f));
        return 1.0f - Mathf.Max(normalized.X, normalized.Y);
    }
}
