using EvoPrototype.Analytics;
using EvoPrototype.Core;
using EvoPrototype.Evolution;
using Godot;

var failures = new List<string>();

Run("Wall clamp stops outward movement without bounce", TestWallClampStopsOutwardMovementWithoutBounce);
Run("Overlap resolution separates agents by radius", TestOverlapResolutionSeparatesAgentsByRadius);
Run("Overlap solver resolves dense chains of agents", TestOverlapSolverResolvesDenseChainsOfAgents);
Run("Brain turns toward open space near a wall", TestBrainTurnsTowardOpenSpaceNearWall);
Run("Brain adds unstuck turn near a wall at low speed", TestBrainAddsUnstuckTurnNearWallAtLowSpeed);
Run("Decision timers differ per agent because of jitter", TestDecisionTimersDifferPerAgentBecauseOfJitter);
Run("Sustained throttle overcomes drag over time", TestSustainedThrottleOvercomesDragOverTime);
Run("Heavier agent accelerates less with same intent", TestHeavierAgentAcceleratesLessWithSameIntent);
Run("Brake reduces speed when intent requests it", TestBrakeReducesSpeedWhenIntentRequestsIt);
Run("No energy prevents active acceleration", TestNoEnergyPreventsActiveAcceleration);
Run("Idle drain reduces energy over time", TestIdleDrainReducesEnergyOverTime);
Run("World acceleration wish is applied in absolute coordinates", TestWorldAccelerationWishIsAppliedInAbsoluteCoordinates);
Run("Brain brakes when a neighbor is too close in front", TestBrainBrakesWhenNeighborIsTooCloseInFront);
Run("Region survival rule keeps agents inside configured zones", TestRegionSurvivalRuleKeepsAgentsInsideConfiguredZones);
Run("Reproduction system restores target population from survivors", TestReproductionSystemRestoresTargetPopulationFromSurvivors);
Run("Reproduction system falls back when no survivor exists", TestReproductionSystemFallsBackWhenNoSurvivorExists);
Run("Evolution cycle advances generation after duration", TestEvolutionCycleAdvancesGenerationAfterDuration);
Run("Statistics collector captures deaths and averages", TestStatisticsCollectorCapturesDeathsAndAverages);
Run("Run history keeps snapshots in generation order", TestRunHistoryKeepsSnapshotsInGenerationOrder);

if (failures.Count > 0)
{
    foreach (var failure in failures)
    {
        Console.Error.WriteLine(failure);
    }

    return 1;
}

Console.WriteLine("All simulation tests passed.");
return 0;

void Run(string testName, Action testAction)
{
    try
    {
        testAction();
        Console.WriteLine($"PASS: {testName}");
    }
    catch (Exception exception)
    {
        failures.Add($"FAIL: {testName} -> {exception.Message}");
    }
}

void TestWallClampStopsOutwardMovementWithoutBounce()
{
    var simulation = new Simulation(new SimulationSettings
    {
        WorldSize = new Vector2(100.0f, 100.0f),
        AgentCount = 0,
        AgentRadius = 5.0f,
        Acceleration = 0.0f,
        Drag = 0.0f,
        MaxSpeed = 200.0f
    });

    var agent = new Agent
    {
        Genome = AgentGenome.Default,
        PhysicsProfile = PhysicsProfile.Default,
        Position = new Vector2(95.0f, 50.0f),
        Velocity = new Vector2(20.0f, 0.0f),
        Heading = 0.0f,
        Radius = 5.0f,
        Energy = 100.0f,
        DecisionTimer = 999.0f
    };

    simulation.WorldState.Agents.Add(agent);
    simulation.Update(1.0f);

    AssertNearlyEqual(95.0f, agent.Position.X, 0.01f, "Agent should stay clamped to the right wall.");
    AssertNearlyEqual(0.0f, agent.Velocity.X, 0.01f, "Agent should lose outward horizontal velocity instead of bouncing.");
}

