using EvoPrototype.Core;
using EvoPrototype.Rendering;
using Godot;

namespace EvoPrototype.Game;

public partial class SandboxController : Node2D
{
    private readonly RunConfiguration _runConfiguration = new();
    private Simulation? _simulation;
    private SandboxRenderer? _sandboxRenderer;
    private Label? _timeValueLabel;
    private Label? _agentValueLabel;
    private Label? _energyValueLabel;
    private Label? _speedValueLabel;
    private SpinBox? _agentCountSpinBox;
    private SpinBox? _accelerationSpinBox;
    private SpinBox? _dragSpinBox;
    private SpinBox? _stuckSpeedSpinBox;
    private Button? _applyButton;
    private float _elapsedTimeSeconds;

    /// <summary>
    /// Wires the scene, syncs UI controls, and starts a fresh simulation run.
    /// </summary>
    public override void _Ready()
    {
        _sandboxRenderer = GetNode<SandboxRenderer>("SandboxRenderer");
        _timeValueLabel = GetNode<Label>("CanvasLayer/UiRoot/TopBar/StatsRow/TimeValue");
        _agentValueLabel = GetNode<Label>("CanvasLayer/UiRoot/TopBar/StatsRow/AgentValue");
        _energyValueLabel = GetNode<Label>("CanvasLayer/UiRoot/RightPanel/PanelMargin/PanelStack/Metrics/EnergyValue");
        _speedValueLabel = GetNode<Label>("CanvasLayer/UiRoot/RightPanel/PanelMargin/PanelStack/Metrics/SpeedValue");
        _agentCountSpinBox = GetNode<SpinBox>("CanvasLayer/UiRoot/RightPanel/PanelMargin/PanelStack/Controls/AgentCountEditor");
        _accelerationSpinBox = GetNode<SpinBox>("CanvasLayer/UiRoot/RightPanel/PanelMargin/PanelStack/Controls/AccelerationEditor");
        _dragSpinBox = GetNode<SpinBox>("CanvasLayer/UiRoot/RightPanel/PanelMargin/PanelStack/Controls/DragEditor");
        _stuckSpeedSpinBox = GetNode<SpinBox>("CanvasLayer/UiRoot/RightPanel/PanelMargin/PanelStack/Controls/StuckSpeedEditor");
        _applyButton = GetNode<Button>("CanvasLayer/UiRoot/RightPanel/PanelMargin/PanelStack/ApplyButton");
        _applyButton.Pressed += OnApplyPressed;
        SyncEditorsFromConfiguration();
        ApplyWindowSize();
        ResetSimulation();
    }

    public override void _ExitTree()
    {
        if (_applyButton is not null)
        {
            _applyButton.Pressed -= OnApplyPressed;
        }
    }

    public override void _Process(double delta)
    {
        if (_simulation is null || _sandboxRenderer is null)
        {
            return;
        }

        _elapsedTimeSeconds += (float)delta;
        _simulation.Update((float)delta);
        _sandboxRenderer.QueueRedraw();
        UpdateHud();
    }

    private void ResetSimulation()
    {
        _elapsedTimeSeconds = 0.0f;
        _simulation = new Simulation(_runConfiguration.ToSimulationSettings());
        _simulation.InitializeAgents();
        _sandboxRenderer!.Bind(_simulation, _runConfiguration);
        UpdateHud();
    }

    private void OnApplyPressed()
    {
        _runConfiguration.AgentCount = (int)_agentCountSpinBox!.Value;
        _runConfiguration.Acceleration = (float)_accelerationSpinBox!.Value;
        _runConfiguration.Drag = (float)_dragSpinBox!.Value;
        _runConfiguration.StuckSpeedThreshold = (float)_stuckSpeedSpinBox!.Value;
        ResetSimulation();
    }

    private void SyncEditorsFromConfiguration()
    {
        _agentCountSpinBox!.Value = _runConfiguration.AgentCount;
        _accelerationSpinBox!.Value = _runConfiguration.Acceleration;
        _dragSpinBox!.Value = _runConfiguration.Drag;
        _stuckSpeedSpinBox!.Value = _runConfiguration.StuckSpeedThreshold;
    }

    private void UpdateHud()
    {
        if (_simulation is null)
        {
            return;
        }

        var agentCount = _simulation.WorldState.Agents.Count;
        var totalEnergy = 0.0f;
        var totalSpeed = 0.0f;

        foreach (var agent in _simulation.WorldState.Agents)
        {
            totalEnergy += agent.Energy;
            totalSpeed += agent.Velocity.Length();
        }

        var averageEnergy = agentCount > 0 ? totalEnergy / agentCount : 0.0f;
        var averageSpeed = agentCount > 0 ? totalSpeed / agentCount : 0.0f;

        _timeValueLabel!.Text = $"{_elapsedTimeSeconds:0.0}s";
        _agentValueLabel!.Text = agentCount.ToString();
        _energyValueLabel!.Text = $"{averageEnergy:0.0}";
        _speedValueLabel!.Text = $"{averageSpeed:0.0}";
    }

    private void ApplyWindowSize()
    {
        DisplayServer.WindowSetSize(new Vector2I(
            Mathf.RoundToInt(_runConfiguration.ViewportSize.X),
            Mathf.RoundToInt(_runConfiguration.ViewportSize.Y)));
    }
}
