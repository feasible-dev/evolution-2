namespace EvoPrototype.Evolution;

/// <summary>
/// Controls high-level mutation probabilities for agent rule programs.
/// Parameter amplitudes remain controlled separately for now.
/// </summary>
public readonly record struct MutationPolicy(
    float ProgramMutationChance,
    float AddRuleChance,
    float RemoveRuleChance,
    float ConditionKindMutationChance,
    float ActionKindMutationChance,
    float ConditionParameterMutationChance,
    float ActionParameterMutationChance)
{
    public static MutationPolicy Default => new(
        ProgramMutationChance: 0.10f,
        AddRuleChance: 0.04f,
        RemoveRuleChance: 0.03f,
        ConditionKindMutationChance: 0.08f,
        ActionKindMutationChance: 0.10f,
        ConditionParameterMutationChance: 0.28f,
        ActionParameterMutationChance: 0.32f);
}
