namespace EvoPrototype.Core;

/// <summary>
/// Describes which rule families may exist in the current sandbox or round.
/// Future cards will modify this pool instead of touching the simulation directly.
/// </summary>
public readonly record struct GenePoolDefinition(
    bool AllowTimeCondition,
    bool AllowTurnBiasAction,
    bool AllowThrottleBiasAction,
    bool AllowSteerToTargetAction,
    int MaxRulesPerAgent)
{
    public static GenePoolDefinition Default => new(
        AllowTimeCondition: true,
        AllowTurnBiasAction: true,
        AllowThrottleBiasAction: true,
        AllowSteerToTargetAction: true,
        MaxRulesPerAgent: 2);
}
