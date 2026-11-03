using Godot;

namespace EvoPrototype.Core;

public sealed class Agent
{
    public string AgentId { get; init; } = System.Guid.NewGuid().ToString("N");

    public string? ParentAgentId { get; set; }

    public bool HasParent { get; set; }

    public int GenerationNumber { get; set; }

    public int ParentChildCount { get; set; }

    public int LineageAge { get; set; }

    public required AgentGenome Genome { get; init; }

    public Vector2 Position { get; set; }

    public Vector2 SpawnOrigin { get; set; }

    public Vector2 ParentSpawnOrigin { get; set; }

    public float MaxDistanceFromBirth { get; set; }

    public Vector2 Velocity { get; set; }

    public float Heading { get; set; }

    public float Radius { get; set; }

    public float Energy { get; set; }

    public float DecisionTimer { get; set; }

    public float TimeSinceLastDecision { get; set; }

    public float TimeBelowSpeedThreshold { get; set; }

    public float ImmobileAxisXSeconds { get; set; }

    public float ImmobileAxisYSeconds { get; set; }

    public bool IsTouchingWall { get; set; }

    public float BrainClock { get; set; }

    public float DecisionTurnNoise { get; set; }

    public float DecisionThrottleNoise { get; set; }

    public required PhysicsProfile PhysicsProfile { get; set; }

    public AgentIntent CurrentIntent { get; set; } = AgentIntent.Neutral;

    public Color Color { get; set; } = Colors.White;
}
