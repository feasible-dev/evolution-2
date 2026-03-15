namespace EvoPrototype.Core;

public readonly record struct AgentGenome(
    float BaseThrottle,
    float BaseTurnRate,
    float WallAvoidanceWeight,
    float NeighborAvoidanceWeight,
    float DecisionIntervalSeconds,
    float DecisionIntervalJitterSeconds,
    float WanderAmplitude,
    float WanderFrequency,
    float StuckTurnBias)
{
    public static AgentGenome Default => new(
        BaseThrottle: 0.8f,
        BaseTurnRate: 0.0f,
        WallAvoidanceWeight: 1.2f,
        NeighborAvoidanceWeight: 0.8f,
        DecisionIntervalSeconds: 0.2f,
        DecisionIntervalJitterSeconds: 0.08f,
        WanderAmplitude: 0.6f,
        WanderFrequency: 1.3f,
        StuckTurnBias: 2.4f);
}
