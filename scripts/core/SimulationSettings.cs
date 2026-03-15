using Godot;

namespace EvoPrototype.Core;

public sealed class SimulationSettings
{
    public Vector2 WorldSize { get; init; } = new(1280.0f, 720.0f);

    public int AgentCount { get; init; } = 200;

    public float AgentRadius { get; init; } = 4.0f;

    public float MaxSpeed { get; init; } = 110.0f;

    public float Acceleration { get; init; } = 220.0f;

    public float Drag { get; init; } = 0.35f;

    public float WallAvoidanceDistance { get; init; } = 60.0f;

    public float NeighborAwarenessDistance { get; init; } = 32.0f;

    public float StuckSpeedThreshold { get; init; } = 8.0f;

    public float MinimumDecisionIntervalSeconds { get; init; } = 0.06f;

    public float LowSpeedReconsiderationSeconds { get; init; } = 0.18f;
}
