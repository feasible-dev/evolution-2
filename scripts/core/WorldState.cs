using System.Collections.Generic;
using Godot;

namespace EvoPrototype.Core;

public sealed class WorldState
{
    public WorldState(Vector2 size)
    {
        Size = size;
    }

    public Vector2 Size { get; }

    public List<Agent> Agents { get; } = new();
}
