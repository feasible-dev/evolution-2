using Godot;

namespace EvoPrototype.Core;

public readonly record struct AgentGenome(
    float BaseThrottle,
    float PreferredSpeedFraction,
    float BaseTurnRate,
    float WallAvoidanceWeight,
    float NeighborAvoidanceWeight,
    float DecisionIntervalSeconds,
    float DecisionIntervalJitterSeconds,
    float WanderAmplitude,
    float WanderFrequency,
    float StuckTurnBias,
    float RandomTurnStrength,
    float RandomThrottleBias,
    Vector2 TargetPointNormalized,
    float TargetPointWeight,
    GeneProgram Program)
{
    public static AgentGenome Default => new(
        BaseThrottle: 0.8f,
        PreferredSpeedFraction: 0.7f,
        BaseTurnRate: 0.0f,
        WallAvoidanceWeight: 1.2f,
        NeighborAvoidanceWeight: 0.8f,
        DecisionIntervalSeconds: 0.2f,
        DecisionIntervalJitterSeconds: 0.08f,
        WanderAmplitude: 0.6f,
        WanderFrequency: 1.3f,
        StuckTurnBias: 2.4f,
        RandomTurnStrength: 0.35f,
        RandomThrottleBias: 0.12f,
        TargetPointNormalized: new Vector2(0.5f, 0.5f),
        TargetPointWeight: 0.8f,
        Program: GeneProgram.Empty);
}
