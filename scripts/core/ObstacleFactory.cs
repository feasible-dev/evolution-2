using System;
using System.Collections.Generic;
using Godot;

namespace EvoPrototype.Core;

public static class ObstacleFactory
{
    public static IReadOnlyList<BoardObstacle> Create(Vector2 worldSize, ObstacleLayoutKind layout)
    {
        return layout switch
        {
            ObstacleLayoutKind.CentralDisk => new[]
            {
                BoardObstacle.Circle(worldSize * 0.5f, Mathf.Min(worldSize.X, worldSize.Y) * 0.10f)
            },
            ObstacleLayoutKind.SplitBar => new[]
            {
                BoardObstacle.Rectangle(
                    worldSize * 0.5f,
                    new Vector2(Mathf.Min(worldSize.X, worldSize.Y) * 0.08f, worldSize.Y * 0.44f))
            },
            ObstacleLayoutKind.TwinBlocks => new[]
            {
                BoardObstacle.Rectangle(
                    new Vector2(worldSize.X * 0.32f, worldSize.Y * 0.5f),
                    new Vector2(worldSize.X * 0.14f, worldSize.Y * 0.14f)),
                BoardObstacle.Rectangle(
                    new Vector2(worldSize.X * 0.68f, worldSize.Y * 0.5f),
                    new Vector2(worldSize.X * 0.14f, worldSize.Y * 0.14f))
            },
            _ => Array.Empty<BoardObstacle>()
        };
    }
}