void TestOverlapResolutionSeparatesAgentsByRadius()
{
    var simulation = new Simulation(new SimulationSettings
    {
        WorldSize = new Vector2(100.0f, 100.0f),
        AgentCount = 0,
        AgentRadius = 5.0f,
        Acceleration = 0.0f,
        Drag = 0.0f,
        MaxSpeed = 0.0f
    });

    var firstAgent = new Agent
    {
        Genome = AgentGenome.Default,
        PhysicsProfile = PhysicsProfile.Default,
        Position = new Vector2(50.0f, 50.0f),
        Velocity = Vector2.Zero,
        Heading = 0.0f,
        Radius = 5.0f,
        Energy = 100.0f,
        DecisionTimer = 999.0f
    };

    var secondAgent = new Agent
    {
        Genome = AgentGenome.Default,
        PhysicsProfile = PhysicsProfile.Default,
        Position = new Vector2(53.0f, 50.0f),
        Velocity = Vector2.Zero,
        Heading = 0.0f,
        Radius = 5.0f,
        Energy = 100.0f,
        DecisionTimer = 999.0f
    };

    simulation.WorldState.Agents.Add(firstAgent);
    simulation.WorldState.Agents.Add(secondAgent);
    simulation.Update(0.0f);

    var distance = firstAgent.Position.DistanceTo(secondAgent.Position);
    AssertTrue(distance >= 9.99f, "Agents should be separated to at least the sum of their radii.");
}

void TestOverlapSolverResolvesDenseChainsOfAgents()
{
    var simulation = new Simulation(new SimulationSettings
    {
        WorldSize = new Vector2(120.0f, 120.0f),
        AgentCount = 0,
        AgentRadius = 6.0f,
        Acceleration = 0.0f,
        Drag = 0.0f,
        MaxSpeed = 0.0f,
        OverlapSolverIterations = 6
    });

    simulation.WorldState.Agents.Add(new Agent
    {
        Genome = AgentGenome.Default,
        PhysicsProfile = PhysicsProfile.Default,
        Position = new Vector2(50.0f, 60.0f),
        Velocity = Vector2.Zero,
        Heading = 0.0f,
        Radius = 6.0f,
        Energy = 100.0f,
        DecisionTimer = 999.0f
    });

    simulation.WorldState.Agents.Add(new Agent
    {
        Genome = AgentGenome.Default,
        PhysicsProfile = PhysicsProfile.Default,
        Position = new Vector2(56.0f, 60.0f),
        Velocity = Vector2.Zero,
        Heading = 0.0f,
        Radius = 6.0f,
        Energy = 100.0f,
        DecisionTimer = 999.0f
    });

    simulation.WorldState.Agents.Add(new Agent
    {
        Genome = AgentGenome.Default,
        PhysicsProfile = PhysicsProfile.Default,
        Position = new Vector2(62.0f, 60.0f),
        Velocity = Vector2.Zero,
        Heading = 0.0f,
        Radius = 6.0f,
        Energy = 100.0f,
        DecisionTimer = 999.0f
    });

    simulation.Update(0.0f);

    var agents = simulation.WorldState.Agents;
    for (var firstIndex = 0; firstIndex < agents.Count; firstIndex++)
    {
        for (var secondIndex = firstIndex + 1; secondIndex < agents.Count; secondIndex++)
        {
            var distance = agents[firstIndex].Position.DistanceTo(agents[secondIndex].Position);
            AssertTrue(distance >= 11.99f, "Dense overlap chains should also be fully separated.");
        }
    }
}

