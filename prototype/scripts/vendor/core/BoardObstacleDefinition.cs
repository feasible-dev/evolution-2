using Godot;

namespace EvoPrototype.Core;

/// <summary>
/// Editable obstacle definition stored in normalized board space for setup mode.
/// </summary>
public readonly record struct BoardObstacleDefinition(
    BoardObstacleKind Kind,
    Vector2 CenterNormalized,
    Vector2 SizeNormalized,
    float RadiusFraction)
{
    public static BoardObstacleDefinition Circle(Vector2 centerNormalized, float radiusFraction)
    {
        return new BoardObstacleDefinition(BoardObstacleKind.Circle, centerNormalized, Vector2.Zero, radiusFraction);
    }

    public static BoardObstacleDefinition Rectangle(Vector2 centerNormalized, Vector2 sizeNormalized)
    {
        return new BoardObstacleDefinition(BoardObstacleKind.Rectangle, centerNormalized, sizeNormalized, 0.0f);
    }
}
