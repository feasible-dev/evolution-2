using EvoPrototype.Core;
using EvoPrototype.Evolution;
using Godot;

namespace EvolutionTwo.Presentation;

public enum BroadcastStage { Preparation, Running, Selection }
public sealed record Commentary(int Speaker, int Pose, string Text);

public sealed class LevelOneSession
{
    public const float PeriodSeconds = 36;
    public const float BaseTimeScale = 2;
    public const float StepSeconds = 1f / 60;
    private const double ClockStep = 1.0 / 60;
    public const int Population = 240;
    public const float ZoneFraction = .05f;
    public static readonly Vector2 WorldSize = new(1280, 560);
    public Vector2 ArenaSize { get; private set; } = WorldSize;
    private RegionSurvivalRule _arenaRule = null!;
    public const int MaxSpeed = 10;
    private readonly bool _isOpponent;
    private int _roundNumber;
    private bool _opponentCelebrated;
    private const int SimulationSeed = 4127;
    public LevelOneSession? Opponent { get; }
    private HashSet<Agent> _survivors = new();
    private readonly List<PeriodOutcome> _history = new();
    private readonly Queue<Commentary> _commentary = new();
    private readonly int[] _composition = new int[4];
    private double _accumulator;
    private float _commentCooldown;
    private int _lastMilestone;
    public Simulation Simulation { get; private set; } = null!;
    public EvolutionCycleController Cycle { get; private set; } = null!;
    public BroadcastStage Stage { get; private set; }
    public BudgetPlan Draft { get; private set; }
    public BudgetPlan Active { get; private set; }
    public BudgetPlan? Pending { get; private set; }
    public int AppliedAtGeneration { get; private set; }
    public IReadOnlySet<Agent> Survivors => _survivors;
    public IReadOnlyList<PeriodOutcome> History => _history;
    public IReadOnlyList<int> Composition => _composition;
    public PeriodOutcome? LastResult => _history.LastOrDefault();
    public bool Discovered { get; private set; }
    public bool PerfectSurvivalReached { get; private set; }
    public bool Paused { get; private set; }
    public bool HasStarted { get; private set; }
    public int Speed { get; private set; }
    public int Extinctions { get; private set; }
    public double SimulatedSeconds { get; private set; }
    public string Notice { get; private set; } = "";
    public string Speech { get; private set; } = "";
    public int Speaker { get; private set; }
    public int Expression { get; private set; }
    public float SpeechAge { get; private set; }
    public bool IsSpeaking => SpeechAge < MathF.Min(5, Speech.Length / 30f + .4f);
    public int Revision { get; private set; }
    public int Period => Cycle.GenerationIndex + 1;
    public int Completed => _history.Count;
    public int Best => _history.Count == 0 ? 0 : _history.Max(h => h.Survivors);

    public LevelOneSession() : this(false) { }
    private LevelOneSession(bool isOpponent)
    {
        _isOpponent = isOpponent;
        if (!isOpponent) Opponent = new LevelOneSession(true);
        Reset();
    }
    public void Reset()
    {
        Opponent?.Reset(); _opponentCelebrated = false;
        Draft = Active = BudgetPlan.Default;
        Pending = null;
        AppliedAtGeneration = 1;
        _survivors.Clear(); _history.Clear(); Array.Clear(_composition);
        PerfectSurvivalReached = Discovered = Paused = HasStarted = false;
        Extinctions = 0; _accumulator = 0; SimulatedSeconds = 0;
        Speed = 5; _commentCooldown = 0; _lastMilestone = 0;
        Notice = L10n.Text("notice.initial");
        _arenaRule = new RegionSurvivalRule(SurvivalRegionFactory.CreateZones(ArenaSize, RightEdge()));
        var settings = BuildSimulationSettings(Active);
        InitializePopulation(settings);
        Cycle = MakeCycle(settings);
        Stage = BroadcastStage.Preparation;
        RecountPopulation();
        Conversation(new Commentary(0, 0, L10n.Text("speech.intro_gerard")),
            new Commentary(1, 1, L10n.Text("speech.intro_rene")));
        Revision++;
    }
    private sealed class LiveArenaRule(LevelOneSession owner) : ISurvivalRule
    {
        public SurvivalOutcome Evaluate(Agent agent, WorldState world, EvolutionSettings settings)
            => owner._arenaRule.Evaluate(agent, world, settings);
    }

