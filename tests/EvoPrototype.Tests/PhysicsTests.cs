using EvoPrototype.Core;
using Godot;
using static TestAssertions;
using static TestFixtures;

public static class PhysicsTests
{
    public static void Register(TestHarness harness)
    {
        harness.Run("Wall clamp stops outward movement without bounce", TestWallClampStopsOutwardMovementWithoutBounce);
        harness.Run("Overlap resolution separates agents by radius", TestOverlapResolutionSeparatesAgentsByRadius);
        harness.Run("Overlap solver resolves dense chains of agents", TestOverlapSolverResolvesDenseChainsOfAgents);
        harness.Run("Brain turns toward open space near a wall", TestBrainTurnsTowardOpenSpaceNearWall);
        harness.Run("Brain adds unstuck turn near a wall at low speed", TestBrainAddsUnstuckTurnNearWallAtLowSpeed);
        harness.Run("Decision timers differ per agent because of jitter", TestDecisionTimersDifferPerAgentBecauseOfJitter);
        harness.Run("Sustained throttle overcomes drag over time", TestSustainedThrottleOvercomesDragOverTime);
        harness.Run("Heavier agent accelerates less with same intent", TestHeavierAgentAcceleratesLessWithSameIntent);
        harness.Run("Brake reduces speed when intent requests it", TestBrakeReducesSpeedWhenIntentRequestsIt);
        harness.Run("No energy prevents active acceleration", TestNoEnergyPreventsActiveAcceleration);
        harness.Run("Idle drain reduces energy over time", TestIdleDrainReducesEnergyOverTime);
        harness.Run("World acceleration wish is applied in absolute coordinates", TestWorldAccelerationWishIsAppliedInAbsoluteCoordinates);
        harness.Run("Brain brakes when a neighbor is too close in front", TestBrainBrakesWhenNeighborIsTooCloseInFront);
        harness.Run("Brain steers toward configured world target", TestBrainSteersTowardConfiguredWorldTarget);
        harness.Run("Preferred speed fraction changes throttle response", TestPreferredSpeedFractionChangesThrottleResponse);
        harness.Run("Action resolver sums weighted contributions by channel", TestActionResolverSumsWeightedContributionsByChannel);
        harness.Run("Time-based gene rule activates only after its threshold", TestTimeBasedGeneRuleActivatesOnlyAfterThreshold);
        harness.Run("Gene steer-to-target rule adds world acceleration", TestGeneSteerToTargetRuleAddsWorldAcceleration);
    }

    private static void TestWallClampStopsOutwardMovementWithoutBounce()
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

    private static void TestOverlapResolutionSeparatesAgentsByRadius()
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

    private static void TestOverlapSolverResolvesDenseChainsOfAgents()
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

    private static void TestBrainTurnsTowardOpenSpaceNearWall()
    {
        var agent = new Agent
        {
            Genome = AgentGenome.Default with
            {
                BaseThrottle = 0.8f,
                PreferredSpeedFraction = 0.7f,
                BaseTurnRate = 0.0f,
                WallAvoidanceWeight = 1.5f,
                NeighborAvoidanceWeight = 0.0f,
                DecisionIntervalSeconds = 0.2f,
                DecisionIntervalJitterSeconds = 0.0f,
                WanderAmplitude = 0.0f,
                WanderFrequency = 1.0f,
                StuckTurnBias = 2.0f,
                RandomTurnStrength = 0.0f,
                RandomThrottleBias = 0.0f,
                TargetPointWeight = 0.0f
            },
            PhysicsProfile = PhysicsProfile.Default,
            Position = Vector2.Zero,
            Velocity = Vector2.Zero,
            Heading = 0.0f,
            Radius = 4.0f,
            Energy = 100.0f,
            BrainClock = 0.0f
        };

        var observation = new AgentObservation(12.0f, 10.0f, 40.0f, Vector2.Zero, false, 12.0f, false);
        var decision = AgentBrain.Decide(agent, observation, new SimulationSettings { WallAvoidanceDistance = 60.0f });
        AssertTrue(decision.TurnWish > 0.0f, "Agent should turn toward the side with more clearance.");
    }

