using Godot;

namespace EvoPrototype.Core;

public sealed class Agent
{
    public required AgentGenome Genome { get; init; }

    public Vector2 Position { get; set; }

    public Vector2 Velocity { get; set; }

    public float Heading { get; set; }

    public float Radius { get; set; }

    public float Energy { get; set; }

    public float DecisionTimer { get; set; }

    public float TimeSinceLastDecision { get; set; }

    public float TimeBelowSpeedThreshold { get; set; }

    public bool IsTouchingWall { get; set; }

    public float BrainClock { get; set; }

    public AgentDecision CurrentDecision { get; set; } = AgentDecision.Neutral;

    public Color Color { get; set; } = Colors.White;
}
