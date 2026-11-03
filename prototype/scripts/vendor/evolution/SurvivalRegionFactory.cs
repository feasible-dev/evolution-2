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
            SurvivalZoneLayout.Rectangle => CreateRectangle(
                worldSize,
                definition.RectangleCenterNormalized,
                definition.RectangleSizeNormalized),
            SurvivalZoneLayout.TwinBars => CreateTwinBars(
                worldSize,
                definition.FirstBarCenterNormalized,
                definition.FirstBarSizeNormalized,
                definition.SecondBarCenterNormalized,
                definition.SecondBarSizeNormalized),
            SurvivalZoneLayout.BorderBands => CreateBorderBands(worldSize, definition.BorderBandThickness),
            _ => CreateCenteredCircle(worldSize, worldSize * 0.5f, Mathf.Min(worldSize.X, worldSize.Y) * 0.18f)
        };
    }

    public static IReadOnlyList<ISurvivalZone> CreateRectangle(
        Vector2 worldSize,
        Vector2 centerNormalized,
        Vector2 sizeNormalized)
    {
        var size = new Vector2(
            Mathf.Clamp(sizeNormalized.X, 0.02f, 1.0f) * worldSize.X,
            Mathf.Clamp(sizeNormalized.Y, 0.02f, 1.0f) * worldSize.Y);
        var center = new Vector2(
            Mathf.Clamp(centerNormalized.X, 0.0f, 1.0f) * worldSize.X,
            Mathf.Clamp(centerNormalized.Y, 0.0f, 1.0f) * worldSize.Y);
        var rect = new Rect2(center - (size * 0.5f), size);

        return new[]
        {
            (ISurvivalZone)new RectangleSurvivalZone(rect)
        };
    }

    public static IReadOnlyList<ISurvivalZone> CreateTwinBars(
        Vector2 worldSize,
        Vector2 firstCenterNormalized,
        Vector2 firstSizeNormalized,
        Vector2 secondCenterNormalized,
        Vector2 secondSizeNormalized)
    {
        return new[]
        {
            (ISurvivalZone)new RectangleSurvivalZone(CreateNormalizedRect(worldSize, firstCenterNormalized, firstSizeNormalized)),
            new RectangleSurvivalZone(CreateNormalizedRect(worldSize, secondCenterNormalized, secondSizeNormalized))
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

    private static Rect2 CreateNormalizedRect(Vector2 worldSize, Vector2 centerNormalized, Vector2 sizeNormalized)
    {
        var size = new Vector2(
            Mathf.Clamp(sizeNormalized.X, 0.02f, 1.0f) * worldSize.X,
            Mathf.Clamp(sizeNormalized.Y, 0.02f, 1.0f) * worldSize.Y);
        var center = new Vector2(
            Mathf.Clamp(centerNormalized.X, 0.0f, 1.0f) * worldSize.X,
            Mathf.Clamp(centerNormalized.Y, 0.0f, 1.0f) * worldSize.Y);
        return new Rect2(center - (size * 0.5f), size);
    }
}
