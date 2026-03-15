using System.Collections.Generic;
using EvoPrototype.Analytics;
using EvoPrototype.Core;
using EvoPrototype.Evolution;
using EvoPrototype.Rendering;
using EvoPrototype.Ui;
using Godot;

namespace EvoPrototype.Game;

public partial class SandboxController : Node2D
{
    private const float MaxSimulationStepSeconds = 1.0f / 30.0f;
    private readonly RunConfiguration _runConfiguration = new();
    private Simulation? _simulation;
    private EvolutionCycleController? _evolutionCycleController;
    private IReadOnlyList<SurvivalRegion>? _survivalRegions;
    private SandboxRenderer? _sandboxRenderer;
    private GenerationChart? _generationChart;
    private Label? _timeValueLabel;
    private Label? _generationValueLabel;
    private Label? _lifeValueLabel;
    private Label? _agentValueLabel;
    private Label? _survivorValueLabel;
    private Label? _phaseValueLabel;
    private Label? _energyValueLabel;
    private Label? _speedValueLabel;
    private SpinBox? _agentCountSpinBox;
    private SpinBox? _lifeDurationSpinBox;
    private SpinBox? _selectionDurationSpinBox;
    private SpinBox? _timeScaleSpinBox;
    private SpinBox? _mutationStrengthSpinBox;
    private SpinBox? _bandThicknessSpinBox;
    private SpinBox? _accelerationSpinBox;
    private SpinBox? _dragSpinBox;
    private SpinBox? _stuckSpeedSpinBox;
    private Button? _applyButton;
    private Button? _previousMetricButton;
    private Button? _nextMetricButton;
    private Label? _chartMetricValueLabel;
    private RunHistory _runHistory = new();
    private int _chartMetricIndex;
    private float _elapsedTimeSeconds;

    /// <summary>
    /// Wires the scene, syncs UI controls, and starts a fresh simulation run.
    /// </summary>
    public override void _Ready()
    {
        BindUiNodes();
        ConnectSignals();
        SyncEditorsFromConfiguration();
        ApplyWindowSize();
        ResetSimulation();
    }

    public override void _ExitTree()
    {
        DisconnectSignals();
    }

    public override void _Process(double delta)
    {
        if (_simulation is null || _sandboxRenderer is null || _evolutionCycleController is null)
        {
            return;
        }

        var scaledDelta = (float)delta * _runConfiguration.TimeScale;
        _elapsedTimeSeconds += scaledDelta;
        StepSimulation(scaledDelta);

        UpdateRendering();
        UpdateHud();
    }

