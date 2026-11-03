using System.Collections.Generic;
using EvoPrototype.Core;

namespace EvoPrototype.Evolution;

/// <summary>
/// Per-rule mutation tuning compiled from configurable cards.
/// Values are absolute probabilities used when a rule currently relies on a given condition or action family.
/// </summary>
public sealed class RuleMutationProfile
{
    public static RuleMutationProfile Empty => new();

    public Dictionary<GeneConditionKind, float> ConditionMutationRates { get; init; } = new();

    public Dictionary<GeneActionKind, float> ActionMutationRates { get; init; } = new();

    public Dictionary<GeneConditionKind, float> ConditionMutationAmplitudes { get; init; } = new();

    public Dictionary<GeneActionKind, float> ActionMutationAmplitudes { get; init; } = new();

    public Dictionary<GeneConditionKind, float> ConditionPopularities { get; init; } = new();

    public Dictionary<GeneActionKind, float> ActionPopularities { get; init; } = new();

    public float GetConditionMutationRate(GeneConditionKind kind, float fallback)
    {
        return ConditionMutationRates.TryGetValue(kind, out var value) ? value : fallback;
    }

    public float GetActionMutationRate(GeneActionKind kind, float fallback)
    {
        return ActionMutationRates.TryGetValue(kind, out var value) ? value : fallback;
    }

    public float GetConditionMutationAmplitude(GeneConditionKind kind, float fallback)
    {
        return ConditionMutationAmplitudes.TryGetValue(kind, out var value) ? value : fallback;
    }

    public float GetActionMutationAmplitude(GeneActionKind kind, float fallback)
    {
        return ActionMutationAmplitudes.TryGetValue(kind, out var value) ? value : fallback;
    }

    public float GetConditionPopularity(GeneConditionKind kind, float fallback)
    {
        return ConditionPopularities.TryGetValue(kind, out var value) ? value : fallback;
    }

    public float GetActionPopularity(GeneActionKind kind, float fallback)
    {
        return ActionPopularities.TryGetValue(kind, out var value) ? value : fallback;
    }
}