    private static void TestBrainAddsUnstuckTurnNearWallAtLowSpeed()
    {
        var agent = new Agent
        {
            Genome = AgentGenome.Default with
            {
                BaseThrottle = 0.8f,
                PreferredSpeedFraction = 0.7f,
                BaseTurnRate = 0.0f,
                WallAvoidanceWeight = 0.0f,
                NeighborAvoidanceWeight = 0.0f,
                DecisionIntervalSeconds = 0.2f,
                DecisionIntervalJitterSeconds = 0.0f,
                WanderAmplitude = 0.0f,
                WanderFrequency = 1.0f,
                StuckTurnBias = 2.5f,
                RandomTurnStrength = 0.0f,
                RandomThrottleBias = 0.0f,
                TargetPointWeight = 0.0f
            },
            PhysicsProfile = PhysicsProfile.Default,
            Position = Vector2.Zero,
            Velocity = Vector2.Zero,
            Heading = 0.0f,
            Radius = 4.0f,
            Energy = 100.0f,
            BrainClock = 0.0f
        };

        var observation = new AgentObservation(5.5f, 8.0f, 25.0f, Vector2.Zero, false, 1.0f, true);
        var decision = AgentBrain.Decide(agent, observation, new SimulationSettings
        {
            WallAvoidanceDistance = 60.0f,
            StuckSpeedThreshold = 8.0f
        });

        AssertTrue(decision.TurnWish > 2.0f, "Agent should get a strong unstuck turn when slow and pressed against a wall.");
    }

    private static void TestDecisionTimersDifferPerAgentBecauseOfJitter()
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

    private static void TestSustainedThrottleOvercomesDragOverTime()
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
            Genome = AgentGenome.Default with
            {
                BaseThrottle = 1.0f,
                PreferredSpeedFraction = 1.0f,
                BaseTurnRate = 0.0f,
                WallAvoidanceWeight = 0.0f,
                NeighborAvoidanceWeight = 0.0f,
                DecisionIntervalSeconds = 0.1f,
                DecisionIntervalJitterSeconds = 0.0f,
                WanderAmplitude = 0.0f,
                WanderFrequency = 1.0f,
                StuckTurnBias = 0.0f,
                RandomTurnStrength = 0.0f,
                RandomThrottleBias = 0.0f,
                TargetPointWeight = 0.0f
            },
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

    private static void TestHeavierAgentAcceleratesLessWithSameIntent()
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

    private static void TestBrakeReducesSpeedWhenIntentRequestsIt()
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

    private static void TestNoEnergyPreventsActiveAcceleration()
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

    private static void TestIdleDrainReducesEnergyOverTime()
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

    private static void TestWorldAccelerationWishIsAppliedInAbsoluteCoordinates()
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

