using System.Collections.Generic;

namespace EvoPrototype.Cards;

/// <summary>
/// Central catalog of starter sandbox cards.
/// </summary>
public static class CardCatalog
{
    public static readonly CardDefinition MutationBoost = new(
        Id: "mutation_boost",
        Name: "Mutation Boost",
        Description: "Increase mutation strength for the current build.",
        EffectKind: CardEffectKind.IncreaseMutationStrength,
        ScalarValue: 0.02f);

    public static readonly CardDefinition TemporalLogic = new(
        Id: "temporal_logic",
        Name: "Temporal Logic",
        Description: "Allow time-based conditions inside gene rules.",
        EffectKind: CardEffectKind.EnableTimeCondition);

    public static readonly CardDefinition ExtraRuleSlot = new(
        Id: "extra_rule_slot",
        Name: "Extra Rule Slot",
        Description: "Increase the maximum number of rules an agent program may carry.",
        EffectKind: CardEffectKind.IncreaseMaxRulesPerProgram,
        IntValue: 1);

    public static readonly CardDefinition TargetSteering = new(
        Id: "target_steering",
        Name: "Target Steering",
        Description: "Allow steer-to-target rule actions.",
        EffectKind: CardEffectKind.EnableSteerToTargetAction);

    public static IReadOnlyList<CardDefinition> StarterSandboxDeck { get; } = new[]
    {
        MutationBoost,
        TemporalLogic,
        ExtraRuleSlot,
        TargetSteering
    };
}
