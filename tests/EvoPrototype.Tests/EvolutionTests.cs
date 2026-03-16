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
}