void TestBrainTurnsTowardOpenSpaceNearWall()
{
    var agent = new Agent
    {
        Genome = new AgentGenome(
            BaseThrottle: 0.8f,
            BaseTurnRate: 0.0f,
            WallAvoidanceWeight: 1.5f,
            NeighborAvoidanceWeight: 0.0f,
            DecisionIntervalSeconds: 0.2f,
            DecisionIntervalJitterSeconds: 0.0f,
            WanderAmplitude: 0.0f,
            WanderFrequency: 1.0f,
            StuckTurnBias: 2.0f),
        PhysicsProfile = PhysicsProfile.Default,
        Position = Vector2.Zero,
        Velocity = Vector2.Zero,
        Heading = 0.0f,
        Radius = 4.0f,
        Energy = 100.0f,
        BrainClock = 0.0f
    };

    var observation = new AgentObservation(
        ForwardWallDistance: 12.0f,
        LeftWallDistance: 10.0f,
        RightWallDistance: 40.0f,
        NearestNeighborLocalOffset: Vector2.Zero,
        HasNeighbor: false,
        Speed: 12.0f,
        IsTouchingWall: false);

    var decision = AgentBrain.Decide(agent, observation, new SimulationSettings
    {
        WallAvoidanceDistance = 60.0f
    });

    AssertTrue(decision.TurnWish > 0.0f, "Agent should turn toward the side with more clearance.");
}

void TestBrainAddsUnstuckTurnNearWallAtLowSpeed()
{
    var agent = new Agent
    {
        Genome = new AgentGenome(
            BaseThrottle: 0.8f,
            BaseTurnRate: 0.0f,
            WallAvoidanceWeight: 0.0f,
            NeighborAvoidanceWeight: 0.0f,
            DecisionIntervalSeconds: 0.2f,
            DecisionIntervalJitterSeconds: 0.0f,
            WanderAmplitude: 0.0f,
            WanderFrequency: 1.0f,
            StuckTurnBias: 2.5f),
        PhysicsProfile = PhysicsProfile.Default,
        Position = Vector2.Zero,
        Velocity = Vector2.Zero,
        Heading = 0.0f,
        Radius = 4.0f,
        Energy = 100.0f,
        BrainClock = 0.0f
    };

    var observation = new AgentObservation(
        ForwardWallDistance: 5.5f,
        LeftWallDistance: 8.0f,
        RightWallDistance: 25.0f,
        NearestNeighborLocalOffset: Vector2.Zero,
        HasNeighbor: false,
        Speed: 1.0f,
        IsTouchingWall: true);

    var decision = AgentBrain.Decide(agent, observation, new SimulationSettings
    {
        WallAvoidanceDistance = 60.0f,
        StuckSpeedThreshold = 8.0f
    });

    AssertTrue(decision.TurnWish > 2.0f, "Agent should get a strong unstuck turn when slow and pressed against a wall.");
}

void TestDecisionTimersDifferPerAgentBecauseOfJitter()
{
    var simulation = new Simulation(new SimulationSettings
    {
        WorldSize = new Vector2(200.0f, 120.0f),
        AgentCount = 2,
        AgentRadius = 4.0f
    }, seed: 123);

    simulation.InitializeAgents();

    var firstTimer = simulation.WorldState.Agents[0].DecisionTimer;
    var secondTimer = simulation.WorldState.Agents[1].DecisionTimer;

    AssertTrue(MathF.Abs(firstTimer - secondTimer) > 0.0001f, "Agents should not share the exact same next decision time.");
}

