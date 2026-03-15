using Godot;

namespace EvoPrototype.Core;

public readonly record struct AgentIntent(
    Vector2 WorldAccelerationWish,
    Vector2 LocalAccelerationWish,
    float TurnWish,
    float BrakeWish)
{
    public static AgentIntent Neutral => new(Vector2.Zero, Vector2.Zero, 0.0f, 0.0f);
}
