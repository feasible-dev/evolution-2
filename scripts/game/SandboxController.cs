using System.Collections.Generic;
using EvoPrototype.Analytics;
using EvoPrototype.Cards;
using EvoPrototype.Core;
using EvoPrototype.Evolution;
using EvoPrototype.Gameplay;
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
    private Control? _rightPanel;
    private Control? _metricsTitle;
    private Control? _metricsSection;
    private Control? _evolutionTitle;
    private Control? _setupTitle;
    private Control? _setupControlsSection;
    private Control? _cardsTitle;
    private Control? _cardsSection;
    private Control? _activeCardsBlock;
    private GridContainer? _activeCardsList;
    private Control? _availableCardsBlock;
    private GridContainer? _availableCardsList;
    private Control? _evolutionControlsSection;
    private Control? _physicsTitle;
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
    private SpinBox? _trailAlphaSpinBox;
    private SpinBox? _accelerationSpinBox;
    private SpinBox? _dragSpinBox;
    private SpinBox? _stuckSpeedSpinBox;
    private Label? _setupHintLabel;
    private Label? _setupSubtitleLabel;
    private Label? _cardsHintLabel;
    private Button? _setupStartRunButton;
    private Button? _startRunButton;
    private Button? _editZoneButton;
    private Button? _applyButton;
    private Button? _previousMetricButton;
    private Button? _nextMetricButton;
    private Label? _chartMetricValueLabel;
    private RunHistory _runHistory = new();
    private RunConfiguration? _activeRunConfiguration;
    private RunConfiguration? _pendingRunConfiguration;
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
        RefreshCardsUi();
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
        StepSimulation(scaledDelta, MaxSimulationStepSeconds);
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
        _rightPanel = GetNode<Control>("CanvasLayer/UiRoot/RightPanel");
        _metricsTitle = GetNode<Control>("CanvasLayer/UiRoot/LeftPanel/PanelMargin/PanelStack/MetricsTitle");
        _metricsSection = GetNode<Control>("CanvasLayer/UiRoot/LeftPanel/PanelMargin/PanelStack/Metrics");
        _evolutionTitle = GetNode<Control>("CanvasLayer/UiRoot/LeftPanel/PanelMargin/PanelStack/EvolutionTitle");
        _setupTitle = GetNode<Control>("CanvasLayer/UiRoot/RightPanel/PanelMargin/PanelStack/SetupTitle");
        _setupControlsSection = GetNode<Control>("CanvasLayer/UiRoot/RightPanel/PanelMargin/PanelStack/SetupControls");
        _cardsTitle = GetNode<Control>("CanvasLayer/UiRoot/RightPanel/PanelMargin/PanelStack/CardsTitle");
        _cardsSection = GetNode<Control>("CanvasLayer/UiRoot/RightPanel/PanelMargin/PanelStack/CardsSection");
        _activeCardsBlock = GetNode<Control>("CanvasLayer/UiRoot/RightPanel/PanelMargin/PanelStack/CardsSection/ActiveCardsBlock");
        _activeCardsList = GetNode<GridContainer>("CanvasLayer/UiRoot/RightPanel/PanelMargin/PanelStack/CardsSection/ActiveCardsBlock/ActiveCardsList");
        _availableCardsBlock = GetNode<Control>("CanvasLayer/UiRoot/RightPanel/PanelMargin/PanelStack/CardsSection/AvailableCardsBlock");
        _availableCardsList = GetNode<GridContainer>("CanvasLayer/UiRoot/RightPanel/PanelMargin/PanelStack/CardsSection/AvailableCardsBlock/AvailableCardsList");
        _evolutionControlsSection = GetNode<Control>("CanvasLayer/UiRoot/LeftPanel/PanelMargin/PanelStack/EvolutionControls");
        _physicsTitle = GetNode<Control>("CanvasLayer/UiRoot/LeftPanel/PanelMargin/PanelStack/PhysicsTitle");
        _physicsControlsSection = GetNode<Control>("CanvasLayer/UiRoot/LeftPanel/PanelMargin/PanelStack/PhysicsControls");
        _generationChart = GetNode<GenerationChart>("CanvasLayer/UiRoot/BottomPanel/BottomMargin/BottomStack/Chart");
        _timeValueLabel = GetNode<Label>("CanvasLayer/UiRoot/TopBar/StatsRow/TimeValue");
        _generationValueLabel = GetNode<Label>("CanvasLayer/UiRoot/TopBar/StatsRow/GenerationValue");
        _lifeValueLabel = GetNode<Label>("CanvasLayer/UiRoot/TopBar/StatsRow/LifeValue");
        _agentValueLabel = GetNode<Label>("CanvasLayer/UiRoot/TopBar/StatsRow/AgentValue");
        _survivorValueLabel = GetNode<Label>("CanvasLayer/UiRoot/LeftPanel/PanelMargin/PanelStack/Metrics/SurvivorValue");
        _phaseValueLabel = GetNode<Label>("CanvasLayer/UiRoot/LeftPanel/PanelMargin/PanelStack/Metrics/PhaseValue");
        _energyValueLabel = GetNode<Label>("CanvasLayer/UiRoot/LeftPanel/PanelMargin/PanelStack/Metrics/EnergyValue");
        _speedValueLabel = GetNode<Label>("CanvasLayer/UiRoot/LeftPanel/PanelMargin/PanelStack/Metrics/SpeedValue");
        _agentCountSpinBox = GetNode<SpinBox>("CanvasLayer/UiRoot/LeftPanel/PanelMargin/PanelStack/EvolutionControls/AgentCountEditor");
        _lifeDurationSpinBox = GetNode<SpinBox>("CanvasLayer/UiRoot/LeftPanel/PanelMargin/PanelStack/EvolutionControls/LifeDurationEditor");
        _selectionDurationSpinBox = GetNode<SpinBox>("CanvasLayer/UiRoot/LeftPanel/PanelMargin/PanelStack/EvolutionControls/SelectionDurationEditor");
        _timeScaleSpinBox = GetNode<SpinBox>("CanvasLayer/UiRoot/LeftPanel/PanelMargin/PanelStack/EvolutionControls/TimeScaleEditor");
        _mutationStrengthSpinBox = GetNode<SpinBox>("CanvasLayer/UiRoot/LeftPanel/PanelMargin/PanelStack/EvolutionControls/MutationStrengthEditor");
        _trailAlphaSpinBox = GetNode<SpinBox>("CanvasLayer/UiRoot/LeftPanel/PanelMargin/PanelStack/EvolutionControls/TrailAlphaEditor");
        _accelerationSpinBox = GetNode<SpinBox>("CanvasLayer/UiRoot/LeftPanel/PanelMargin/PanelStack/PhysicsControls/AccelerationEditor");
        _dragSpinBox = GetNode<SpinBox>("CanvasLayer/UiRoot/LeftPanel/PanelMargin/PanelStack/PhysicsControls/DragEditor");
        _stuckSpeedSpinBox = GetNode<SpinBox>("CanvasLayer/UiRoot/LeftPanel/PanelMargin/PanelStack/PhysicsControls/StuckSpeedEditor");
        _setupHintLabel = GetNode<Label>("CanvasLayer/UiRoot/RightPanel/PanelMargin/PanelStack/SetupControls/SetupHint");
        _setupSubtitleLabel = GetNode<Label>("CanvasLayer/UiRoot/SetupScreen/SetupMargin/SetupStack/SetupSubtitle");
        _cardsHintLabel = GetNode<Label>("CanvasLayer/UiRoot/RightPanel/PanelMargin/PanelStack/CardsSection/CardsHint");
        _setupStartRunButton = GetNode<Button>("CanvasLayer/UiRoot/SetupScreen/SetupMargin/SetupStack/SetupButtonRow/SetupStartRunButton");
        _startRunButton = GetNode<Button>("CanvasLayer/UiRoot/RightPanel/PanelMargin/PanelStack/SetupControls/StartRunButton");
        _editZoneButton = GetNode<Button>("CanvasLayer/UiRoot/RightPanel/PanelMargin/PanelStack/SetupControls/EditZoneButton");
        _applyButton = GetNode<Button>("CanvasLayer/UiRoot/LeftPanel/PanelMargin/PanelStack/ApplyButton");
        _previousMetricButton = GetNode<Button>("CanvasLayer/UiRoot/BottomPanel/BottomMargin/BottomStack/ChartHeader/PreviousMetricButton");
        _nextMetricButton = GetNode<Button>("CanvasLayer/UiRoot/BottomPanel/BottomMargin/BottomStack/ChartHeader/NextMetricButton");
        _chartMetricValueLabel = GetNode<Label>("CanvasLayer/UiRoot/BottomPanel/BottomMargin/BottomStack/ChartHeader/MetricValue");
        _setupOverlay.Bind(_runConfiguration);
        _setupOverlay.DefinitionChanged += OnSetupDefinitionChanged;
    }

    private void ConnectSignals()
    {
        _setupStartRunButton!.Pressed += OnStartRunPressed;
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

        if (_setupStartRunButton is not null)
        {
            _setupStartRunButton.Pressed -= OnStartRunPressed;
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
        _pendingRunConfiguration = null;
        var runtimeConfiguration = _activeRunConfiguration ?? _runConfiguration;
        _simulation = new Simulation(runtimeConfiguration.ToSimulationSettings());
        _simulation.InitializeAgents();
        _survivalZones = SurvivalRegionFactory.CreateZones(
            runtimeConfiguration.WorldSize,
            runtimeConfiguration.SurvivalZoneDefinition);
        _evolutionCycleController = new EvolutionCycleController(
            new RegionSurvivalRule(_survivalZones),
            new ReproductionSystem(new MutationSystem(), seed: null),
            runtimeConfiguration.ToEvolutionSettings(),
            runtimeConfiguration.ToSimulationSettings());
        _sandboxRenderer!.Bind(_simulation, runtimeConfiguration);
        _sandboxRenderer.SetSurvivalZones(_survivalZones);
        _sandboxRenderer.SetSelectionState(System.Array.Empty<Agent>(), selectionActive: false, selectionProgress: 0.0f);
        _sandboxRenderer.SetLiveRenderingEnabled(true);
        _sandboxRenderer.CaptureFrame();
        _runHistory = new RunHistory();
        _generationChart!.Bind(_runHistory);
        UpdateChartMetric();
        UpdateHud();
    }

    private void EnterSetupPhase()
    {
        _sessionState.CurrentPhase = GamePhase.Setup;
        _activeRunConfiguration = null;
        _pendingRunConfiguration = null;
        _simulation = null;
        _evolutionCycleController = null;
        _survivalZones = SurvivalRegionFactory.CreateZones(_runConfiguration.WorldSize, _sessionState.SurvivalZoneDefinition);
        _setupOverlay!.SetDefinition(_sessionState.SurvivalZoneDefinition);
        _setupOverlay.SetEditingEnabled(true);
        _setupOverlay.Visible = true;
        _sandboxRenderer!.Visible = false;
        RefreshCardsUi();
        UpdateSetupUi();
        UpdateHud();
    }

    private void StartRun()
    {
        _runConfiguration.SurvivalZoneDefinition = _sessionState.SurvivalZoneDefinition;
        _activeRunConfiguration = BuildRuntimeConfigurationFromCards();
        ResetSimulation();
        _sessionState.CurrentPhase = GamePhase.Running;
        _setupOverlay!.SetEditingEnabled(false);
        _setupOverlay.Visible = false;
        _sandboxRenderer!.Visible = true;
        RefreshCardsUi();
        UpdateSetupUi();
        UpdateRendering();
        UpdateHud();
    }

    private RunConfiguration BuildRuntimeConfigurationFromCards()
    {
        var build = EvolutionBuildCompiler.CreateFromBase(
            _runConfiguration,
            _sessionState.CardSelectionState.ActiveCards);
        return EvolutionBuildCompiler.Compile(_runConfiguration, build);
    }

    private void OnApplyPressed()
    {
        _runConfiguration.AgentCount = (int)_agentCountSpinBox!.Value;
        _runConfiguration.GenerationDurationSeconds = (float)_lifeDurationSpinBox!.Value;
        _runConfiguration.SelectionDurationSeconds = (float)_selectionDurationSpinBox!.Value;
        _runConfiguration.TimeScale = (float)_timeScaleSpinBox!.Value;
        _runConfiguration.MutationStrength = (float)_mutationStrengthSpinBox!.Value;
        _runConfiguration.TrailAlpha = (float)_trailAlphaSpinBox!.Value;
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
        _trailAlphaSpinBox!.Value = _runConfiguration.TrailAlpha;
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
        _setupScreen!.Visible = false;
        _topBar!.Visible = !inSetup;
        _bottomPanel!.Visible = !inSetup;
        _rightPanel!.Visible = true;
        _metricsTitle!.Visible = !inSetup;
        _metricsSection!.Visible = !inSetup;
        _evolutionTitle!.Visible = !inSetup;
        _setupTitle!.Visible = inSetup;
        _setupControlsSection!.Visible = true;
        _cardsTitle!.Visible = true;
        _cardsSection!.Visible = true;
        _activeCardsBlock!.Visible = true;
        _availableCardsBlock!.Visible = true;
        _evolutionControlsSection!.Visible = !inSetup;
        _physicsTitle!.Visible = !inSetup;
        _physicsControlsSection!.Visible = !inSetup;
        _setupHintLabel!.Text = inSetup
            ? "Drag inside the circle to move it. Drag its edge handle to resize it, then start the run."
            : "The current run uses the configured survival zone. Return to setup to edit it.";
        _cardsHintLabel!.Text = inSetup
            ? "Choose which genetic possibilities are available before the run starts."
            : "Card changes made during the run apply to the next generation.";
        _setupSubtitleLabel!.Text = "Phase 1: Design the survival zone";
        _setupStartRunButton!.Visible = false;
        _startRunButton!.Visible = inSetup;
        _editZoneButton!.Visible = !inSetup;
        _startRunButton!.Disabled = !inSetup;
        _editZoneButton!.Disabled = inSetup;
        _applyButton!.Disabled = inSetup;
        _applyButton.Visible = !inSetup;
    }

    private void UpdateRendering()
    {
        var showSelectionOverlay = _evolutionCycleController!.Phase == EvolutionPhase.Selection;
        _sandboxRenderer!.SetSelectionState(
            _evolutionCycleController!.SelectedSurvivors,
            showSelectionOverlay,
            _evolutionCycleController.SelectionProgress);
        _sandboxRenderer.SetLiveRenderingEnabled(true);
        _sandboxRenderer.CaptureFrame();
        _sandboxRenderer.QueueRedraw();
    }

    private void ApplyWindowSize()
    {
        DisplayServer.WindowSetSize(new Vector2I(
            Mathf.RoundToInt(_runConfiguration.ViewportSize.X),
            Mathf.RoundToInt(_runConfiguration.ViewportSize.Y)));
    }

    private bool StepSimulation(float scaledDelta, float maxStepSeconds)
    {
        if (_simulation is null || _evolutionCycleController is null)
        {
            return false;
        }

        var remaining = scaledDelta;
        var safetyCounter = 0;
        var shouldRefreshRendering = false;

        while (remaining > 0.0f && safetyCounter < 120)
        {
            var step = Mathf.Min(remaining, maxStepSeconds);
            var phaseBeforeUpdate = _evolutionCycleController.Phase;
            var generationBeforeUpdate = _evolutionCycleController.GenerationIndex;

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
                ApplyPendingRunConfiguration();
                var snapshot = StatisticsCollector.Capture(
                    _evolutionCycleController.GenerationIndex,
                    _simulation.WorldState.Agents,
                    _evolutionCycleController.SelectedSurvivors);
                _runHistory.Add(snapshot);
                _generationChart!.QueueRedraw();
                shouldRefreshRendering = true;
            }

            if (_evolutionCycleController.GenerationIndex != generationBeforeUpdate)
            {
                shouldRefreshRendering = true;
            }

            remaining -= step;
            safetyCounter += 1;
        }

        return shouldRefreshRendering;
    }

    private void ApplyPendingRunConfiguration()
    {
        if (_pendingRunConfiguration is null || _simulation is null || _evolutionCycleController is null)
        {
            return;
        }

        _activeRunConfiguration = _pendingRunConfiguration;
        _pendingRunConfiguration = null;

        var simulationSettings = _activeRunConfiguration.ToSimulationSettings();
        var evolutionSettings = _activeRunConfiguration.ToEvolutionSettings();
        _simulation.UpdateSettings(simulationSettings);
        _evolutionCycleController.UpdateSettings(evolutionSettings, simulationSettings);
    }

    private void RefreshCardsUi()
    {
        RebuildCardList(_activeCardsList!, _sessionState.CardSelectionState.ActiveCards, activeList: true);
        RebuildCardList(_availableCardsList!, _sessionState.CardSelectionState.AvailableCards, activeList: false);
    }

    private void RebuildCardList(GridContainer container, IReadOnlyList<CardDefinition> cards, bool activeList)
    {
        foreach (var child in container.GetChildren())
        {
            container.RemoveChild(child);
            child.QueueFree();
        }

        if (cards.Count == 0)
        {
            var emptyLabel = new Label
            {
                Text = activeList ? "No active cards yet." : "No cards available."
            };
            emptyLabel.Modulate = new Color(0.68f, 0.72f, 0.78f);
            container.AddChild(emptyLabel);
            return;
        }

        foreach (var card in cards)
        {
            container.AddChild(BuildCardEntry(card, activeList));
        }
    }

    private Control BuildCardEntry(CardDefinition card, bool activeList)
    {
        var panel = new PanelContainer
        {
            CustomMinimumSize = new Vector2(0.0f, 96.0f)
        };
        panel.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;

        var (backgroundColor, borderColor, contentColor) = GetCardVisuals(card.Category, activeList);
        var styleBox = new StyleBoxFlat
        {
            BgColor = backgroundColor,
            BorderColor = borderColor,
            BorderWidthLeft = 2,
            BorderWidthTop = 2,
            BorderWidthRight = 2,
            BorderWidthBottom = 2,
            CornerRadiusTopLeft = 8,
            CornerRadiusTopRight = 8,
            CornerRadiusBottomRight = 8,
            CornerRadiusBottomLeft = 8
        };
        panel.AddThemeStyleboxOverride("panel", styleBox);

        var margin = new MarginContainer();
        margin.AddThemeConstantOverride("margin_left", 10);
        margin.AddThemeConstantOverride("margin_top", 8);
        margin.AddThemeConstantOverride("margin_right", 10);
        margin.AddThemeConstantOverride("margin_bottom", 8);
        panel.AddChild(margin);

        var stack = new VBoxContainer();
        stack.AddThemeConstantOverride("separation", 6);
        margin.AddChild(stack);

        var title = new Label
        {
            Text = card.Name
        };
        title.AddThemeFontSizeOverride("font_size", 16);
        title.Modulate = contentColor;
        stack.AddChild(title);

        var description = new Label
        {
            Text = card.Description,
            AutowrapMode = TextServer.AutowrapMode.WordSmart
        };
        description.AddThemeFontSizeOverride("font_size", 12);
        description.Modulate = contentColor with { A = activeList ? 0.92f : 0.72f };
        stack.AddChild(description);

        var button = new Button
        {
            Text = activeList ? "Remove" : "Add"
        };
        button.Pressed += () =>
        {
            if (activeList)
            {
                _sessionState.CardSelectionState.TryDeactivate(card.Id);
            }
            else
            {
                _sessionState.CardSelectionState.TryActivate(card.Id);
            }

            if (_sessionState.CurrentPhase == GamePhase.Running)
            {
                _pendingRunConfiguration = BuildRuntimeConfigurationFromCards();
            }

            RefreshCardsUi();
        };
        stack.AddChild(button);

        return panel;
    }

    private static (Color Background, Color Border, Color Content) GetCardVisuals(CardCategory category, bool active)
    {
        var baseColor = category switch
        {
            CardCategory.Condition => new Color(0.18f, 0.48f, 0.34f),
            CardCategory.Action => new Color(0.22f, 0.36f, 0.66f),
            CardCategory.Program => new Color(0.52f, 0.34f, 0.18f),
            CardCategory.Mutation => new Color(0.56f, 0.18f, 0.44f),
            CardCategory.Budget => new Color(0.52f, 0.48f, 0.16f),
            _ => new Color(0.30f, 0.30f, 0.34f)
        };

        var background = active
            ? baseColor.Lerp(Colors.White, 0.12f) with { A = 0.92f }
            : baseColor.Darkened(0.42f) with { A = 0.68f };
        var border = active
            ? baseColor.Lerp(Colors.White, 0.28f)
            : baseColor.Darkened(0.12f);
        var content = active
            ? new Color(0.96f, 0.97f, 0.99f)
            : new Color(0.78f, 0.81f, 0.86f);

        return (background, border, content);
    }

}
