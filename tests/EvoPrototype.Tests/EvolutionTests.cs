using EvoPrototype.Core;
using EvoPrototype.Evolution;
using Godot;
using static TestAssertions;
using static TestFixtures;

public static class EvolutionTests
{
    public static void Register(TestHarness harness)
    {
        harness.Run("Region survival rule keeps agents inside configured zones", TestRegionSurvivalRuleKeepsAgentsInsideConfiguredZones);
        harness.Run("Circle survival zone accepts only positions inside radius", TestCircleSurvivalZoneAcceptsOnlyPositionsInsideRadius);
        harness.Run("Reproduction system restores target population from survivors", TestReproductionSystemRestoresTargetPopulationFromSurvivors);
        harness.Run("Reproduction system falls back when no survivor exists", TestReproductionSystemFallsBackWhenNoSurvivorExists);
        harness.Run("Evolution cycle advances generation after duration", TestEvolutionCycleAdvancesGenerationAfterDuration);
        harness.Run("Survival region factory builds four border bands", TestSurvivalRegionFactoryBuildsFourBorderBands);
        harness.Run("Survival region factory builds a centered circle", TestSurvivalRegionFactoryBuildsCenteredCircle);
        harness.Run("Survival region factory respects zone definitions", TestSurvivalRegionFactoryRespectsZoneDefinitions);
        harness.Run("Survival region factory uses normalized circle centers", TestSurvivalRegionFactoryUsesNormalizedCircleCenter);
        harness.Run("Gene pool can disable time-triggered rules", TestGenePoolCanDisableTimeTriggeredRules);
        harness.Run("Gene pool can restrict generated action families", TestGenePoolCanRestrictGeneratedActionFamilies);
        harness.Run("Mutation policy can preserve programs exactly", TestMutationPolicyCanPreserveProgramsExactly);
        harness.Run("Mutation policy can force action kind mutation", TestMutationPolicyCanForceActionKindMutation);
        harness.Run("Time threshold sampling stays inside the generation duration", TestTimeThresholdSamplingStaysInsideGenerationDuration);
        harness.Run("Time threshold mutation stays bounded and local", TestTimeThresholdMutationStaysBoundedAndLocal);
        harness.Run("Action scalar sampling and mutation stay inside action domains", TestActionScalarSamplingAndMutationStayInsideDomains);
        harness.Run("Target coordinates sampling and mutation stay normalized", TestTargetCoordinatesSamplingAndMutationStayNormalized);
    }

    private static void TestRegionSurvivalRuleKeepsAgentsInsideConfiguredZones()
    {
        var worldState = new WorldState(new Vector2(100.0f, 100.0f));
        var rule = new RegionSurvivalRule(new[]
        {
            (ISurvivalZone)new RectangleSurvivalZone(new Rect2(0.0f, 0.0f, 10.0f, 100.0f)),
            new RectangleSurvivalZone(new Rect2(90.0f, 0.0f, 10.0f, 100.0f))
        });

        var insideAgent = CreateTestAgent(new Vector2(5.0f, 50.0f));
        var outsideAgent = CreateTestAgent(new Vector2(50.0f, 50.0f));

        AssertTrue(rule.Survives(insideAgent, worldState, new EvolutionSettings()), "Agent inside a survival region should survive.");
        AssertTrue(!rule.Survives(outsideAgent, worldState, new EvolutionSettings()), "Agent outside all survival regions should die.");
    }

    private static void TestCircleSurvivalZoneAcceptsOnlyPositionsInsideRadius()
    {
        var zone = new CircleSurvivalZone(new Vector2(50.0f, 50.0f), 10.0f);

        AssertTrue(zone.Contains(new Vector2(50.0f, 50.0f)), "Circle zone should contain its center.");
        AssertTrue(zone.Contains(new Vector2(56.0f, 58.0f)), "Circle zone should contain nearby points within radius.");
        AssertTrue(!zone.Contains(new Vector2(70.0f, 50.0f)), "Circle zone should reject points outside radius.");
    }