void TestSustainedThrottleOvercomesDragOverTime()
{
    var simulation = new Simulation(new SimulationSettings
    {
        WorldSize = new Vector2(400.0f, 200.0f),
        AgentCount = 0,
        AgentRadius = 4.0f,
        MaxSpeed = 110.0f,
        Acceleration = 220.0f,
        Drag = 0.35f,
        MinimumDecisionIntervalSeconds = 0.05f
    }, seed: 42);

    var agent = new Agent
    {
        Genome = new AgentGenome(
            BaseThrottle: 1.0f,
            BaseTurnRate: 0.0f,
            WallAvoidanceWeight: 0.0f,
            NeighborAvoidanceWeight: 0.0f,
            DecisionIntervalSeconds: 0.1f,
            DecisionIntervalJitterSeconds: 0.0f,
            WanderAmplitude: 0.0f,
            WanderFrequency: 1.0f,
            StuckTurnBias: 0.0f),
        PhysicsProfile = PhysicsProfile.Default,
        Position = new Vector2(100.0f, 100.0f),
        Velocity = Vector2.Zero,
        Heading = 0.0f,
        Radius = 4.0f,
        Energy = 100.0f,
        DecisionTimer = 0.0f,
        TimeSinceLastDecision = 0.1f,
        BrainClock = 0.0f
    };

    simulation.WorldState.Agents.Add(agent);

    for (var index = 0; index < 20; index++)
    {
        simulation.Update(0.1f);
    }

    AssertTrue(agent.Velocity.Length() > 40.0f, "An agent with full throttle should keep meaningful speed instead of damping to near zero.");
}

void TestHeavierAgentAcceleratesLessWithSameIntent()
{
    var lightAgent = new Agent
    {
        Genome = AgentGenome.Default,
        PhysicsProfile = PhysicsProfile.Default with { Mass = 1.0f, MaxForwardAcceleration = 200.0f },
        Position = Vector2.Zero,
        Velocity = Vector2.Zero,
        Heading = 0.0f,
        Radius = 4.0f,
        Energy = 100.0f,
        CurrentIntent = new AgentIntent(Vector2.Zero, new Vector2(0.0f, 1.0f), 0.0f, 0.0f)
    };

    var heavyAgent = new Agent
    {
        Genome = AgentGenome.Default,
        PhysicsProfile = PhysicsProfile.Default with { Mass = 2.0f, MaxForwardAcceleration = 200.0f },
        Position = Vector2.Zero,
        Velocity = Vector2.Zero,
        Heading = 0.0f,
        Radius = 4.0f,
        Energy = 100.0f,
        CurrentIntent = new AgentIntent(Vector2.Zero, new Vector2(0.0f, 1.0f), 0.0f, 0.0f)
    };

    var lightVelocity = SimulateSingleStepVelocity(lightAgent, 0.1f);
    var heavyVelocity = SimulateSingleStepVelocity(heavyAgent, 0.1f);

    AssertTrue(lightVelocity.Length() > heavyVelocity.Length(), "Higher mass should reduce acceleration for the same intent.");
}

void TestBrakeReducesSpeedWhenIntentRequestsIt()
{
    var agent = new Agent
    {
        Genome = AgentGenome.Default,
        PhysicsProfile = PhysicsProfile.Default with { LinearDrag = 0.0f, MaxBrakeAcceleration = 100.0f },
        Position = Vector2.Zero,
        Velocity = new Vector2(50.0f, 0.0f),
        Heading = 0.0f,
        Radius = 4.0f,
        Energy = 100.0f,
        CurrentIntent = new AgentIntent(Vector2.Zero, Vector2.Zero, 0.0f, 1.0f)
    };

    var velocityAfter = SimulateSingleStepVelocity(agent, 0.1f);
    AssertTrue(velocityAfter.Length() < 50.0f, "Brake intent should reduce speed.");
}

void TestNoEnergyPreventsActiveAcceleration()
{
    var agent = new Agent
    {
        Genome = AgentGenome.Default,
        PhysicsProfile = PhysicsProfile.Default with { LinearDrag = 0.0f, MaxForwardAcceleration = 200.0f },
        Position = Vector2.Zero,
        Velocity = Vector2.Zero,
        Heading = 0.0f,
        Radius = 4.0f,
        Energy = 0.0f,
        CurrentIntent = new AgentIntent(Vector2.Zero, new Vector2(0.0f, 1.0f), 0.0f, 0.0f)
    };

    var velocityAfter = SimulateSingleStepVelocity(agent, 0.1f);
    AssertNearlyEqual(0.0f, velocityAfter.Length(), 0.01f, "An agent without energy should not gain speed from active acceleration.");
}

