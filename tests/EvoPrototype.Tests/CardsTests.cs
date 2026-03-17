using EvoPrototype.Cards;
using EvoPrototype.Core;
using EvoPrototype.Game;
using EvoPrototype.Gameplay;
using Godot;
using static TestAssertions;

public static class CardsTests
{
    public static void Register(TestHarness harness)
    {
        harness.Run("Cards compile into a build and modify mutation strength", TestCardsCompileIntoBuildAndModifyMutationStrength);
        harness.Run("Cards can increase the maximum number of rules", TestCardsCanIncreaseMaximumNumberOfRules);
        harness.Run("Compiled build preserves unrelated run configuration values", TestCompiledBuildPreservesUnrelatedRunConfigurationValues);
    }

    private static void TestCardsCompileIntoBuildAndModifyMutationStrength()
    {
        var baseConfiguration = new RunConfiguration
        {
            MutationStrength = 0.03f,
            GenePool = GenePoolDefinition.Default with
            {
                AllowTimeCondition = false,
                AllowSteerToTargetAction = false
            }
        };

        var build = EvolutionBuildCompiler.CreateFromBase(
            baseConfiguration,
            new[]
            {
                CardCatalog.MutationBoost,
                CardCatalog.TemporalLogic,
                CardCatalog.TargetSteering
            });

        AssertNearlyEqual(0.05f, build.MutationStrength, 0.0001f, "Mutation boost cards should modify the build mutation strength.");
        AssertTrue(build.GenePool.AllowTimeCondition, "Temporal logic cards should enable time-based conditions.");
        AssertTrue(build.GenePool.AllowSteerToTargetAction, "Target steering cards should enable target-steering actions.");
    }

    private static void TestCardsCanIncreaseMaximumNumberOfRules()
    {
        var baseConfiguration = new RunConfiguration
        {
            GenePool = GenePoolDefinition.Default with { MaxRulesPerAgent = 2 }
        };

        var build = EvolutionBuildCompiler.CreateFromBase(
            baseConfiguration,
            new[]
            {
                CardCatalog.ExtraRuleSlot,
                CardCatalog.ExtraRuleSlot
            });

        AssertTrue(build.GenePool.MaxRulesPerAgent == 4, "Extra-rule cards should increase the rule budget additively.");
    }

    private static void TestCompiledBuildPreservesUnrelatedRunConfigurationValues()
    {
        var baseConfiguration = new RunConfiguration
        {
            AgentCount = 240,
            WorldSize = new Vector2(900.0f, 500.0f),
            MutationStrength = 0.03f,
            GenePool = GenePoolDefinition.Default with { MaxRulesPerAgent = 2 }
        };

        var build = EvolutionBuildCompiler.CreateFromBase(baseConfiguration, new[] { CardCatalog.ExtraRuleSlot });
        var compiled = EvolutionBuildCompiler.Compile(baseConfiguration, build);

        AssertTrue(compiled.AgentCount == 240, "Compiling a build should preserve unrelated runtime values such as agent count.");
        AssertNearlyEqual(900.0f, compiled.WorldSize.X, 0.0001f, "Compiling a build should preserve world size.");
        AssertTrue(compiled.GenePool.MaxRulesPerAgent == 3, "Compiled builds should inject the updated gene-rule budget.");
    }
}
