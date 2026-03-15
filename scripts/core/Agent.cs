using Godot;

namespace EvoPrototype.Core;

public sealed class Agent
{
    public Vector2 Position { get; set; }

    public Vector2 Velocity { get; set; }

    public float Energy { get; set; }

    public Color Color { get; set; } = Colors.White;
}