    private static void TestReproductionSystemRestoresTargetPopulationFromSurvivors()
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

    private static void TestReproductionSystemFallsBackWhenNoSurvivorExists()
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

    private static void TestEvolutionCycleAdvancesGenerationAfterDuration()
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
                (ISurvivalZone)new RectangleSurvivalZone(new Rect2(0.0f, 0.0f, 10.0f, 100.0f))
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

    private static void TestSurvivalRegionFactoryBuildsFourBorderBands()
    {
        var regions = SurvivalRegionFactory.CreateBorderBands(new Vector2(200.0f, 100.0f), 12.0f);

        AssertTrue(regions.Count == 4, "Border band factory should create four regions.");
        var leftBand = (RectangleSurvivalZone)regions[0];
        var rightBand = (RectangleSurvivalZone)regions[1];
        var topBand = (RectangleSurvivalZone)regions[2];
        var bottomBand = (RectangleSurvivalZone)regions[3];

        AssertNearlyEqual(12.0f, leftBand.Bounds.Size.X, 0.0001f, "Left border band should use the configured thickness.");
        AssertNearlyEqual(188.0f, rightBand.Bounds.Position.X, 0.0001f, "Right border band should start at world width minus thickness.");
        AssertNearlyEqual(12.0f, topBand.Bounds.Size.Y, 0.0001f, "Top border band should use the configured thickness.");
        AssertNearlyEqual(88.0f, bottomBand.Bounds.Position.Y, 0.0001f, "Bottom border band should start at world height minus thickness.");
    }

    private static void TestSurvivalRegionFactoryBuildsCenteredCircle()
    {
        var zones = SurvivalRegionFactory.CreateCenteredCircle(new Vector2(200.0f, 100.0f), 24.0f);
        var circleZone = (CircleSurvivalZone)zones[0];

        AssertTrue(zones.Count == 1, "Centered circle factory should create exactly one zone.");
        AssertNearlyEqual(100.0f, circleZone.Center.X, 0.0001f, "Centered circle should use half the world width.");
        AssertNearlyEqual(50.0f, circleZone.Center.Y, 0.0001f, "Centered circle should use half the world height.");
        AssertNearlyEqual(24.0f, circleZone.Radius, 0.0001f, "Centered circle should preserve the requested radius.");
    }

    private static void TestSurvivalRegionFactoryRespectsZoneDefinitions()
    {
        var circleZones = SurvivalRegionFactory.CreateZones(
            new Vector2(200.0f, 100.0f),
            new SurvivalZoneDefinition(SurvivalZoneLayout.CenterCircle, new Vector2(0.5f, 0.5f), 0.2f, 12.0f));
        var bandZones = SurvivalRegionFactory.CreateZones(
            new Vector2(200.0f, 100.0f),
            new SurvivalZoneDefinition(SurvivalZoneLayout.BorderBands, new Vector2(0.5f, 0.5f), 0.2f, 16.0f));

        var circleZone = (CircleSurvivalZone)circleZones[0];
        var leftBand = (RectangleSurvivalZone)bandZones[0];

        AssertNearlyEqual(20.0f, circleZone.Radius, 0.0001f, "Zone definitions should scale centered circles from the smaller world side.");
        AssertNearlyEqual(16.0f, leftBand.Bounds.Size.X, 0.0001f, "Zone definitions should pass through border-band thickness.");
    }

    private static void TestSurvivalRegionFactoryUsesNormalizedCircleCenter()
    {
        var zones = SurvivalRegionFactory.CreateZones(
            new Vector2(200.0f, 100.0f),
            new SurvivalZoneDefinition(SurvivalZoneLayout.CenterCircle, new Vector2(0.25f, 0.75f), 0.1f, 12.0f));
        var circleZone = (CircleSurvivalZone)zones[0];

        AssertNearlyEqual(50.0f, circleZone.Center.X, 0.0001f, "Circle center X should scale from the normalized setup value.");
        AssertNearlyEqual(75.0f, circleZone.Center.Y, 0.0001f, "Circle center Y should scale from the normalized setup value.");
        AssertNearlyEqual(10.0f, circleZone.Radius, 0.0001f, "Circle radius should still scale from the smaller world side.");
    }

