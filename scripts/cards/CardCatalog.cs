using System.Collections.Generic;

namespace EvoPrototype.Cards;

/// <summary>
/// Central catalog of starter sandbox cards.
/// </summary>
public static class CardCatalog
{
    public static readonly CardDefinition RelativeTurn = new(
        Id: "relative_turn",
        Name: "Relative Turn",
        Description: "Allow evolved rules to add local turning bias.",
        Category: CardCategory.Action,
        EffectKind: CardEffectKind.EnableTurnBiasAction);

    public static readonly CardDefinition RelativeImpulse = new(
        Id: "relative_impulse",
        Name: "Relative Impulse",
        Description: "Allow evolved rules to push forward or brake through local throttle bias.",
        Category: CardCategory.Action,
        EffectKind: CardEffectKind.EnableThrottleBiasAction);

    public static readonly CardDefinition MutationBoost = new(
        Id: "mutation_boost",
        Name: "Mutation Boost",
        Description: "Increase mutation strength for the current build.",
        Category: CardCategory.Mutation,
        EffectKind: CardEffectKind.IncreaseMutationStrength,
        ScalarValue: 0.02f);

    public static readonly CardDefinition TemporalLogic = new(
        Id: "temporal_logic",
        Name: "Temporal Logic",
        Description: "Allow time-based conditions inside gene rules.",
        Category: CardCategory.Condition,
        EffectKind: CardEffectKind.EnableTimeCondition);

    public static readonly CardDefinition ExtraRuleSlot = new(
        Id: "extra_rule_slot",
        Name: "Extra Rule Slot",
        Description: "Increase the maximum number of rules an agent program may carry.",
        Category: CardCategory.Program,
        EffectKind: CardEffectKind.IncreaseMaxRulesPerProgram,
        IntValue: 1);

    public static readonly CardDefinition TargetSteering = new(
        Id: "target_steering",
        Name: "Target Steering",
        Description: "Allow steer-to-target rule actions.",
        Category: CardCategory.Action,
        EffectKind: CardEffectKind.EnableSteerToTargetAction);

    public static IReadOnlyList<CardDefinition> StarterSandboxDeck { get; } = new[]
    {
        RelativeTurn,
        RelativeImpulse,
        MutationBoost,
        TemporalLogic,
        ExtraRuleSlot,
        TargetSteering
    };

    public static IReadOnlyList<string> DefaultActiveCardIds { get; } = new[]
    {
        RelativeTurn.Id,
        RelativeImpulse.Id,
        TargetSteering.Id
    };
}
