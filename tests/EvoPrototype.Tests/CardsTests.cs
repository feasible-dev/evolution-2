using System;
using EvoPrototype.Cards;
using EvoPrototype.Core;
using EvoPrototype.Evolution;
using EvoPrototype.Game;
using EvoPrototype.Gameplay;
using Godot;
using static TestAssertions;

public static class CardsTests
{
    public static void Register(TestHarness harness)
    {
        harness.Run("Starter sandbox cards activate the baseline movement language", TestStarterSandboxCardsActivateBaselineMovementLanguage);
        harness.Run("Card selection state activates and deactivates cards without duplicates", TestCardSelectionStateActivatesAndDeactivatesCardsWithoutDuplicates);
        harness.Run("Cards compile into a build and modify mutation strength", TestCardsCompileIntoBuildAndModifyMutationStrength);
        harness.Run("Compiled builds preserve mutation policy", TestCompiledBuildPreservesMutationPolicy);
        harness.Run("Run configuration forwards mutation policy to evolution settings", TestRunConfigurationForwardsMutationPolicyToEvolutionSettings);
        harness.Run("Cards can enable relative and absolute rule actions", TestCardsCanEnableRelativeAndAbsoluteRuleActions);
        harness.Run("Cards can increase the maximum number of rules", TestCardsCanIncreaseMaximumNumberOfRules);
        harness.Run("Compiled build preserves unrelated run configuration values", TestCompiledBuildPreservesUnrelatedRunConfigurationValues);
    }

    private static void TestStarterSandboxCardsActivateBaselineMovementLanguage()
    {
        var selectionState = new CardSelectionState(CardCatalog.StarterSandboxDeck, CardCatalog.DefaultActiveCardIds);

        AssertTrue(selectionState.IsActive(CardCatalog.RelativeTurn.Id), "Relative turn should be active by default.");
        AssertTrue(selectionState.IsActive(CardCatalog.RelativeImpulse.Id), "Relative impulse should be active by default.");
        AssertTrue(selectionState.IsActive(CardCatalog.TargetSteering.Id), "Target steering should be active by default.");
    }

    private static void TestCardSelectionStateActivatesAndDeactivatesCardsWithoutDuplicates()
    {
        var selectionState = new CardSelectionState(CardCatalog.StarterSandboxDeck);

        AssertTrue(selectionState.TryActivate(CardCatalog.RelativeImpulse.Id), "A starter card should be activatable.");
        AssertTrue(!selectionState.TryActivate(CardCatalog.RelativeImpulse.Id), "The same card should not be activatable twice.");
        AssertTrue(selectionState.ActiveCards.Count == 1, "Active card count should not grow when adding duplicates.");
        AssertTrue(selectionState.TryDeactivate(CardCatalog.RelativeImpulse.Id), "An active card should be removable.");
        AssertTrue(selectionState.ActiveCards.Count == 0, "Removing an active card should empty the active set.");
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

    private static void TestCardsCanEnableRelativeAndAbsoluteRuleActions()
    {
        var baseConfiguration = new RunConfiguration
        {
            GenePool = new GenePoolDefinition(
                AllowTimeCondition: false,
                AllowTurnBiasAction: false,
                AllowThrottleBiasAction: false,
                AllowSteerToTargetAction: false,
                MaxRulesPerAgent: 1)
        };

        var build = EvolutionBuildCompiler.CreateFromBase(
            baseConfiguration,
            new[]
            {
                CardCatalog.RelativeTurn,
                CardCatalog.RelativeImpulse,
                CardCatalog.TargetSteering
            });

        AssertTrue(build.GenePool.AllowTurnBiasAction, "Relative-turn cards should enable turn-bias actions.");
        AssertTrue(build.GenePool.AllowThrottleBiasAction, "Relative-impulse cards should enable throttle-bias actions.");
        AssertTrue(build.GenePool.AllowSteerToTargetAction, "Target-steering cards should enable absolute target actions.");
    }

    private static void TestCompiledBuildPreservesMutationPolicy()
    {
        var baseConfiguration = new RunConfiguration
        {
            MutationPolicy = MutationPolicy.Default with
            {
                ProgramMutationChance = 0.42f,
                AddRuleChance = 0.11f,
                RemoveRuleChance = 0.09f,
                ConditionKindMutationChance = 0.21f,
                ActionKindMutationChance = 0.34f,
                ConditionParameterMutationChance = 0.55f,
                ActionParameterMutationChance = 0.67f
            }
        };

        var build = EvolutionBuildCompiler.CreateFromBase(baseConfiguration, Array.Empty<CardDefinition>());
        var compiled = EvolutionBuildCompiler.Compile(baseConfiguration, build);

        AssertNearlyEqual(0.42f, compiled.MutationPolicy.ProgramMutationChance, 0.0001f, "Compiled builds should preserve program mutation chance.");
        AssertNearlyEqual(0.67f, compiled.MutationPolicy.ActionParameterMutationChance, 0.0001f, "Compiled builds should preserve action-parameter mutation chance.");
    }

    private static void TestRunConfigurationForwardsMutationPolicyToEvolutionSettings()
    {
        var runConfiguration = new RunConfiguration
        {
            MutationPolicy = MutationPolicy.Default with
            {
                ProgramMutationChance = 0.31f,
                AddRuleChance = 0.12f,
                RemoveRuleChance = 0.07f,
                ConditionKindMutationChance = 0.18f,
                ActionKindMutationChance = 0.27f,
                ConditionParameterMutationChance = 0.49f,
                ActionParameterMutationChance = 0.58f
            }
        };

        var evolutionSettings = runConfiguration.ToEvolutionSettings();

        AssertNearlyEqual(0.31f, evolutionSettings.MutationPolicy.ProgramMutationChance, 0.0001f, "Evolution settings should receive the configured program mutation chance.");
        AssertNearlyEqual(0.58f, evolutionSettings.MutationPolicy.ActionParameterMutationChance, 0.0001f, "Evolution settings should receive the configured action-parameter mutation chance.");
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