    private static void TestGenePoolCanDisableTimeTriggeredRules()
    {
        var random = new System.Random(123);
        var genePool = new GenePoolDefinition(
            AllowTimeCondition: false,
            AllowDistanceToPointCondition: false,
            AllowNeighborCountCondition: false,
            AllowTurnBiasAction: true,
            AllowThrottleBiasAction: true,
            AllowSteerToTargetAction: true,
            AllowBrakeAction: false,
            AllowRandomTurnAction: false,
            MaxRulesPerAgent: 2);

        for (var index = 0; index < 32; index++)
        {
            var program = GeneRuleFactory.CreateRandomProgram(random, genePool, generationDurationSeconds: 10.0f);
            AssertTrue(program.PrimaryRule.ConditionKind != GeneConditionKind.TimeGreaterOrEqual, "Disabled time conditions should never appear in generated primary rules.");
            AssertTrue(program.SecondaryRule.ConditionKind != GeneConditionKind.TimeGreaterOrEqual, "Disabled time conditions should never appear in generated secondary rules.");
        }
    }

    private static void TestGenePoolCanRestrictGeneratedActionFamilies()
    {
        var random = new System.Random(456);
        var genePool = new GenePoolDefinition(
            AllowTimeCondition: true,
            AllowDistanceToPointCondition: false,
            AllowNeighborCountCondition: false,
            AllowTurnBiasAction: false,
            AllowThrottleBiasAction: false,
            AllowSteerToTargetAction: true,
            AllowBrakeAction: false,
            AllowRandomTurnAction: false,
            MaxRulesPerAgent: 2);

        for (var index = 0; index < 32; index++)
        {
            var program = GeneRuleFactory.CreateRandomProgram(random, genePool, generationDurationSeconds: 10.0f);
            if (program.PrimaryRule.Enabled)
            {
                AssertTrue(program.PrimaryRule.ActionKind == GeneActionKind.SteerToTarget, "Restricted pools should only generate allowed primary actions.");
            }

            if (program.SecondaryRule.Enabled)
            {
                AssertTrue(program.SecondaryRule.ActionKind == GeneActionKind.SteerToTarget, "Restricted pools should only generate allowed secondary actions.");
            }
        }
    }

    private static void TestMutationPolicyCanPreserveProgramsExactly()
    {
        var random = new System.Random(111);
        var source = new GeneProgram(
            new GeneRule(
                Enabled: true,
                ConditionKind: GeneConditionKind.TimeGreaterOrEqual,
                ConditionThreshold: 3.0f,
                ActionKind: GeneActionKind.AddTurnBias,
                Scalar: 0.5f,
                TargetPointNormalized: new Vector2(0.4f, 0.6f)),
            GeneRule.Disabled);

        var preserved = GeneRuleFactory.MutateProgram(
            random,
            source,
            GenePoolDefinition.Default,
            MutationPolicy.Default with { ProgramMutationChance = 0.0f },
            generationDurationSeconds: 10.0f,
            mutationStrength: 0.2f);

        AssertTrue(preserved.Equals(source), "Zero program mutation chance should keep the parent program unchanged.");
    }

