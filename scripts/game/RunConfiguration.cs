using EvoPrototype.Core;
using EvoPrototype.Evolution;
using Godot;

namespace EvoPrototype.Game;

public sealed class RunConfiguration
{
    public int AgentCount { get; set; } = 200;

    public Vector2 WorldSize { get; set; } = new(1280.0f, 720.0f);

    public Vector2 WorldOrigin { get; set; } = new(32.0f, 104.0f);

    public Vector2 ViewportSize { get; set; } = new(1680.0f, 900.0f);

    public float AgentRadius { get; set; } = 4.0f;

    public float MaxSpeed { get; set; } = 110.0f;

    public float Acceleration { get; set; } = 220.0f;

    public float Drag { get; set; } = 0.35f;

    public float WallAvoidanceDistance { get; set; } = 60.0f;

    public float NeighborAwarenessDistance { get; set; } = 32.0f;

    public float StuckSpeedThreshold { get; set; } = 8.0f;

    public float MinimumDecisionIntervalSeconds { get; set; } = 0.06f;

    public float LowSpeedReconsiderationSeconds { get; set; } = 0.18f;

    public float NeighborBrakeDistance { get; set; } = 18.0f;

    public float GenerationDurationSeconds { get; set; } = 20.0f;

    public float SelectionDurationSeconds { get; set; } = 1.5f;

    public float TimeScale { get; set; } = 1.0f;

    public float SurvivalBandThickness { get; set; } = 64.0f;

    public float MutationStrength { get; set; } = 0.12f;

    public SimulationSettings ToSimulationSettings()
    {
        return new SimulationSettings
        {
            WorldSize = WorldSize,
            AgentCount = AgentCount,
            AgentRadius = AgentRadius,
            MaxSpeed = MaxSpeed,
            Acceleration = Acceleration,
            Drag = Drag,
            WallAvoidanceDistance = WallAvoidanceDistance,
            NeighborAwarenessDistance = NeighborAwarenessDistance,
            StuckSpeedThreshold = StuckSpeedThreshold,
            MinimumDecisionIntervalSeconds = MinimumDecisionIntervalSeconds,
            LowSpeedReconsiderationSeconds = LowSpeedReconsiderationSeconds,
            NeighborBrakeDistance = NeighborBrakeDistance
        };
    }

    public EvolutionSettings ToEvolutionSettings()
    {
        return new EvolutionSettings
        {
            GenerationDurationSeconds = GenerationDurationSeconds,
            SelectionDurationSeconds = SelectionDurationSeconds,
            TargetPopulationSize = AgentCount,
            MutationStrength = MutationStrength,
            SurvivalBandThickness = SurvivalBandThickness
        };
    }
}
