namespace EvoPrototype.Core;

public readonly record struct SimulationFrameMetrics(
    float DecisionMs,
    float MovementMs,
    float OverlapMs,
    int DecisionAgents,
    int CheckedPairs,
    int ResolvedPairs,
    int CollisionActiveAgents,
    int CollisionSkippedAgents)
{
    public static SimulationFrameMetrics Empty => new(0.0f, 0.0f, 0.0f, 0, 0, 0, 0, 0);
}
