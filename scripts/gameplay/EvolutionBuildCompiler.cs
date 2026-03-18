using System.Collections.Generic;
using EvoPrototype.Cards;
using EvoPrototype.Game;

namespace EvoPrototype.Gameplay;

/// <summary>
/// Applies cards to a build and produces a concrete run configuration for the simulation.
/// </summary>
public static class EvolutionBuildCompiler
{
    public static EvolutionBuild CreateFromBase(RunConfiguration baseConfiguration, IReadOnlyList<CardDefinition> cards)
    {
        var build = EvolutionBuild.FromDefaults(
            mutationStrength: baseConfiguration.MutationStrength,
            mutationPolicy: baseConfiguration.MutationPolicy,
            genePool: baseConfiguration.GenePool,
            survivalZoneDefinition: baseConfiguration.SurvivalZoneDefinition);

        foreach (var card in cards)
        {
            ApplyCard(build, card);
        }

        return build;
    }

    public static RunConfiguration Compile(RunConfiguration baseConfiguration, EvolutionBuild build)
    {
        var compiled = baseConfiguration.Clone();
        compiled.MutationStrength = build.MutationStrength;
        compiled.MutationPolicy = build.MutationPolicy;
        compiled.GenePool = build.GenePool;
        compiled.SurvivalZoneDefinition = build.SurvivalZoneDefinition;
        return compiled;
    }

    private static void ApplyCard(EvolutionBuild build, CardDefinition card)
    {
        build.AppliedCards.Add(card);

        switch (card.EffectKind)
        {
            case CardEffectKind.IncreaseMutationStrength:
                build.MutationStrength += card.ScalarValue;
                break;

            case CardEffectKind.IncreaseProgramMutationChance:
                build.MutationPolicy = build.MutationPolicy with
                {
                    ProgramMutationChance = Clamp01(build.MutationPolicy.ProgramMutationChance + card.ScalarValue)
                };
                break;

            case CardEffectKind.IncreaseAddRuleChance:
                build.MutationPolicy = build.MutationPolicy with
                {
                    AddRuleChance = Clamp01(build.MutationPolicy.AddRuleChance + card.ScalarValue)
                };
                break;

            case CardEffectKind.IncreaseRemoveRuleChance:
                build.MutationPolicy = build.MutationPolicy with
                {
                    RemoveRuleChance = Clamp01(build.MutationPolicy.RemoveRuleChance + card.ScalarValue)
                };
                break;

            case CardEffectKind.IncreaseConditionKindMutationChance:
                build.MutationPolicy = build.MutationPolicy with
                {
                    ConditionKindMutationChance = Clamp01(build.MutationPolicy.ConditionKindMutationChance + card.ScalarValue)
                };
                break;

            case CardEffectKind.IncreaseActionKindMutationChance:
                build.MutationPolicy = build.MutationPolicy with
                {
                    ActionKindMutationChance = Clamp01(build.MutationPolicy.ActionKindMutationChance + card.ScalarValue)
                };
                break;

            case CardEffectKind.IncreaseActionParameterMutationChance:
                build.MutationPolicy = build.MutationPolicy with
                {
                    ActionParameterMutationChance = Clamp01(build.MutationPolicy.ActionParameterMutationChance + card.ScalarValue)
                };
                break;

            case CardEffectKind.IncreaseConditionParameterMutationChance:
                build.MutationPolicy = build.MutationPolicy with
                {
                    ConditionParameterMutationChance = Clamp01(build.MutationPolicy.ConditionParameterMutationChance + card.ScalarValue)
                };
                break;

            case CardEffectKind.EnableTurnBiasAction:
                build.GenePool = build.GenePool with { AllowTurnBiasAction = true };
                break;

            case CardEffectKind.EnableThrottleBiasAction:
                build.GenePool = build.GenePool with { AllowThrottleBiasAction = true };
                break;

            case CardEffectKind.EnableTimeCondition:
                build.GenePool = build.GenePool with { AllowTimeCondition = true };
                break;

            case CardEffectKind.IncreaseMaxRulesPerProgram:
                build.GenePool = build.GenePool with
                {
                    MaxRulesPerAgent = build.GenePool.MaxRulesPerAgent + card.IntValue
                };
                break;

            case CardEffectKind.EnableSteerToTargetAction:
                build.GenePool = build.GenePool with { AllowSteerToTargetAction = true };
                break;
        }
    }

    private static float Clamp01(float value)
    {
        return value < 0.0f ? 0.0f : value > 1.0f ? 1.0f : value;
    }
}