    private static void TestMutationPolicyCanForceActionKindMutation()
    {
        var random = new System.Random(222);
        var source = new GeneRule(
            Enabled: true,
            ConditionKind: GeneConditionKind.Always,
            ConditionThreshold: 0.0f,
            ActionKind: GeneActionKind.AddTurnBias,
            Scalar: 0.5f,
            TargetPointNormalized: new Vector2(0.5f, 0.5f));

        var mutated = GeneRuleFactory.MutateRule(
            random,
            source,
            new GenePoolDefinition(
                AllowTimeCondition: true,
                AllowDistanceToPointCondition: false,
                AllowNeighborCountCondition: false,
                AllowTurnBiasAction: false,
                AllowThrottleBiasAction: true,
                AllowSteerToTargetAction: false,
                AllowBrakeAction: false,
                AllowRandomTurnAction: false,
                MaxRulesPerAgent: 2),
            MutationPolicy.Default with
            {
                RemoveRuleChance = 0.0f,
                AddRuleChance = 0.0f,
                ConditionKindMutationChance = 0.0f,
                ActionKindMutationChance = 1.0f,
                ConditionParameterMutationChance = 0.0f,
                ActionParameterMutationChance = 0.0f
            },
            generationDurationSeconds: 10.0f,
            mutationStrength: 0.1f);

        AssertTrue(mutated.Enabled, "Forced action mutation should not disable the rule by itself.");
        AssertTrue(mutated.ActionKind == GeneActionKind.AddThrottleBias, "When turn bias is no longer allowed, forced action mutation should pick an allowed action.");
    }

    private static void TestTimeThresholdSamplingStaysInsideGenerationDuration()
    {
        var random = new System.Random(789);
        var genePool = GenePoolDefinition.Default;

        for (var index = 0; index < 48; index++)
        {
            var rule = GeneRuleFactory.CreateRandomRule(random, genePool, generationDurationSeconds: 14.0f, disableChance: 0.0d);
            if (rule.ConditionKind == GeneConditionKind.TimeGreaterOrEqual)
            {
                AssertTrue(rule.ConditionThreshold >= 0.0f && rule.ConditionThreshold <= 14.0f, "Time thresholds should stay inside the current generation duration.");
            }
        }
    }

    private static void TestTimeThresholdMutationStaysBoundedAndLocal()
    {
        var random = new System.Random(987);
        var source = new GeneRule(
            Enabled: true,
            ConditionKind: GeneConditionKind.TimeGreaterOrEqual,
            ConditionThreshold: 7.0f,
            ActionKind: GeneActionKind.AddTurnBias,
            Scalar: 0.8f,
            TargetPointNormalized: new Vector2(0.5f, 0.5f));

        var aggregateDelta = 0.0f;
        for (var index = 0; index < 32; index++)
        {
            var mutated = GeneRuleFactory.MutateRule(
                random,
                source,
                GenePoolDefinition.Default,
                MutationPolicy.Default,
                generationDurationSeconds: 14.0f,
                mutationStrength: 0.08f);

            if (!mutated.Enabled || mutated.ConditionKind != GeneConditionKind.TimeGreaterOrEqual)
            {
                continue;
            }

            AssertTrue(mutated.ConditionThreshold >= 0.0f && mutated.ConditionThreshold <= 14.0f, "Mutated time thresholds should remain bounded by the generation duration.");
            aggregateDelta += Mathf.Abs(mutated.ConditionThreshold - source.ConditionThreshold);
        }

        AssertTrue(aggregateDelta < 40.0f, "Low-strength beta mutations should stay reasonably local around the parent threshold.");
    }