    public void ResizeArena(Vector2 size)
    {
        if (size.X <= 0 || size.Y <= 0 || ArenaSize.DistanceSquaredTo(size) < .001f) return;
        var ratio = size / ArenaSize;
        ArenaSize = size;
        foreach (var agent in Simulation.WorldState.Agents)
        {
            agent.Position = BoardGeometry.ClampPoint(agent.Position * ratio, size, BoardShapeKind.RoundedRectangle, agent.Radius + .5f);
            agent.SpawnOrigin *= ratio;
            agent.ParentSpawnOrigin *= ratio;
            // Resizing is not movement and does not change velocity, energy or inherited traits.
        }
        Simulation.WorldState.Size = size;
        var settings = BuildSimulationSettings(Active);
        Simulation.UpdateSettings(settings);
        Cycle.UpdateSettings(BuildEvolutionSettings(), settings);
        _arenaRule = new RegionSurvivalRule(SurvivalRegionFactory.CreateZones(size, RightEdge()));
        Opponent?.ResizeArena(size);
        Revision++;
    }

    private EvolutionCycleController MakeCycle(SimulationSettings settings)
    {
        var cycle = new EvolutionCycleController(new LiveArenaRule(this),
            new ReproductionSystem(new MutationSystem(seed: 211), seed: 871), BuildEvolutionSettings(), settings);
        cycle.SetManualAdvanceEnabled(true);
        return cycle;
    }
    public bool ChangeAllocation(BehaviorChoice choice, int delta)
    {
        if (!Enum.IsDefined(choice) || delta is not (-1 or 1)) return false;
        return SetAllocation(choice, Draft[choice] + delta);
    }
    public bool SetAllocation(BehaviorChoice choice, int value)
    {
        if (HasStarted) return false;
        if (!Enum.IsDefined(choice) || value < 0 || value > BudgetPlan.Limit) return false;
        Draft = Draft.Allocate(choice, value);
        Notice = Draft[choice] < value
            ? L10n.Text("notice.full")
            : Draft.Remaining > 0 ? L10n.Text("notice.remaining", Draft.Remaining)
            : L10n.Text("notice.ready");
        Revision++; return true;
    }
    public void ClearPlan()
    {
        if (HasStarted) return;
        Draft = BudgetPlan.Default; Notice = L10n.Text("notice.cleared"); Revision++;
    }
    public bool SetSpeed(int speed)
    {
        if (speed < 1 || speed > MaxSpeed) return false;
        Speed = speed; Opponent?.SetSpeed(speed); Revision++; return true;
    }
    public void TogglePause()
    {
        if (!HasStarted) return;
        Paused = !Paused; Revision++;
    }
    public bool ApplyPlan()
    {
        if (HasStarted) { Notice = L10n.Text("plan.locked"); Revision++; return false; }
        if (!Draft.IsValid) { Notice = L10n.Text("notice.incomplete"); Revision++; return false; }
        return Start();
    }
    public bool Start(BudgetPlan? opponentPlan = null)
    {
        if (HasStarted || !Draft.IsValid || opponentPlan is { IsValid: false }) return false;
        if (Opponent is { } rival)
        {
            var plan = opponentPlan ?? OpponentStrategy.Choose(++_roundNumber);
            rival.SetAllocation(BehaviorChoice.Turn, plan.Turn);
            rival.SetAllocation(BehaviorChoice.Throttle, plan.Throttle);
            rival.SetAllocation(BehaviorChoice.Target, plan.Target);
            rival.SetSpeed(Speed);
            if (!rival.Start()) throw new InvalidOperationException("Opponent could not commit its plan.");
        }
        Active = Draft; Pending = null;
        var settings = BuildSimulationSettings(Active);
        InitializePopulation(settings);
        SeedInitialPrograms(settings); NormalizeBehaviorParameters(); Cycle = MakeCycle(settings);
        Simulation.ResolvePopulationLayout();
        HasStarted = true; Paused = false; Stage = BroadcastStage.Running;
        _accumulator = 0; RecountPopulation();
        Notice = L10n.Text("notice.started");
        Conversation(new Commentary(0, 0, L10n.Text("speech.start")));
        _commentCooldown = 6;
        Revision++; return true;
    }

