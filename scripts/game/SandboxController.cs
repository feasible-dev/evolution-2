using EvoPrototype.Core;
using EvoPrototype.Rendering;
using Godot;

namespace EvoPrototype.Game;

public partial class SandboxController : Node2D
{
    private readonly RunConfiguration _runConfiguration = new();
    private Simulation? _simulation;
    private SandboxRenderer? _sandboxRenderer;

    public override void _Ready()
    {
        _sandboxRenderer = GetNode<SandboxRenderer>("SandboxRenderer");

        _simulation = new Simulation(_runConfiguration.ToSimulationSettings());
        _simulation.InitializeAgents();

        _sandboxRenderer.Bind(_simulation);
    }

    public override void _Process(double delta)
    {
        if (_simulation is null || _sandboxRenderer is null)
        {
            return;
        }

        _simulation.Update((float)delta);
        _sandboxRenderer.QueueRedraw();
    }
}
