using Godot;

namespace EvoPrototype.Evolution;

/// <summary>
/// Describes the active survival-zone layout independently from the current tuning UI.
/// </summary>
public readonly record struct SurvivalZoneDefinition(
    SurvivalZoneLayout Layout,
    Vector2 CircleCenterNormalized,
    float CircleRadiusFraction,
    float BorderBandThickness)
{
    public static SurvivalZoneDefinition Default => new(
        Layout: SurvivalZoneLayout.CenterCircle,
        CircleCenterNormalized: new Vector2(0.5f, 0.5f),
        CircleRadiusFraction: 0.18f,
        BorderBandThickness: 12.0f);
}