    public void Tick(float realDelta, double frameBudgetMilliseconds = 0)
    {
        if (realDelta <= 0) return;
        SpeechAge += realDelta;
        _commentCooldown -= realDelta;
        if (SpeechAge >= MathF.Max(3.5f, Speech.Length / 27f + 1) && _commentary.Count > 0)
            Show(_commentary.Dequeue());
        if (Paused) return;
        if (!HasStarted) return;
        _accumulator += (double)realDelta * Speed * BaseTimeScale;
        var frameStart = System.Diagnostics.Stopwatch.GetTimestamp();
        while (_accumulator >= ClockStep - .000000001)
        {
            _accumulator = Math.Max(0, _accumulator - ClockStep);
            StepSimulation();
            Opponent?.StepSimulation();
            if (Opponent is { PerfectSurvivalReached: true } && !_opponentCelebrated)
            {
                _opponentCelebrated = true;
                Paused = true; Discovered = true; Revision++;
                if (!PerfectSurvivalReached)
                {
                    Notice = L10n.Text("notice.opponent_perfect");
                    Conversation(new Commentary(1, 1, L10n.Text("speech.opponent_perfect")));
                    _commentCooldown = 12;
                }
            }
            if (Paused) { _accumulator = 0; return; }
            // Under load, slow simulated time rather than stall rendering to catch up.
            // Both teams have completed the same fixed step; no physics or selection step is skipped.
            if (frameBudgetMilliseconds > 0 && System.Diagnostics.Stopwatch.GetElapsedTime(frameStart).TotalMilliseconds >= frameBudgetMilliseconds)
            {
                _accumulator = Math.Min(_accumulator, ClockStep);
                break;
            }
        }
        if (_commentCooldown <= 0 && Completed >= 10 && Completed / 10 > _lastMilestone)
        {
            _lastMilestone = Completed / 10;
            Conversation(new Commentary(1, 0, L10n.Text("speech.milestone", Completed, LastResult!.SurvivalRate * 100)));
            _commentCooldown = 7;
        }
    }
    private void StepSimulation()
    {
            var wasRunning = Cycle.Phase == EvolutionPhase.Running;
            if (wasRunning)
            {
                Simulation.Update(StepSeconds);
                SimulatedSeconds += ClockStep;
            }
            Cycle.Update(Simulation.WorldState, StepSeconds, StepSeconds / (Speed * BaseTimeScale));
            if (wasRunning && Cycle.Phase == EvolutionPhase.Selection)
            {
                Stage = BroadcastStage.Selection;
                _survivors = Cycle.SelectedSurvivors.ToHashSet();
                RecordResult();
                Revision++;
                if (Paused) return;
            }
            if (!Discovered && Stage == BroadcastStage.Selection && Cycle.SelectionElapsedSeconds >= .18f)
            {
                Discovered = true;
                Conversation(new Commentary(0, 2, L10n.Text("speech.reveal_gerard")),
                    new Commentary(1, 0, L10n.Text("speech.reveal_rene")));
                _commentCooldown = 8;
                Revision++;
            }
            if (Cycle.WaitingForManualAdvance) AdvanceGeneration();
    }
    private void AdvanceGeneration()
    {
        if (Pending is { } plan)
        {
            Active = plan; Pending = null;
            AppliedAtGeneration = Period + 1;
            Notice = L10n.Text("notice.active", AppliedAtGeneration);
        }
        var settings = BuildSimulationSettings(Active);
        Simulation.UpdateSettings(settings);
        Cycle.UpdateSettings(BuildEvolutionSettings(), settings);
        if (!Cycle.AdvanceToNextGeneration(Simulation.WorldState)) throw new InvalidOperationException("Generation boundary not ready.");
        NormalizeBehaviorParameters();
        Simulation.ResolvePopulationLayout();
        Stage = BroadcastStage.Running; _survivors.Clear(); RecountPopulation();
        Revision++;
    }
    private void RecountPopulation()
    {
        Array.Clear(_composition);
        foreach (var a in Simulation.WorldState.Agents)
            _composition[BehaviorCatalog.For(a) is { } d ? (int)d.Id : 3]++;
    }
    private void RecordResult()
    {
        var rules = BehaviorCatalog.All.ToDictionary(d => d.Id, d =>
        {
            var agents = Simulation.WorldState.Agents.Where(a => BehaviorCatalog.For(a)?.Id == d.Id).ToArray();
            return new RuleOutcome(agents.Length, agents.Count(a => _survivors.Contains(a)));
        });
        var neutral = Simulation.WorldState.Agents.Where(a => BehaviorCatalog.For(a) is null).ToArray();
        _history.Add(new PeriodOutcome(Period, _survivors.Count, rules,
            new RuleOutcome(neutral.Length, neutral.Count(a => _survivors.Contains(a))), Active));
        if (_survivors.Count == 0) Extinctions++;
        if (_survivors.Count == Population && !PerfectSurvivalReached)
        {
            PerfectSurvivalReached = true; Discovered = true; Paused = !_isOpponent;
            Notice = L10n.Text("notice.perfect");
            Conversation(new Commentary(0, 2, L10n.Text("speech.perfect_gerard")),
                new Commentary(1, 1, L10n.Text("speech.perfect_rene")));
            _commentCooldown = 12;
        }
    }
    private void Conversation(Commentary first, params Commentary[] tail)
    {
        _commentary.Clear(); Show(first);
        foreach (var line in tail) _commentary.Enqueue(line);
    }
    private void Show(Commentary line)
    {
        Speaker = line.Speaker; Expression = line.Pose; Speech = line.Text; SpeechAge = 0; Revision++;
    }
    public static SurvivalZoneDefinition RightEdge() => SurvivalZoneDefinition.Default with
    {
        Layout = SurvivalZoneLayout.Rectangle,
        RectangleCenterNormalized = new Vector2(1 - ZoneFraction / 2, .5f),
        RectangleSizeNormalized = new Vector2(ZoneFraction, 1)
    };
    private SimulationSettings BuildSimulationSettings(BudgetPlan plan) => new()
    {
        WorldSize = ArenaSize, BoardShape = BoardShapeKind.RoundedRectangle,
        AgentCount = Population, AgentRadius = 2, // Drag is inherited per-agent via PhysicsProfile.
        GenerationDurationSeconds = PeriodSeconds, ContinuousBaselineBrownianMotion = true,
        BaseRandomJerkRuleWeight = .55f, BaseRandomJerkFadeGenerations = 8,
        FixedExplorationWeight = DuelMotionPolicy.ExplorationWeight, FixedTargetAcceleration = DuelMotionPolicy.TargetAcceleration,
        GenePool = BehaviorCatalog.Pool(plan), RuleMutationProfile = BehaviorCatalog.Profile(plan)
    };
    private EvolutionSettings BuildEvolutionSettings() => new()
    {
        FixedPhysicsProfile = PhysicsProfile.Default,
        GenerationDurationSeconds = PeriodSeconds,
        SelectionDurationSeconds = Discovered ? .10f : .80f,
        SelectionMinRealTimeSeconds = Discovered ? .10f : .80f,
        SelectionMaxRealTimeSeconds = Discovered ? .10f : .80f,
        TimeScale = 1, TargetPopulationSize = Population, MutationStrength = .03f,
        // Introductory challenge: preserve successful inherited behaviour while retaining variation.
        MutationPolicy = MutationPolicy.Default with { ProgramMutationChance = .10f, AddRuleChance = .08f,
            RemoveRuleChance = .005f, ActionKindMutationChance = .08f, ActionParameterMutationChance = .20f },
        RuleMutationProfile = BehaviorCatalog.Profile(Active),
        SurvivalZoneDefinition = RightEdge()
    };
    private void InitializePopulation(SimulationSettings settings)
    {
        // Generate positions and bodies from the same neutral pool before applying either deck.
        Simulation = new Simulation(BuildSimulationSettings(new BudgetPlan(3, 3, 4)), seed: SimulationSeed);
        Simulation.InitializeAgents();
        Simulation.UpdateSettings(settings);
        ApplySharedPhysicalProfile();
    }

