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
        harness.Run("Reproduction system restores target population from survivors", TestReproductionSystemRestoresTargetPopulationFromSurvivors);
        harness.Run("Reproduction system falls back when no survivor exists", TestReproductionSystemFallsBackWhenNoSurvivorExists);
        harness.Run("Evolution cycle advances generation after duration", TestEvolutionCycleAdvancesGenerationAfterDuration);
        harness.Run("Survival region factory builds four border bands", TestSurvivalRegionFactoryBuildsFourBorderBands);
    }

    private static void TestRegionSurvivalRuleKeepsAgentsInsideConfiguredZones()
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

    private static void TestSurvivalRegionFactoryBuildsFourBorderBands()
    {
        var regions = SurvivalRegionFactory.CreateBorderBands(new Vector2(200.0f, 100.0f), 12.0f);

        AssertTrue(regions.Count == 4, "Border band factory should create four regions.");
        AssertNearlyEqual(12.0f, regions[0].Bounds.Size.X, 0.0001f, "Left border band should use the configured thickness.");
        AssertNearlyEqual(188.0f, regions[1].Bounds.Position.X, 0.0001f, "Right border band should start at world width minus thickness.");
        AssertNearlyEqual(12.0f, regions[2].Bounds.Size.Y, 0.0001f, "Top border band should use the configured thickness.");
        AssertNearlyEqual(88.0f, regions[3].Bounds.Position.Y, 0.0001f, "Bottom border band should start at world height minus thickness.");
    }
}
