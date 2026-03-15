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

        DrawRect(
            new Rect2(Vector2.Zero, _simulation.WorldState.Size),
            new Color(0.08f, 0.1f, 0.12f),
            filled: true);

        foreach (var agent in _simulation.WorldState.Agents)
        {
            DrawCircle(agent.Position, _runConfiguration.AgentRadius, agent.Color);
        }
    }
}