void TestIdleDrainReducesEnergyOverTime()
{
    var agent = new Agent
    {
        Genome = AgentGenome.Default,
        PhysicsProfile = PhysicsProfile.Default with { IdleEnergyDrainPerSecond = 2.0f, LinearDrag = 0.0f },
        Position = Vector2.Zero,
        Velocity = Vector2.Zero,
        Heading = 0.0f,
        Radius = 4.0f,
        Energy = 10.0f,
        CurrentIntent = AgentIntent.Neutral
    };

    SimulateSingleStep(agent, 0.5f);
    AssertNearlyEqual(9.0f, agent.Energy, 0.02f, "Idle drain should reduce energy continuously even without movement.");
}

void TestWorldAccelerationWishIsAppliedInAbsoluteCoordinates()
{
    var agent = new Agent
    {
        Genome = AgentGenome.Default,
        PhysicsProfile = PhysicsProfile.Default with { LinearDrag = 0.0f, MaxForwardAcceleration = 200.0f },
        Position = Vector2.Zero,
        Velocity = Vector2.Zero,
        Heading = Mathf.Pi / 2.0f,
        Radius = 4.0f,
        Energy = 100.0f,
        CurrentIntent = new AgentIntent(new Vector2(50.0f, 0.0f), Vector2.Zero, 0.0f, 0.0f)
    };

    SimulateSingleStep(agent, 0.1f);
    AssertTrue(agent.Velocity.X > 0.1f, "Absolute world acceleration should affect X even if heading points elsewhere.");
    AssertNearlyEqual(0.0f, agent.Velocity.Y, 0.05f, "Pure world X acceleration should not inject noticeable Y velocity here.");
}

void TestBrainBrakesWhenNeighborIsTooCloseInFront()
{
    var agent = new Agent
    {
        Genome = AgentGenome.Default,
        PhysicsProfile = PhysicsProfile.Default,
        Position = Vector2.Zero,
        Velocity = new Vector2(10.0f, 0.0f),
        Heading = 0.0f,
        Radius = 4.0f,
        Energy = 100.0f,
        BrainClock = 0.0f
    };

    var observation = new AgentObservation(
        ForwardWallDistance: 100.0f,
        LeftWallDistance: 100.0f,
        RightWallDistance: 100.0f,
        NearestNeighborLocalOffset: new Vector2(0.0f, 5.0f),
        HasNeighbor: true,
        Speed: 10.0f,
        IsTouchingWall: false);

    var intent = AgentBrain.Decide(agent, observation, new SimulationSettings
    {
        NeighborBrakeDistance = 18.0f
    });

    AssertTrue(intent.BrakeWish > 0.1f, "A close neighbor in front should trigger braking.");
}

void TestRegionSurvivalRuleKeepsAgentsInsideConfiguredZones()
{
    var worldState = new WorldState(new Vector2(100.0f, 100.0f));
    var rule = new RegionSurvivalRule(new[]
    {
        new SurvivalRegion(new Rect2(0.0f, 0.0f, 10.0f, 100.0f)),
        new SurvivalRegion(new Rect2(90.0f, 0.0f, 10.0f, 100.0f))
    });

    var insideAgent = CreateTestAgent(new Vector2(5.0f, 50.0f));
    var outsideAgent = CreateTestAgent(new Vector2(50.0f, 50.0f));

    AssertTrue(rule.Survives(insideAgent, worldState, new EvolutionSettings()), "Agent inside a survival region should survive.");
    AssertTrue(!rule.Survives(outsideAgent, worldState, new EvolutionSettings()), "Agent outside all survival regions should die.");
}