    private void ApplySharedPhysicalProfile()
    {
        foreach (var agent in Simulation.WorldState.Agents)
        {
            agent.PhysicsProfile = PhysicsProfile.Default;
            agent.Energy = PhysicsProfile.Default.MaxEnergy;
        }
    }

    private void SeedInitialPrograms(SimulationSettings settings)
    {
        var random = new Random(509);
        for (var i = 0; i < Simulation.WorldState.Agents.Count; i++)
        {
            var a = Simulation.WorldState.Agents[i];
            var rule = GeneRuleFactory.CreateRandomRule(random, settings.GenePool, PeriodSeconds, .10, settings.RuleMutationProfile);
            var genome = a.Genome with { Program = new GeneProgram(rule, GeneRule.Disabled, GeneRule.Disabled) };
            genome = DuelMotionPolicy.Normalize(genome);
            Simulation.WorldState.Agents[i] = CopyWithGenome(a, genome);
        }
    }
    private void NormalizeBehaviorParameters()
    {
        var agents = Simulation.WorldState.Agents;
        for (var i = 0; i < agents.Count; i++)
        {
            agents[i] = CopyWithGenome(agents[i], DuelMotionPolicy.Normalize(agents[i].Genome));
            agents[i].DecisionTimer = DuelMotionPolicy.DecisionInterval;
        }
    }