    private void BindUiNodes()
    {
        _sandboxRenderer = GetNode<SandboxRenderer>("SandboxRenderer");
        _generationChart = GetNode<GenerationChart>("CanvasLayer/UiRoot/BottomPanel/BottomMargin/BottomStack/Chart");
        _timeValueLabel = GetNode<Label>("CanvasLayer/UiRoot/TopBar/StatsRow/TimeValue");
        _generationValueLabel = GetNode<Label>("CanvasLayer/UiRoot/TopBar/StatsRow/GenerationValue");
        _lifeValueLabel = GetNode<Label>("CanvasLayer/UiRoot/TopBar/StatsRow/LifeValue");
        _agentValueLabel = GetNode<Label>("CanvasLayer/UiRoot/TopBar/StatsRow/AgentValue");
        _survivorValueLabel = GetNode<Label>("CanvasLayer/UiRoot/RightPanel/PanelMargin/PanelStack/Metrics/SurvivorValue");
        _phaseValueLabel = GetNode<Label>("CanvasLayer/UiRoot/RightPanel/PanelMargin/PanelStack/Metrics/PhaseValue");
        _energyValueLabel = GetNode<Label>("CanvasLayer/UiRoot/RightPanel/PanelMargin/PanelStack/Metrics/EnergyValue");
        _speedValueLabel = GetNode<Label>("CanvasLayer/UiRoot/RightPanel/PanelMargin/PanelStack/Metrics/SpeedValue");
        _agentCountSpinBox = GetNode<SpinBox>("CanvasLayer/UiRoot/RightPanel/PanelMargin/PanelStack/EvolutionControls/AgentCountEditor");
        _lifeDurationSpinBox = GetNode<SpinBox>("CanvasLayer/UiRoot/RightPanel/PanelMargin/PanelStack/EvolutionControls/LifeDurationEditor");
        _selectionDurationSpinBox = GetNode<SpinBox>("CanvasLayer/UiRoot/RightPanel/PanelMargin/PanelStack/EvolutionControls/SelectionDurationEditor");
        _timeScaleSpinBox = GetNode<SpinBox>("CanvasLayer/UiRoot/RightPanel/PanelMargin/PanelStack/EvolutionControls/TimeScaleEditor");
        _mutationStrengthSpinBox = GetNode<SpinBox>("CanvasLayer/UiRoot/RightPanel/PanelMargin/PanelStack/EvolutionControls/MutationStrengthEditor");
        _bandThicknessSpinBox = GetNode<SpinBox>("CanvasLayer/UiRoot/RightPanel/PanelMargin/PanelStack/EvolutionControls/BandThicknessEditor");
        _accelerationSpinBox = GetNode<SpinBox>("CanvasLayer/UiRoot/RightPanel/PanelMargin/PanelStack/PhysicsControls/AccelerationEditor");
        _dragSpinBox = GetNode<SpinBox>("CanvasLayer/UiRoot/RightPanel/PanelMargin/PanelStack/PhysicsControls/DragEditor");
        _stuckSpeedSpinBox = GetNode<SpinBox>("CanvasLayer/UiRoot/RightPanel/PanelMargin/PanelStack/PhysicsControls/StuckSpeedEditor");
        _applyButton = GetNode<Button>("CanvasLayer/UiRoot/RightPanel/PanelMargin/PanelStack/ApplyButton");
        _previousMetricButton = GetNode<Button>("CanvasLayer/UiRoot/BottomPanel/BottomMargin/BottomStack/ChartHeader/PreviousMetricButton");
        _nextMetricButton = GetNode<Button>("CanvasLayer/UiRoot/BottomPanel/BottomMargin/BottomStack/ChartHeader/NextMetricButton");
        _chartMetricValueLabel = GetNode<Label>("CanvasLayer/UiRoot/BottomPanel/BottomMargin/BottomStack/ChartHeader/MetricValue");
    }

    private void ConnectSignals()
    {
        _applyButton!.Pressed += OnApplyPressed;
        _previousMetricButton!.Pressed += OnPreviousMetricPressed;
        _nextMetricButton!.Pressed += OnNextMetricPressed;
    }

    private void DisconnectSignals()
    {
        if (_applyButton is not null)
        {
            _applyButton.Pressed -= OnApplyPressed;
        }

        if (_previousMetricButton is not null)
        {
            _previousMetricButton.Pressed -= OnPreviousMetricPressed;
        }

        if (_nextMetricButton is not null)
        {
            _nextMetricButton.Pressed -= OnNextMetricPressed;
        }
    }

    private void ResetSimulation()
    {
        _elapsedTimeSeconds = 0.0f;
        _simulation = new Simulation(_runConfiguration.ToSimulationSettings());
        _simulation.InitializeAgents();
        _survivalRegions = SurvivalRegionFactory.CreateBorderBands(
            _runConfiguration.WorldSize,
            _runConfiguration.SurvivalBandThickness);
        _evolutionCycleController = new EvolutionCycleController(
            new RegionSurvivalRule(_survivalRegions),
            new ReproductionSystem(new MutationSystem(), seed: null),
            _runConfiguration.ToEvolutionSettings(),
            _runConfiguration.ToSimulationSettings());
        _sandboxRenderer!.Bind(_simulation, _runConfiguration);
        _sandboxRenderer.SetSurvivalRegions(_survivalRegions);
        _sandboxRenderer.SetSelectionState(System.Array.Empty<Agent>(), selectionActive: false, selectionProgress: 0.0f);
        _runHistory = new RunHistory();
        _generationChart!.Bind(_runHistory);
        UpdateChartMetric();
        UpdateHud();
    }

    private void OnApplyPressed()
    {
        _runConfiguration.AgentCount = (int)_agentCountSpinBox!.Value;
        _runConfiguration.GenerationDurationSeconds = (float)_lifeDurationSpinBox!.Value;
        _runConfiguration.SelectionDurationSeconds = (float)_selectionDurationSpinBox!.Value;
        _runConfiguration.TimeScale = (float)_timeScaleSpinBox!.Value;
        _runConfiguration.MutationStrength = (float)_mutationStrengthSpinBox!.Value;
        _runConfiguration.SurvivalBandThickness = (float)_bandThicknessSpinBox!.Value;
        _runConfiguration.Acceleration = (float)_accelerationSpinBox!.Value;
        _runConfiguration.Drag = (float)_dragSpinBox!.Value;
        _runConfiguration.StuckSpeedThreshold = (float)_stuckSpeedSpinBox!.Value;
        ResetSimulation();
    }