void TestReproductionSystemRestoresTargetPopulationFromSurvivors()
{
    var mutationSystem = new MutationSystem(seed: 1);
    var reproductionSystem = new ReproductionSystem(mutationSystem, seed: 2);
    var survivors = new[]
    {
        CreateTestAgent(new Vector2(10.0f, 10.0f)),
        CreateTestAgent(new Vector2(20.0f, 20.0f))
    };

    var nextGeneration = reproductionSystem.BuildNextGeneration(
        survivors,
        new EvolutionSettings
        {
            TargetPopulationSize = 12,
            MutationStrength = 0.05f
        },
        new SimulationSettings
        {
            WorldSize = new Vector2(200.0f, 120.0f),
            AgentRadius = 4.0f
        });

    AssertTrue(nextGeneration.Count == 12, "Reproduction should rebuild the full target population.");
}

void TestReproductionSystemFallsBackWhenNoSurvivorExists()
{
    var mutationSystem = new MutationSystem(seed: 3);
    var reproductionSystem = new ReproductionSystem(mutationSystem, seed: 4);

    var nextGeneration = reproductionSystem.BuildNextGeneration(
        Array.Empty<Agent>(),
        new EvolutionSettings
        {
            TargetPopulationSize = 8,
            MutationStrength = 0.05f
        },
        new SimulationSettings
        {
            WorldSize = new Vector2(200.0f, 120.0f),
            AgentRadius = 4.0f
        });

    AssertTrue(nextGeneration.Count == 8, "Fallback reproduction should still rebuild the target population.");
}

void TestEvolutionCycleAdvancesGenerationAfterDuration()
{
    var worldState = new WorldState(new Vector2(100.0f, 100.0f));
    worldState.Agents.Add(CreateTestAgent(new Vector2(5.0f, 50.0f)));
    worldState.Agents.Add(CreateTestAgent(new Vector2(50.0f, 50.0f)));

    var evolutionSettings = new EvolutionSettings
    {
        GenerationDurationSeconds = 1.0f,
        TargetPopulationSize = 6,
        MutationStrength = 0.05f
    };

    var cycleController = new EvolutionCycleController(
        new RegionSurvivalRule(new[]
        {
            new SurvivalRegion(new Rect2(0.0f, 0.0f, 10.0f, 100.0f))
        }),
        new ReproductionSystem(new MutationSystem(seed: 5), seed: 6),
        evolutionSettings,
        new SimulationSettings
        {
            WorldSize = new Vector2(100.0f, 100.0f),
            AgentRadius = 4.0f
        });

    var enteredSelection = cycleController.Update(worldState, 1.1f);
    AssertTrue(!enteredSelection, "Controller should first enter selection mode before rebuilding the next generation.");
    AssertTrue(cycleController.Phase == EvolutionPhase.Selection, "Cycle controller should enter the selection phase after the life duration.");
    AssertTrue(cycleController.LastSurvivorCount == 1, "Exactly one agent should have survived this configured region.");

    var advancedGeneration = cycleController.Update(worldState, 1.6f);
    AssertTrue(advancedGeneration, "Controller should rebuild the next generation after the selection phase duration.");
    AssertTrue(cycleController.GenerationIndex == 1, "Generation index should increment after a full cycle.");
    AssertTrue(cycleController.Phase == EvolutionPhase.Running, "Controller should return to the running phase after reproduction.");
    AssertTrue(worldState.Agents.Count == 6, "Next generation should restore the target population size.");
}