    private static Agent CopyWithGenome(Agent a, AgentGenome genome) => new()
    {
        AgentId = a.AgentId, ParentAgentId = a.ParentAgentId, HasParent = a.HasParent,
        GenerationNumber = a.GenerationNumber, ParentChildCount = a.ParentChildCount, LineageAge = a.LineageAge,
        Genome = genome, Position = a.Position, SpawnOrigin = a.SpawnOrigin, ParentSpawnOrigin = a.ParentSpawnOrigin,
        MaxDistanceFromBirth = a.MaxDistanceFromBirth, Velocity = a.Velocity, Heading = a.Heading,
        Radius = a.Radius, Energy = a.Energy, DecisionTimer = a.DecisionTimer,
        TimeSinceLastDecision = a.TimeSinceLastDecision, TimeBelowSpeedThreshold = a.TimeBelowSpeedThreshold,
        ImmobileAxisXSeconds = a.ImmobileAxisXSeconds, ImmobileAxisYSeconds = a.ImmobileAxisYSeconds,
        IsTouchingWall = a.IsTouchingWall, BrainClock = a.BrainClock,
        DecisionTurnNoise = a.DecisionTurnNoise, DecisionThrottleNoise = a.DecisionThrottleNoise,
        PhysicsProfile = a.PhysicsProfile, CurrentIntent = a.CurrentIntent, Color = a.Color
    };
}
