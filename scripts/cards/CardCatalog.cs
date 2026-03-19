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

    public static readonly CardDefinition Brake = new(
        Id: "brake",
        Name: "Brake",
        Description: "Allow evolved rules to issue direct braking actions.",
        Category: CardCategory.Action,
        EffectKind: CardEffectKind.EnableBrakeAction);

    public static readonly CardDefinition RandomTurn = new(
        Id: "random_turn",
        Name: "Random Turn",
        Description: "Allow evolved rules to inject noisy turning impulses.",
        Category: CardCategory.Action,
        EffectKind: CardEffectKind.EnableRandomTurnAction);

    public static readonly CardDefinition MutationBoost = new(
        Id: "mutation_boost",
        Name: "Mutation Boost",
        Description: "Increase numeric mutation amplitude for the current build.",
        Category: CardCategory.Mutation,
        EffectKind: CardEffectKind.IncreaseMutationStrength,
        ScalarValue: 0.02f);

    public static readonly CardDefinition ProgramMutation = new(
        Id: "program_mutation",
        Name: "Program Mutation",
        Description: "Increase the chance that full rule programs mutate each generation.",
        Category: CardCategory.Mutation,
        EffectKind: CardEffectKind.IncreaseProgramMutationChance,
        ScalarValue: 0.08f);

    public static readonly CardDefinition RuleGrowth = new(
        Id: "rule_growth",
        Name: "Rule Growth",
        Description: "Increase the chance that mutation adds a new rule to a program.",
        Category: CardCategory.Mutation,
        EffectKind: CardEffectKind.IncreaseAddRuleChance,
        ScalarValue: 0.05f);

    public static readonly CardDefinition RulePruning = new(
        Id: "rule_pruning",
        Name: "Rule Pruning",
        Description: "Increase the chance that mutation removes an existing rule.",
        Category: CardCategory.Mutation,
        EffectKind: CardEffectKind.IncreaseRemoveRuleChance,
        ScalarValue: 0.03f);

    public static readonly CardDefinition ConditionShift = new(
        Id: "condition_shift",
        Name: "Condition Shift",
        Description: "Increase the chance that a rule changes its condition kind.",
        Category: CardCategory.Mutation,
        EffectKind: CardEffectKind.IncreaseConditionKindMutationChance,
        ScalarValue: 0.08f);

    public static readonly CardDefinition ActionShift = new(
        Id: "action_shift",
        Name: "Action Shift",
        Description: "Increase the chance that a rule changes its action kind.",
        Category: CardCategory.Mutation,
        EffectKind: CardEffectKind.IncreaseActionKindMutationChance,
        ScalarValue: 0.08f);

    public static readonly CardDefinition ActionParameterMutation = new(
        Id: "action_parameter_mutation",
        Name: "Action Parameter Mutation",
        Description: "Increase mutation on rule action parameters.",
        Category: CardCategory.Mutation,
        EffectKind: CardEffectKind.IncreaseActionParameterMutationChance,
        ScalarValue: 0.10f);

    public static readonly CardDefinition ConditionParameterMutation = new(
        Id: "condition_parameter_mutation",
        Name: "Condition Parameter Mutation",
        Description: "Increase mutation on rule condition parameters.",
        Category: CardCategory.Mutation,
        EffectKind: CardEffectKind.IncreaseConditionParameterMutationChance,
        ScalarValue: 0.10f);

    public static readonly CardDefinition TemporalLogic = new(
        Id: "temporal_logic",
        Name: "Temporal Logic",
        Description: "Allow time-based conditions inside gene rules.",
        Category: CardCategory.Condition,
        EffectKind: CardEffectKind.EnableTimeCondition);

    public static readonly CardDefinition TargetRadius = new(
        Id: "target_radius",
        Name: "Target Radius",
        Description: "Allow conditions based on distance to a target point.",
        Category: CardCategory.Condition,
        EffectKind: CardEffectKind.EnableDistanceCondition);

    public static readonly CardDefinition CrowdSense = new(
        Id: "crowd_sense",
        Name: "Crowd Sense",
        Description: "Allow conditions based on nearby neighbor count.",
        Category: CardCategory.Condition,
        EffectKind: CardEffectKind.EnableNeighborCountCondition);

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
        Brake,
        RandomTurn,
        MutationBoost,
        ProgramMutation,
        RuleGrowth,
        RulePruning,
        ConditionShift,
        ActionShift,
        ActionParameterMutation,
        ConditionParameterMutation,
        TemporalLogic,
        TargetRadius,
        CrowdSense,
        ExtraRuleSlot,
        TargetSteering
    };

    public static IReadOnlyList<string> DefaultActiveCardIds { get; } = new[]
    {
        RelativeTurn.Id,
        RelativeImpulse.Id,
        TargetSteering.Id,
        Brake.Id,
        RandomTurn.Id,
        TargetRadius.Id,
        CrowdSense.Id
    };
}
