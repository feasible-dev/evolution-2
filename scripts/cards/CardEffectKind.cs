namespace EvoPrototype.Cards;

/// <summary>
/// Small initial set of card effects for the sandbox deck-building backend.
/// </summary>
public enum CardEffectKind
{
    IncreaseMutationStrength,
    IncreaseProgramMutationChance,
    IncreaseAddRuleChance,
    IncreaseRemoveRuleChance,
    IncreaseConditionKindMutationChance,
    IncreaseActionKindMutationChance,
    IncreaseActionParameterMutationChance,
    IncreaseConditionParameterMutationChance,
    EnableDistanceCondition,
    EnableNeighborCountCondition,
    EnableHalfSpaceCondition,
    EnableTurnBiasAction,
    EnableThrottleBiasAction,
    EnableBrakeAction,
    EnableRandomTurnAction,
    EnableTimeCondition,
    IncreaseMaxRulesPerProgram,
    EnableSteerToTargetAction
}
