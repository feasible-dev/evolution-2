using System.Collections.Generic;
using Godot;

namespace EvoPrototype.Evolution;

/// <summary>
/// Builds reusable geometric survival regions from high-level layout parameters.
/// </summary>
public static class SurvivalRegionFactory
{
    public static IReadOnlyList<SurvivalRegion> CreateBorderBands(Vector2 worldSize, float thickness)
    {
        return new[]
        {
            new SurvivalRegion(new Rect2(0.0f, 0.0f, thickness, worldSize.Y)),
            new SurvivalRegion(new Rect2(worldSize.X - thickness, 0.0f, thickness, worldSize.Y)),
            new SurvivalRegion(new Rect2(0.0f, 0.0f, worldSize.X, thickness)),
            new SurvivalRegion(new Rect2(0.0f, worldSize.Y - thickness, worldSize.X, thickness))
        };
    }
}
