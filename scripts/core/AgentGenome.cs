namespace EvoPrototype.Core;

public readonly record struct AgentGenome(
    float BaseThrottle,
    float BaseTurnRate,
    float WallAvoidanceWeight,
    float NeighborAvoidanceWeight,
    float DecisionIntervalSeconds)
{
    public static AgentGenome Default => new(
        BaseThrottle: 0.8f,
        BaseTurnRate: 0.0f,
        WallAvoidanceWeight: 1.2f,
        NeighborAvoidanceWeight: 0.8f,
        DecisionIntervalSeconds: 0.2f);
}
