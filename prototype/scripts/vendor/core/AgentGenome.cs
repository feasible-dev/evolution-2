using Godot;

namespace EvoPrototype.Core;

public readonly record struct AgentGenome(
    float DecisionIntervalSeconds,
    float DecisionIntervalJitterSeconds,
    GeneProgram Program)
{
    public static AgentGenome Default => new(
        DecisionIntervalSeconds: 0.2f,
        DecisionIntervalJitterSeconds: 0.08f,
        Program: GeneProgram.Empty);
}
