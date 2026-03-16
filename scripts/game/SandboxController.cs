using System.Collections.Generic;
using EvoPrototype.Analytics;
using EvoPrototype.Core;
using EvoPrototype.Evolution;
using EvoPrototype.Rendering;
using EvoPrototype.Setup;
using EvoPrototype.Ui;
using Godot;

namespace EvoPrototype.Game;

public partial class SandboxController : Node2D
{
    private const float MaxSimulationStepSeconds = 1.0f / 30.0f;
    private readonly RunConfiguration _runConfiguration = new();
    private readonly GameSessionState _sessionState = new();
    private Simulation? _simulation;
    private EvolutionCycleController? _evolutionCycleController;
    private IReadOnlyList<ISurvivalZone>? _survivalZones;
    private SandboxRenderer? _sandboxRenderer;
    private SetupOverlay? _setupOverlay;
    private GenerationChart? _generationChart;
    private Control? _setupScreen;
    private Control? _topBar;
    private Control? _bottomPanel;
    private Control? _metricsSection;
    private Control? _evolutionControlsSection;
    private Control? _physicsControlsSection;
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
    private SpinBox? _accelerationSpinBox;
    private SpinBox? _dragSpinBox;
    private SpinBox? _stuckSpeedSpinBox;
    private Label? _setupHintLabel;
    private Label? _setupSubtitleLabel;
    private Button? _startRunButton;
    private Button? _editZoneButton;
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
        _sessionState.SurvivalZoneDefinition = _runConfiguration.SurvivalZoneDefinition;
        EnterSetupPhase();
    }

    public override void _ExitTree()
    {
        DisconnectSignals();
    }

    public override void _Process(double delta)
    {
        if (_sessionState.CurrentPhase == GamePhase.Setup)
        {
            UpdateHud();
            return;
        }

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
        _setupOverlay = GetNode<SetupOverlay>("SetupOverlay");
        _setupScreen = GetNode<Control>("CanvasLayer/UiRoot/SetupScreen");
        _topBar = GetNode<Control>("CanvasLayer/UiRoot/TopBar");
        _bottomPanel = GetNode<Control>("CanvasLayer/UiRoot/BottomPanel");
        _metricsSection = GetNode<Control>("CanvasLayer/UiRoot/RightPanel/PanelMargin/PanelStack/Metrics");
        _evolutionControlsSection = GetNode<Control>("CanvasLayer/UiRoot/RightPanel/PanelMargin/PanelStack/EvolutionControls");
        _physicsControlsSection = GetNode<Control>("CanvasLayer/UiRoot/RightPanel/PanelMargin/PanelStack/PhysicsControls");
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
        _accelerationSpinBox = GetNode<SpinBox>("CanvasLayer/UiRoot/RightPanel/PanelMargin/PanelStack/PhysicsControls/AccelerationEditor");
        _dragSpinBox = GetNode<SpinBox>("CanvasLayer/UiRoot/RightPanel/PanelMargin/PanelStack/PhysicsControls/DragEditor");
        _stuckSpeedSpinBox = GetNode<SpinBox>("CanvasLayer/UiRoot/RightPanel/PanelMargin/PanelStack/PhysicsControls/StuckSpeedEditor");
        _setupHintLabel = GetNode<Label>("CanvasLayer/UiRoot/RightPanel/PanelMargin/PanelStack/SetupControls/SetupHint");
        _setupSubtitleLabel = GetNode<Label>("CanvasLayer/UiRoot/SetupScreen/SetupMargin/SetupStack/SetupSubtitle");
        _startRunButton = GetNode<Button>("CanvasLayer/UiRoot/RightPanel/PanelMargin/PanelStack/SetupControls/StartRunButton");
        _editZoneButton = GetNode<Button>("CanvasLayer/UiRoot/RightPanel/PanelMargin/PanelStack/SetupControls/EditZoneButton");
        _applyButton = GetNode<Button>("CanvasLayer/UiRoot/RightPanel/PanelMargin/PanelStack/ApplyButton");
        _previousMetricButton = GetNode<Button>("CanvasLayer/UiRoot/BottomPanel/BottomMargin/BottomStack/ChartHeader/PreviousMetricButton");
        _nextMetricButton = GetNode<Button>("CanvasLayer/UiRoot/BottomPanel/BottomMargin/BottomStack/ChartHeader/NextMetricButton");
        _chartMetricValueLabel = GetNode<Label>("CanvasLayer/UiRoot/BottomPanel/BottomMargin/BottomStack/ChartHeader/MetricValue");
        _setupOverlay.Bind(_runConfiguration);
        _setupOverlay.DefinitionChanged += OnSetupDefinitionChanged;
    }

    private void ConnectSignals()
    {
        _startRunButton!.Pressed += OnStartRunPressed;
        _editZoneButton!.Pressed += OnEditZonePressed;
        _applyButton!.Pressed += OnApplyPressed;
        _previousMetricButton!.Pressed += OnPreviousMetricPressed;
        _nextMetricButton!.Pressed += OnNextMetricPressed;
    }

    private void DisconnectSignals()
    {
        if (_setupOverlay is not null)
        {
            _setupOverlay.DefinitionChanged -= OnSetupDefinitionChanged;
        }

        if (_startRunButton is not null)
        {
            _startRunButton.Pressed -= OnStartRunPressed;
        }

        if (_editZoneButton is not null)
        {
            _editZoneButton.Pressed -= OnEditZonePressed;
        }

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
        _survivalZones = SurvivalRegionFactory.CreateZones(
            _runConfiguration.WorldSize,
            _runConfiguration.SurvivalZoneDefinition);
        _evolutionCycleController = new EvolutionCycleController(
            new RegionSurvivalRule(_survivalZones),
            new ReproductionSystem(new MutationSystem(), seed: null),
            _runConfiguration.ToEvolutionSettings(),
            _runConfiguration.ToSimulationSettings());
        _sandboxRenderer!.Bind(_simulation, _runConfiguration);
        _sandboxRenderer.SetSurvivalZones(_survivalZones);
        _sandboxRenderer.SetSelectionState(System.Array.Empty<Agent>(), selectionActive: false, selectionProgress: 0.0f);
        _runHistory = new RunHistory();
        _generationChart!.Bind(_runHistory);
        UpdateChartMetric();
        UpdateHud();
    }

    private void EnterSetupPhase()
    {
        _sessionState.CurrentPhase = GamePhase.Setup;
        _simulation = null;
        _evolutionCycleController = null;
        _survivalZones = SurvivalRegionFactory.CreateZones(_runConfiguration.WorldSize, _sessionState.SurvivalZoneDefinition);
        _setupOverlay!.SetDefinition(_sessionState.SurvivalZoneDefinition);
        _setupOverlay.SetEditingEnabled(true);
        _setupOverlay.Visible = true;
        _sandboxRenderer!.Visible = false;
        UpdateSetupUi();
        UpdateHud();
    }

    private void StartRun()
    {
        _runConfiguration.SurvivalZoneDefinition = _sessionState.SurvivalZoneDefinition;
        ResetSimulation();
        _sessionState.CurrentPhase = GamePhase.Running;
        _setupOverlay!.SetEditingEnabled(false);
        _setupOverlay.Visible = false;
        _sandboxRenderer!.Visible = true;
        UpdateSetupUi();
        UpdateRendering();
        UpdateHud();
    }

    private void OnApplyPressed()
    {
        _runConfiguration.AgentCount = (int)_agentCountSpinBox!.Value;
        _runConfiguration.GenerationDurationSeconds = (float)_lifeDurationSpinBox!.Value;
        _runConfiguration.SelectionDurationSeconds = (float)_selectionDurationSpinBox!.Value;
        _runConfiguration.TimeScale = (float)_timeScaleSpinBox!.Value;
        _runConfiguration.MutationStrength = (float)_mutationStrengthSpinBox!.Value;
        _runConfiguration.Acceleration = (float)_accelerationSpinBox!.Value;
        _runConfiguration.Drag = (float)_dragSpinBox!.Value;
        _runConfiguration.StuckSpeedThreshold = (float)_stuckSpeedSpinBox!.Value;
        if (_sessionState.CurrentPhase == GamePhase.Running)
        {
            StartRun();
        }
    }

    private void SyncEditorsFromConfiguration()
    {
        _agentCountSpinBox!.Value = _runConfiguration.AgentCount;
        _lifeDurationSpinBox!.Value = _runConfiguration.GenerationDurationSeconds;
        _selectionDurationSpinBox!.Value = _runConfiguration.SelectionDurationSeconds;
        _timeScaleSpinBox!.Value = _runConfiguration.TimeScale;
        _mutationStrengthSpinBox!.Value = _runConfiguration.MutationStrength;
        _accelerationSpinBox!.Value = _runConfiguration.Acceleration;
        _dragSpinBox!.Value = _runConfiguration.Drag;
        _stuckSpeedSpinBox!.Value = _runConfiguration.StuckSpeedThreshold;
    }

    private void UpdateHud()
    {
        if (_sessionState.CurrentPhase == GamePhase.Setup)
        {
            _timeValueLabel!.Text = "0.0s";
            _generationValueLabel!.Text = "-";
            _lifeValueLabel!.Text = "Setup";
            _agentValueLabel!.Text = _runConfiguration.AgentCount.ToString();
            _survivorValueLabel!.Text = "-";
            _phaseValueLabel!.Text = "Setup";
            _energyValueLabel!.Text = "-";
            _speedValueLabel!.Text = "-";
            return;
        }

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

    private void OnStartRunPressed()
    {
        StartRun();
    }

    private void OnEditZonePressed()
    {
        EnterSetupPhase();
    }

    private void OnSetupDefinitionChanged(SurvivalZoneDefinition definition)
    {
        _sessionState.SurvivalZoneDefinition = definition;
        _survivalZones = SurvivalRegionFactory.CreateZones(_runConfiguration.WorldSize, definition);
    }

    private void UpdateSetupUi()
    {
        var inSetup = _sessionState.CurrentPhase == GamePhase.Setup;
        _setupScreen!.Visible = inSetup;
        _topBar!.Visible = !inSetup;
        _bottomPanel!.Visible = !inSetup;
        _metricsSection!.Visible = !inSetup;
        _evolutionControlsSection!.Visible = !inSetup;
        _physicsControlsSection!.Visible = !inSetup;
        _setupHintLabel!.Text = inSetup
            ? "Drag inside the circle to move it. Drag its edge handle to resize it, then start the run."
            : "The current run uses the configured survival zone. Return to setup to edit it.";
        _setupSubtitleLabel!.Text = "Phase 1: Build the survival zone";
        _startRunButton!.Disabled = !inSetup;
        _editZoneButton!.Disabled = inSetup;
        _applyButton!.Disabled = inSetup;
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
