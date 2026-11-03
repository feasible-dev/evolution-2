using Godot;

namespace EvoPrototype.Core;

public readonly record struct BoardObstacle(
    BoardObstacleKind Kind,
    Vector2 Center,
    Vector2 Size,
    float Radius)
{
    public static BoardObstacle Circle(Vector2 center, float radius)
    {
        return new BoardObstacle(BoardObstacleKind.Circle, center, Vector2.Zero, radius);
    }

    public static BoardObstacle Rectangle(Vector2 center, Vector2 size)
    {
        return new BoardObstacle(BoardObstacleKind.Rectangle, center, size, 0.0f);
    }
}
