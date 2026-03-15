namespace EvoPrototype.Core;

public readonly record struct AgentDecision(float TurnRate, float Throttle)
{
    public static AgentDecision Neutral => new(0.0f, 0.0f);
}
