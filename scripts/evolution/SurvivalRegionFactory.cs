using System.Collections.Generic;
using Godot;

namespace EvoPrototype.Evolution;

/// <summary>
/// Builds reusable geometric survival regions from high-level layout parameters.
/// </summary>
public static class SurvivalRegionFactory
{
    public static IReadOnlyList<ISurvivalZone> CreateZones(Vector2 worldSize, SurvivalZoneDefinition definition)
    {
        return definition.Layout switch
        {
            SurvivalZoneLayout.CenterCircle => CreateCenteredCircle(
                worldSize,
                new Vector2(
                    definition.CircleCenterNormalized.X * worldSize.X,
                    definition.CircleCenterNormalized.Y * worldSize.Y),
                Mathf.Min(worldSize.X, worldSize.Y) * definition.CircleRadiusFraction),
            SurvivalZoneLayout.BorderBands => CreateBorderBands(worldSize, definition.BorderBandThickness),
            _ => CreateCenteredCircle(worldSize, worldSize * 0.5f, Mathf.Min(worldSize.X, worldSize.Y) * 0.18f)
        };
    }

    public static IReadOnlyList<ISurvivalZone> CreateBorderBands(Vector2 worldSize, float thickness)
    {
        return new[]
        {
            (ISurvivalZone)new RectangleSurvivalZone(new Rect2(0.0f, 0.0f, thickness, worldSize.Y)),
            new RectangleSurvivalZone(new Rect2(worldSize.X - thickness, 0.0f, thickness, worldSize.Y)),
            new RectangleSurvivalZone(new Rect2(0.0f, 0.0f, worldSize.X, thickness)),
            new RectangleSurvivalZone(new Rect2(0.0f, worldSize.Y - thickness, worldSize.X, thickness))
        };
    }

    public static IReadOnlyList<ISurvivalZone> CreateCenteredCircle(Vector2 worldSize, float radius)
    {
        return CreateCenteredCircle(worldSize, worldSize * 0.5f, radius);
    }

    public static IReadOnlyList<ISurvivalZone> CreateCenteredCircle(Vector2 worldSize, Vector2 center, float radius)
    {
        return new[]
        {
            (ISurvivalZone)new CircleSurvivalZone(center, radius)
        };
    }
}
