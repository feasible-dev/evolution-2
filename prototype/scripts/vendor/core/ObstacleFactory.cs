using System;
using System.Collections.Generic;
using Godot;

namespace EvoPrototype.Core;

public static class ObstacleFactory
{
    public static IReadOnlyList<BoardObstacleDefinition> CreateDefinitions(ObstacleLayoutKind layout)
    {
        return layout switch
        {
            ObstacleLayoutKind.CentralDisk => new[]
            {
                BoardObstacleDefinition.Circle(new Vector2(0.5f, 0.5f), 0.10f)
            },
            ObstacleLayoutKind.SplitBar => new[]
            {
                BoardObstacleDefinition.Rectangle(new Vector2(0.5f, 0.5f), new Vector2(0.08f, 0.44f))
            },
            ObstacleLayoutKind.TwinBlocks => new[]
            {
                BoardObstacleDefinition.Rectangle(new Vector2(0.32f, 0.5f), new Vector2(0.14f, 0.14f)),
                BoardObstacleDefinition.Rectangle(new Vector2(0.68f, 0.5f), new Vector2(0.14f, 0.14f))
            },
            _ => Array.Empty<BoardObstacleDefinition>()
        };
    }

    public static IReadOnlyList<BoardObstacle> Create(Vector2 worldSize, ObstacleLayoutKind layout)
    {
        return Create(worldSize, CreateDefinitions(layout));
    }

    public static IReadOnlyList<BoardObstacle> Create(Vector2 worldSize, IReadOnlyList<BoardObstacleDefinition> definitions)
    {
        var obstacles = new BoardObstacle[definitions.Count];
        var minSide = Mathf.Min(worldSize.X, worldSize.Y);

        for (var index = 0; index < definitions.Count; index++)
        {
            var definition = definitions[index];
            var center = new Vector2(
                Mathf.Clamp(definition.CenterNormalized.X, 0.0f, 1.0f) * worldSize.X,
                Mathf.Clamp(definition.CenterNormalized.Y, 0.0f, 1.0f) * worldSize.Y);

            obstacles[index] = definition.Kind switch
            {
                BoardObstacleKind.Circle => BoardObstacle.Circle(center, Mathf.Clamp(definition.RadiusFraction, 0.02f, 0.45f) * minSide),
                BoardObstacleKind.Rectangle => BoardObstacle.Rectangle(
                    center,
                    new Vector2(
                        Mathf.Clamp(definition.SizeNormalized.X, 0.02f, 1.0f) * worldSize.X,
                        Mathf.Clamp(definition.SizeNormalized.Y, 0.02f, 1.0f) * worldSize.Y)),
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        return obstacles;
    }
}