    private void SyncEditorsFromConfiguration()
    {
        _agentCountSpinBox!.Value = _runConfiguration.AgentCount;
        _lifeDurationSpinBox!.Value = _runConfiguration.GenerationDurationSeconds;
        _selectionDurationSpinBox!.Value = _runConfiguration.SelectionDurationSeconds;
        _timeScaleSpinBox!.Value = _runConfiguration.TimeScale;
        _mutationStrengthSpinBox!.Value = _runConfiguration.MutationStrength;
        _bandThicknessSpinBox!.Value = _runConfiguration.SurvivalBandThickness;
        _accelerationSpinBox!.Value = _runConfiguration.Acceleration;
        _dragSpinBox!.Value = _runConfiguration.Drag;
        _stuckSpeedSpinBox!.Value = _runConfiguration.StuckSpeedThreshold;
    }

    private void UpdateHud()
    {
        if (_simulation is null || _evolutionCycleController is null)
        {
            return;
        }
        var liveMetrics = LiveMetrics.FromAgents(_simulation.WorldState.Agents);

        _timeValueLabel!.Text = $"{_elapsedTimeSeconds:0.0}s";
        _generationValueLabel!.Text = _evolutionCycleController.GenerationIndex.ToString();
        _lifeValueLabel!.Text = $"{_evolutionCycleController.GenerationElapsedSeconds:0.0}/{_runConfiguration.GenerationDurationSeconds:0.0}s";
        _agentValueLabel!.Text = liveMetrics.AgentCount.ToString();
        _survivorValueLabel!.Text = _evolutionCycleController.LastSurvivorCount.ToString();
        _phaseValueLabel!.Text = _evolutionCycleController.Phase.ToString();
        _energyValueLabel!.Text = $"{liveMetrics.AverageEnergy:0.0}";
        _speedValueLabel!.Text = $"{liveMetrics.AverageSpeed:0.0}";
    }

    private void UpdateChartMetric()
    {
        var metricId = MetricCatalog.GetByIndex(_chartMetricIndex);
        _chartMetricValueLabel!.Text = MetricCatalog.GetLabel(metricId);
        _generationChart!.SetMetric(metricId);
    }

    private void OnPreviousMetricPressed()
    {
        _chartMetricIndex -= 1;
        UpdateChartMetric();
    }

    private void OnNextMetricPressed()
    {
        _chartMetricIndex += 1;
        UpdateChartMetric();
    }

    private void UpdateRendering()
    {
        _sandboxRenderer!.SetSelectionState(
            _evolutionCycleController!.SelectedSurvivors,
            _evolutionCycleController.Phase == EvolutionPhase.Selection,
            _evolutionCycleController.SelectionProgress);
        _sandboxRenderer.QueueRedraw();
    }

    private void ApplyWindowSize()
    {
        DisplayServer.WindowSetSize(new Vector2I(
            Mathf.RoundToInt(_runConfiguration.ViewportSize.X),
            Mathf.RoundToInt(_runConfiguration.ViewportSize.Y)));
    }

    private void StepSimulation(float scaledDelta)
    {
        if (_simulation is null || _evolutionCycleController is null)
        {
            return;
        }

        var remaining = scaledDelta;
        var safetyCounter = 0;

        while (remaining > 0.0f && safetyCounter < 120)
        {
            var step = Mathf.Min(remaining, MaxSimulationStepSeconds);
            var phaseBeforeUpdate = _evolutionCycleController.Phase;

            if (_evolutionCycleController.Phase == EvolutionPhase.Running)
            {
                _simulation.Update(step);
            }

            if (_evolutionCycleController.Update(_simulation.WorldState, step))
            {
                _simulation.ResolvePopulationLayout();
            }

            if (phaseBeforeUpdate == EvolutionPhase.Running &&
                _evolutionCycleController.Phase == EvolutionPhase.Selection)
            {
                var snapshot = StatisticsCollector.Capture(
                    _evolutionCycleController.GenerationIndex,
                    _simulation.WorldState.Agents,
                    _evolutionCycleController.SelectedSurvivors);
                _runHistory.Add(snapshot);
                _generationChart!.QueueRedraw();
            }

            remaining -= step;
            safetyCounter += 1;
        }
    }

}
