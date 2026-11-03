using Godot;

namespace EvoPrototype.Evolution;

/// <summary>
/// Describes the active survival-zone layout independently from the current tuning UI.
/// </summary>
public readonly record struct SurvivalZoneDefinition(
    SurvivalZoneLayout Layout,
    Vector2 CircleCenterNormalized,
    float CircleRadiusFraction,
    Vector2 RectangleCenterNormalized,
    Vector2 RectangleSizeNormalized,
    Vector2 FirstBarCenterNormalized,
    Vector2 FirstBarSizeNormalized,
    Vector2 SecondBarCenterNormalized,
    Vector2 SecondBarSizeNormalized,
    float BorderBandThickness)
{
    public static SurvivalZoneDefinition Default => new(
        Layout: SurvivalZoneLayout.CenterCircle,
        CircleCenterNormalized: new Vector2(0.5f, 0.5f),
        CircleRadiusFraction: 0.18f,
        RectangleCenterNormalized: new Vector2(0.5f, 0.5f),
        RectangleSizeNormalized: new Vector2(0.28f, 0.20f),
        FirstBarCenterNormalized: new Vector2(0.35f, 0.5f),
        FirstBarSizeNormalized: new Vector2(0.10f, 0.50f),
        SecondBarCenterNormalized: new Vector2(0.65f, 0.5f),
        SecondBarSizeNormalized: new Vector2(0.10f, 0.50f),
        BorderBandThickness: 12.0f);
}
