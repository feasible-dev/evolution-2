namespace EvoPrototype.Core;

/// <summary>
/// Describes which rule families may exist in the current sandbox or round.
/// Future cards will modify this pool instead of touching the simulation directly.
/// </summary>
public readonly record struct GenePoolDefinition(
    bool AllowTimeCondition,
    bool AllowDistanceToPointCondition,
    bool AllowNeighborCountCondition,
    bool AllowHalfSpaceCondition,
    bool AllowTurnBiasAction,
    bool AllowThrottleBiasAction,
    bool AllowSteerToTargetAction,
    bool AllowBrakeAction,
    bool AllowRandomTurnAction,
    int MaxRulesPerAgent)
{
    public static GenePoolDefinition Default => new(
        AllowTimeCondition: true,
        AllowDistanceToPointCondition: true,
        AllowNeighborCountCondition: true,
        AllowHalfSpaceCondition: true,
        AllowTurnBiasAction: true,
        AllowThrottleBiasAction: true,
        AllowSteerToTargetAction: true,
        AllowBrakeAction: true,
        AllowRandomTurnAction: true,
        MaxRulesPerAgent: 2);
}
