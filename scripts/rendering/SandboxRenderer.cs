using EvoPrototype.Core;
using EvoPrototype.Game;
using Godot;

namespace EvoPrototype.Rendering;

public partial class SandboxRenderer : Node2D
{
    private Simulation? _simulation;
    private RunConfiguration? _runConfiguration;

    public void Bind(Simulation simulation, RunConfiguration runConfiguration)
    {
        _simulation = simulation;
        _runConfiguration = runConfiguration;
        QueueRedraw();
    }

    public override void _Draw()
    {
        if (_simulation is null || _runConfiguration is null)
        {
            return;
        }

        var worldRect = new Rect2(_runConfiguration.WorldOrigin, _simulation.WorldState.Size);

        DrawRect(
            worldRect,
            new Color(0.08f, 0.1f, 0.12f),
            filled: true);

        DrawRect(worldRect, new Color(0.24f, 0.28f, 0.33f), filled: false, width: 2.0f);

        foreach (var agent in _simulation.WorldState.Agents)
        {
            DrawCircle(_runConfiguration.WorldOrigin + agent.Position, agent.Radius, agent.Color);
        }
    }
}
