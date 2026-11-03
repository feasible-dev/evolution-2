using Godot;
using System.Collections.Generic;
using EvoPrototype.Evolution;

namespace EvoPrototype.Core;

public sealed class SimulationSettings
{
    public Vector2 WorldSize { get; init; } = new(1280.0f, 720.0f);

    public BoardShapeKind BoardShape { get; init; } = BoardShapeKind.Square;

    public ObstacleLayoutKind ObstacleLayout { get; init; } = ObstacleLayoutKind.None;

    public IReadOnlyList<BoardObstacle> Obstacles { get; init; } = System.Array.Empty<BoardObstacle>();

    public int AgentCount { get; init; } = 200;

    public float AgentRadius { get; init; } = 4.0f;

    public float AgentAgentCollisionRadiusFactor { get; init; } = 0.1f;

    public float MaxSpeed { get; init; } = 110.0f;

    public float Acceleration { get; init; } = 220.0f;

    public float Drag { get; init; } = 0.35f;

    public float WallAvoidanceDistance { get; init; } = 60.0f;

    public float NeighborAwarenessDistance { get; init; } = 32.0f;

    public float StuckSpeedThreshold { get; init; } = 8.0f;

    public float MinimumDecisionIntervalSeconds { get; init; } = 0.06f;

    public float LowSpeedReconsiderationSeconds { get; init; } = 0.18f;

    public float NeighborBrakeDistance { get; init; } = 18.0f;

    public int OverlapSolverIterations { get; init; } = 4;

    public bool UseAdaptiveAgentCollisions { get; init; }

    public GenePoolDefinition GenePool { get; init; } = GenePoolDefinition.Default;

    public float GenerationDurationSeconds { get; init; } = 10.0f;

    public RuleMutationProfile RuleMutationProfile { get; init; } = new();

    public float? FixedTargetAcceleration { get; init; }

    public float? FixedExplorationWeight { get; init; }

    public float BaseRandomJerkRuleWeight { get; init; } = 1.0f;

    public int BaseRandomJerkFadeGenerations { get; init; } = 50;

    public bool ContinuousBaselineBrownianMotion { get; init; }
}
