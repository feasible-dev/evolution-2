using System;

namespace EvoPrototype.Core;

/// <summary>
/// Defines the valid domain and sampling behavior of a rule parameter.
/// </summary>
public readonly record struct ParameterSpec(
    float MinValue,
    float MaxValue,
    ParameterDistributionKind InitialDistribution,
    ParameterDistributionKind MutationDistribution,
    float InitialAlpha = 2.0f,
    float InitialBeta = 2.0f,
    float MutationConcentrationMin = 3.0f,
    float MutationConcentrationMax = 32.0f)
{
    public float Clamp(float value)
    {
        return Math.Clamp(value, MinValue, MaxValue);
    }

    public float SampleInitial(Random random)
    {
        var normalized = InitialDistribution switch
        {
            ParameterDistributionKind.Beta => RandomDistributions.SampleBeta(random, InitialAlpha, InitialBeta),
            _ => (float)random.NextDouble()
        };

        return Denormalize(normalized);
    }

    public float Mutate(Random random, float currentValue, float mutationStrength)
    {
        var currentNormalized = Normalize(currentValue);

        var mutatedNormalized = MutationDistribution switch
        {
            ParameterDistributionKind.Beta => SampleLocalBeta(
                random,
                currentNormalized,
                mutationStrength),
            _ => (float)random.NextDouble()
        };

        return Denormalize(mutatedNormalized);
    }

    private float Normalize(float value)
    {
        if (MaxValue <= MinValue)
        {
            return 0.0f;
        }

        return Math.Clamp((value - MinValue) / (MaxValue - MinValue), 0.0f, 1.0f);
    }

    private float Denormalize(float value)
    {
        return MinValue + (Math.Clamp(value, 0.0f, 1.0f) * (MaxValue - MinValue));
    }

    private float SampleLocalBeta(Random random, float currentNormalized, float mutationStrength)
    {
        var mean = Math.Clamp(currentNormalized, 0.02f, 0.98f);
        var concentration = MutationConcentrationMax +
                            ((MutationConcentrationMin - MutationConcentrationMax) * Math.Clamp(mutationStrength, 0.0f, 1.0f));

        var alpha = Math.Max(0.05f, mean * concentration);
        var beta = Math.Max(0.05f, (1.0f - mean) * concentration);
        return RandomDistributions.SampleBeta(random, alpha, beta);
    }
}
