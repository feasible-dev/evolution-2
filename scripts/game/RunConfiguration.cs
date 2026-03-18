using EvoPrototype.Core;
using EvoPrototype.Evolution;
using Godot;

namespace EvoPrototype.Game;

public sealed class RunConfiguration
{
    public int AgentCount { get; set; } = 200;

    public Vector2 WorldSize { get; set; } = new(1280.0f, 720.0f);

    public Vector2 WorldOrigin { get; set; } = new(344.0f, 104.0f);

    public Vector2 ViewportSize { get; set; } = new(1980.0f, 1240.0f);

    public float AgentRadius { get; set; } = 4.0f;

    public float MaxSpeed { get; set; } = 110.0f;

    public float Acceleration { get; set; } = 220.0f;

    public float Drag { get; set; } = 0.08f;

    public float WallAvoidanceDistance { get; set; } = 60.0f;

    public float NeighborAwarenessDistance { get; set; } = 32.0f;

    public float StuckSpeedThreshold { get; set; } = 8.0f;

    public float MinimumDecisionIntervalSeconds { get; set; } = 0.06f;

    public float LowSpeedReconsiderationSeconds { get; set; } = 0.18f;

    public float NeighborBrakeDistance { get; set; } = 18.0f;

    public float GenerationDurationSeconds { get; set; } = 10.0f;

    public float SelectionDurationSeconds { get; set; } = 1.5f;

    public float TimeScale { get; set; } = 8.0f;

    public float MutationStrength { get; set; } = 0.03f;

    public SurvivalZoneDefinition SurvivalZoneDefinition { get; set; } = SurvivalZoneDefinition.Default;

    public GenePoolDefinition GenePool { get; set; } = new(
        AllowTimeCondition: false,
        AllowTurnBiasAction: false,
        AllowThrottleBiasAction: false,
        AllowSteerToTargetAction: false,
        MaxRulesPerAgent: 1);

    public int TrailPointCount { get; set; } = 10;

    public float TrailAlpha { get; set; } = 0.0f;

    public RunConfiguration Clone()
    {
        return new RunConfiguration
        {
            AgentCount = AgentCount,
            WorldSize = WorldSize,
            WorldOrigin = WorldOrigin,
            ViewportSize = ViewportSize,
            AgentRadius = AgentRadius,
            MaxSpeed = MaxSpeed,
            Acceleration = Acceleration,
            Drag = Drag,
            WallAvoidanceDistance = WallAvoidanceDistance,
            NeighborAwarenessDistance = NeighborAwarenessDistance,
            StuckSpeedThreshold = StuckSpeedThreshold,
            MinimumDecisionIntervalSeconds = MinimumDecisionIntervalSeconds,
            LowSpeedReconsiderationSeconds = LowSpeedReconsiderationSeconds,
            NeighborBrakeDistance = NeighborBrakeDistance,
            GenerationDurationSeconds = GenerationDurationSeconds,
            SelectionDurationSeconds = SelectionDurationSeconds,
            TimeScale = TimeScale,
            MutationStrength = MutationStrength,
            SurvivalZoneDefinition = SurvivalZoneDefinition,
            GenePool = GenePool,
            TrailPointCount = TrailPointCount,
            TrailAlpha = TrailAlpha
        };
    }

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
            NeighborBrakeDistance = NeighborBrakeDistance,
            GenePool = GenePool,
            GenerationDurationSeconds = GenerationDurationSeconds
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
            SurvivalZoneDefinition = SurvivalZoneDefinition
        };
    }
}