    private static void TestActionScalarSamplingAndMutationStayInsideDomains()
    {
        var random = new System.Random(246);

        for (var index = 0; index < 40; index++)
        {
            var rule = GeneRuleFactory.CreateRandomRule(random, GenePoolDefinition.Default, generationDurationSeconds: 10.0f, disableChance: 0.0d);

            if (rule.ActionKind == GeneActionKind.AddTurnBias)
            {
                AssertTrue(rule.Scalar >= -2.2f && rule.Scalar <= 2.2f, "Turn-bias scalars should stay inside their bounded domain.");
            }
            else if (rule.ActionKind == GeneActionKind.AddThrottleBias)
            {
                AssertTrue(rule.Scalar >= -1.0f && rule.Scalar <= 1.0f, "Throttle-bias scalars should stay inside their bounded domain.");
            }
            else if (rule.ActionKind == GeneActionKind.SteerToTarget)
            {
                AssertTrue(rule.Scalar >= 0.0f && rule.Scalar <= 2.0f, "Steer-to-target weights should stay inside their bounded domain.");
            }
            else if (rule.ActionKind == GeneActionKind.AddBrake)
            {
                AssertTrue(rule.Scalar >= 0.1f && rule.Scalar <= 1.0f, "Brake scalars should stay inside their bounded domain.");
            }
            else if (rule.ActionKind == GeneActionKind.AddRandomTurn)
            {
                AssertTrue(rule.Scalar >= 0.3f && rule.Scalar <= 2.4f, "Random-turn scalars should stay inside their bounded domain.");
            }

            var mutated = GeneRuleFactory.MutateRule(
                random,
                rule,
                GenePoolDefinition.Default,
                MutationPolicy.Default,
                generationDurationSeconds: 10.0f,
                mutationStrength: 0.08f);

            if (!mutated.Enabled || mutated.ActionKind == GeneActionKind.None)
            {
                continue;
            }

            if (mutated.ActionKind == GeneActionKind.AddTurnBias)
            {
                AssertTrue(mutated.Scalar >= -2.2f && mutated.Scalar <= 2.2f, "Mutated turn-bias scalars should stay bounded.");
            }
            else if (mutated.ActionKind == GeneActionKind.AddThrottleBias)
            {
                AssertTrue(mutated.Scalar >= -1.0f && mutated.Scalar <= 1.0f, "Mutated throttle-bias scalars should stay bounded.");
            }
            else if (mutated.ActionKind == GeneActionKind.SteerToTarget)
            {
                AssertTrue(mutated.Scalar >= 0.0f && mutated.Scalar <= 2.0f, "Mutated steer-to-target weights should stay bounded.");
            }
            else if (mutated.ActionKind == GeneActionKind.AddBrake)
            {
                AssertTrue(mutated.Scalar >= 0.1f && mutated.Scalar <= 1.0f, "Mutated brake scalars should stay bounded.");
            }
            else if (mutated.ActionKind == GeneActionKind.AddRandomTurn)
            {
                AssertTrue(mutated.Scalar >= 0.3f && mutated.Scalar <= 2.4f, "Mutated random-turn scalars should stay bounded.");
            }
        }
    }

    private static void TestTargetCoordinatesSamplingAndMutationStayNormalized()
    {
        var random = new System.Random(135);
        var source = new GeneRule(
            Enabled: true,
            ConditionKind: GeneConditionKind.Always,
            ConditionThreshold: 0.0f,
            ActionKind: GeneActionKind.SteerToTarget,
            Scalar: 1.0f,
            TargetPointNormalized: new Vector2(0.75f, 0.2f));

        var aggregateShift = 0.0f;
        for (var index = 0; index < 40; index++)
        {
            var mutated = GeneRuleFactory.MutateRule(
                random,
                source,
                GenePoolDefinition.Default,
                MutationPolicy.Default,
                generationDurationSeconds: 10.0f,
                mutationStrength: 0.08f);

            AssertTrue(mutated.TargetPointNormalized.X >= 0.0f && mutated.TargetPointNormalized.X <= 1.0f, "Target X should remain normalized.");
            AssertTrue(mutated.TargetPointNormalized.Y >= 0.0f && mutated.TargetPointNormalized.Y <= 1.0f, "Target Y should remain normalized.");
            aggregateShift += Mathf.Abs(mutated.TargetPointNormalized.X - source.TargetPointNormalized.X);
            aggregateShift += Mathf.Abs(mutated.TargetPointNormalized.Y - source.TargetPointNormalized.Y);
        }

        AssertTrue(aggregateShift < 20.0f, "Low-strength target mutations should stay reasonably local in normalized space.");
    }
}