    private static void TestBrainBrakesWhenNeighborIsTooCloseInFront()
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
            100.0f,
            100.0f,
            100.0f,
            new Vector2(0.0f, 5.0f),
            true,
            10.0f,
            false);

        var intent = AgentBrain.Decide(agent, observation, new SimulationSettings { NeighborBrakeDistance = 18.0f });
        AssertTrue(intent.BrakeWish > 0.1f, "A close neighbor in front should trigger braking.");
    }

    private static void TestBrainSteersTowardConfiguredWorldTarget()
    {
        var agent = new Agent
        {
            Genome = AgentGenome.Default with
            {
                BaseThrottle = 0.0f,
                PreferredSpeedFraction = 0.0f,
                WanderAmplitude = 0.0f,
                RandomTurnStrength = 0.0f,
                RandomThrottleBias = 0.0f,
                WallAvoidanceWeight = 0.0f,
                NeighborAvoidanceWeight = 0.0f,
                TargetPointNormalized = new Vector2(1.0f, 0.5f),
                TargetPointWeight = 1.4f
            },
            PhysicsProfile = PhysicsProfile.Default,
            Position = new Vector2(20.0f, 50.0f),
            Velocity = Vector2.Zero,
            Heading = 0.0f,
            Radius = 4.0f,
            Energy = 100.0f
        };

        var settings = new SimulationSettings { WorldSize = new Vector2(100.0f, 100.0f), Acceleration = 200.0f };
        var observation = new AgentObservation(100.0f, 100.0f, 100.0f, Vector2.Zero, false, 0.0f, false);
        var intent = AgentBrain.Decide(agent, observation, settings);

        AssertTrue(intent.WorldAccelerationWish.X > 0.1f, "Target-seeking genes should add a world-space acceleration toward the configured point.");
    }

    private static void TestPreferredSpeedFractionChangesThrottleResponse()
    {
        var fastAgent = new Agent
        {
            Genome = AgentGenome.Default with
            {
                BaseThrottle = 0.4f,
                PreferredSpeedFraction = 1.0f,
                WanderAmplitude = 0.0f,
                RandomTurnStrength = 0.0f,
                RandomThrottleBias = 0.0f,
                TargetPointWeight = 0.0f
            },
            PhysicsProfile = PhysicsProfile.Default,
            Position = Vector2.Zero,
            Velocity = new Vector2(10.0f, 0.0f),
            Heading = 0.0f,
            Radius = 4.0f,
            Energy = 100.0f
        };

        var slowAgent = new Agent
        {
            Genome = fastAgent.Genome with { PreferredSpeedFraction = 0.1f },
            PhysicsProfile = PhysicsProfile.Default,
            Position = Vector2.Zero,
            Velocity = new Vector2(10.0f, 0.0f),
            Heading = 0.0f,
            Radius = 4.0f,
            Energy = 100.0f
        };

        var settings = new SimulationSettings { MaxSpeed = 100.0f };
        var observation = new AgentObservation(100.0f, 100.0f, 100.0f, Vector2.Zero, false, 10.0f, false);
        var fastIntent = AgentBrain.Decide(fastAgent, observation, settings);
        var slowIntent = AgentBrain.Decide(slowAgent, observation, settings);

        AssertTrue(fastIntent.LocalAccelerationWish.Y > slowIntent.LocalAccelerationWish.Y, "Preferred speed should diversify throttle behavior across agents.");
    }

    private static void TestActionResolverSumsWeightedContributionsByChannel()
    {
        var resolved = ActionSetResolver.Resolve(new[]
        {
            ActionContribution.WorldAcceleration(new Vector2(10.0f, 0.0f)),
            ActionContribution.WorldAcceleration(new Vector2(0.0f, 4.0f), weight: 0.5f),
            ActionContribution.LocalAcceleration(new Vector2(0.0f, 1.0f)),
            ActionContribution.Turn(1.5f),
            ActionContribution.Turn(-0.25f, weight: 2.0f),
            ActionContribution.Brake(0.3f),
            ActionContribution.Brake(0.2f, weight: 0.5f)
        });

        AssertNearlyEqual(10.0f, resolved.WorldAccelerationWish.X, 0.001f, "World X contributions should be accumulated.");
        AssertNearlyEqual(2.0f, resolved.WorldAccelerationWish.Y, 0.001f, "World Y contributions should respect weights.");
        AssertNearlyEqual(1.0f, resolved.LocalAccelerationWish.Y, 0.001f, "Local acceleration should accumulate separately.");
        AssertNearlyEqual(1.0f, resolved.TurnWish, 0.001f, "Turn contributions should be summed with weights.");
        AssertNearlyEqual(0.4f, resolved.BrakeWish, 0.001f, "Brake contributions should be summed with weights.");
    }

    private static void TestTimeBasedGeneRuleActivatesOnlyAfterThreshold()
    {
        var genome = AgentGenome.Default with
        {
            BaseTurnRate = 0.0f,
            WanderAmplitude = 0.0f,
            RandomTurnStrength = 0.0f,
            TargetPointWeight = 0.0f,
            WallAvoidanceWeight = 0.0f,
            NeighborAvoidanceWeight = 0.0f,
            Program = new GeneProgram(
                new GeneRule(
                    Enabled: true,
                    ConditionKind: GeneConditionKind.TimeGreaterOrEqual,
                    ConditionThreshold: 5.0f,
                    ActionKind: GeneActionKind.AddTurnBias,
                    Scalar: 1.25f,
                    TargetPointNormalized: new Vector2(0.5f, 0.5f)),
                GeneRule.Disabled)
        };

        var earlyAgent = new Agent
        {
            Genome = genome,
            PhysicsProfile = PhysicsProfile.Default,
            Position = Vector2.Zero,
            Velocity = Vector2.Zero,
            Heading = 0.0f,
            Radius = 4.0f,
            Energy = 100.0f,
            BrainClock = 2.0f
        };

        var lateAgent = new Agent
        {
            Genome = genome,
            PhysicsProfile = PhysicsProfile.Default,
            Position = Vector2.Zero,
            Velocity = Vector2.Zero,
            Heading = 0.0f,
            Radius = 4.0f,
            Energy = 100.0f,
            BrainClock = 6.0f
        };
        var observation = new AgentObservation(100.0f, 100.0f, 100.0f, Vector2.Zero, false, 0.0f, false);
        var settings = new SimulationSettings();

        var earlyIntent = AgentBrain.Decide(earlyAgent, observation, settings);
        var lateIntent = AgentBrain.Decide(lateAgent, observation, settings);

        AssertTrue(Mathf.Abs(earlyIntent.TurnWish) < 0.01f, "Time-gated rule should stay inactive before its threshold.");
        AssertTrue(lateIntent.TurnWish > 1.0f, "Time-gated rule should activate after its threshold.");
    }

    private static void TestGeneSteerToTargetRuleAddsWorldAcceleration()
    {
        var agent = new Agent
        {
            Genome = AgentGenome.Default with
            {
                BaseThrottle = 0.0f,
                PreferredSpeedFraction = 0.0f,
                BaseTurnRate = 0.0f,
                WanderAmplitude = 0.0f,
                RandomTurnStrength = 0.0f,
                RandomThrottleBias = 0.0f,
                TargetPointWeight = 0.0f,
                WallAvoidanceWeight = 0.0f,
                NeighborAvoidanceWeight = 0.0f,
                Program = new GeneProgram(
                    new GeneRule(
                        Enabled: true,
                        ConditionKind: GeneConditionKind.Always,
                        ConditionThreshold: 0.0f,
                        ActionKind: GeneActionKind.SteerToTarget,
                        Scalar: 1.0f,
                        TargetPointNormalized: new Vector2(1.0f, 0.5f)),
                    GeneRule.Disabled)
            },
            PhysicsProfile = PhysicsProfile.Default,
            Position = new Vector2(10.0f, 50.0f),
            Velocity = Vector2.Zero,
            Heading = 0.0f,
            Radius = 4.0f,
            Energy = 100.0f,
            BrainClock = 0.0f
        };

        var settings = new SimulationSettings { WorldSize = new Vector2(100.0f, 100.0f), Acceleration = 200.0f };
        var observation = new AgentObservation(100.0f, 100.0f, 100.0f, Vector2.Zero, false, 0.0f, false);
        var intent = AgentBrain.Decide(agent, observation, settings);

        AssertTrue(intent.WorldAccelerationWish.X > 0.1f, "Steer-to-target gene rules should produce world acceleration toward their target.");
    }
}