void TestStatisticsCollectorCapturesDeathsAndAverages()
{
    var firstAgent = CreateAnalyticsAgent(new Vector2(5.0f, 10.0f), 1.2f, 0.4f);
    var secondAgent = CreateAnalyticsAgent(new Vector2(10.0f, 10.0f), 1.8f, 0.8f);
    var thirdAgent = CreateAnalyticsAgent(new Vector2(15.0f, 10.0f), 0.9f, 0.6f);

    var population = new[] { firstAgent, secondAgent, thirdAgent };
    var survivors = new[] { firstAgent, thirdAgent };

    var snapshot = StatisticsCollector.Capture(7, population, survivors);

    AssertTrue(snapshot.GenerationIndex == 7, "Snapshot should preserve the current generation index.");
    AssertTrue(snapshot.DeathCount == 1, "One agent should be counted as dead.");
    AssertNearlyEqual(2.0f / 3.0f, snapshot.SurvivalRate, 0.001f, "Survival rate should match survivor ratio.");
    AssertNearlyEqual((1.2f + 1.8f + 0.9f) / 3.0f, snapshot.AverageMass, 0.001f, "Average mass should be computed on the full evaluated population.");
    AssertNearlyEqual((0.4f + 0.8f + 0.6f) / 3.0f, snapshot.AverageBaseThrottle, 0.001f, "Average base throttle should be computed on the full evaluated population.");
}

void TestRunHistoryKeepsSnapshotsInGenerationOrder()
{
    var runHistory = new RunHistory();
    runHistory.Add(new GenerationStatistics(0, 200, 100, 100, 0.5f, 1.0f, 0.8f));
    runHistory.Add(new GenerationStatistics(1, 200, 120, 80, 0.6f, 1.1f, 0.85f));

    AssertTrue(runHistory.Snapshots.Count == 2, "Run history should store each appended snapshot.");
    AssertTrue(runHistory.Snapshots[0].GenerationIndex == 0, "Run history should preserve insertion order.");
    AssertTrue(runHistory.Snapshots[1].GenerationIndex == 1, "Later generations should stay at the end of the history.");
}

Agent CreateAnalyticsAgent(Vector2 position, float mass, float baseThrottle)
{
    return new Agent
    {
        Genome = AgentGenome.Default with { BaseThrottle = baseThrottle },
        PhysicsProfile = PhysicsProfile.Default with { Mass = mass },
        Position = position,
        Velocity = Vector2.Zero,
        Heading = 0.0f,
        Radius = 4.0f,
        Energy = 100.0f,
        DecisionTimer = 999.0f,
        CurrentIntent = AgentIntent.Neutral
    };
}

Vector2 SimulateSingleStepVelocity(Agent sourceAgent, float delta)
{
    var simulation = new Simulation(new SimulationSettings
    {
        WorldSize = new Vector2(400.0f, 200.0f),
        AgentCount = 0,
        AgentRadius = 4.0f,
        MaxSpeed = 500.0f
    }, seed: 1);

    simulation.WorldState.Agents.Add(sourceAgent);
    sourceAgent.DecisionTimer = 999.0f;
    simulation.Update(delta);
    return sourceAgent.Velocity;
}

void SimulateSingleStep(Agent sourceAgent, float delta)
{
    var simulation = new Simulation(new SimulationSettings
    {
        WorldSize = new Vector2(400.0f, 200.0f),
        AgentCount = 0,
        AgentRadius = 4.0f,
        MaxSpeed = 500.0f
    }, seed: 1);

    simulation.WorldState.Agents.Add(sourceAgent);
    sourceAgent.DecisionTimer = 999.0f;
    simulation.Update(delta);
}

Agent CreateTestAgent(Vector2 position)
{
    return new Agent
    {
        Genome = AgentGenome.Default,
        PhysicsProfile = PhysicsProfile.Default,
        Position = position,
        Velocity = Vector2.Zero,
        Heading = 0.0f,
        Radius = 4.0f,
        Energy = 100.0f,
        DecisionTimer = 999.0f,
        CurrentIntent = AgentIntent.Neutral
    };
}

void AssertTrue(bool condition, string message)
{
    if (!condition)
    {
        throw new InvalidOperationException(message);
    }
}

void AssertNearlyEqual(float expected, float actual, float tolerance, string message)
{
    if (MathF.Abs(expected - actual) > tolerance)
    {
        throw new InvalidOperationException($"{message} Expected {expected}, got {actual}.");
    }
}
