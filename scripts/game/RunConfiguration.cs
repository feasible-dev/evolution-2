using Godot;

namespace EvoPrototype.Game;

public sealed class RunConfiguration
{
    public int AgentCount { get; init; } = 200;

    public Vector2 WorldSize { get; init; } = new(1280.0f, 720.0f);

    public float AgentRadius { get; init; } = 4.0f;
}
