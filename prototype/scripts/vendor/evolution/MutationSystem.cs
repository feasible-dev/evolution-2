using System;
using EvoPrototype.Core;

namespace EvoPrototype.Evolution;

public sealed class MutationSystem
{
    private readonly Random _random;

    public MutationSystem(int? seed = null)
    {
        _random = seed.HasValue ? new Random(seed.Value) : new Random();
    }

    public AgentGenome MutateGenome(AgentGenome source, float mutationStrength)
    {
        return MutateGenome(source, mutationStrength, MutationPolicy.Default, GenePoolDefinition.Default);
    }

    public AgentGenome MutateGenome(
        AgentGenome source,
        float mutationStrength,
        MutationPolicy mutationPolicy,
        GenePoolDefinition genePool,
        float generationDurationSeconds = 10.0f,
        RuleMutationProfile? ruleMutationProfile = null)
    {
        return new AgentGenome(
            DecisionIntervalSeconds: Clamp(source.DecisionIntervalSeconds + NextSignedDelta(mutationStrength * 0.2f), 0.04f, 0.5f),
            DecisionIntervalJitterSeconds: Clamp(source.DecisionIntervalJitterSeconds + NextSignedDelta(mutationStrength * 0.1f), 0.0f, 0.2f),
            Program: GeneRuleFactory.MutateProgram(
                _random,
                source.Program,
                genePool,
                mutationPolicy,
                generationDurationSeconds,
                mutationStrength,
                ruleMutationProfile ?? new RuleMutationProfile()));
    }

    public PhysicsProfile MutatePhysicsProfile(PhysicsProfile source, float mutationStrength)
    {
        return source with
        {
            Mass = Clamp(source.Mass + NextSignedDelta(mutationStrength), 0.4f, 3.0f),
            MaxForwardAcceleration = Clamp(source.MaxForwardAcceleration + NextSignedDelta(mutationStrength * 120.0f), 60.0f, 400.0f),
            MaxLateralAcceleration = Clamp(source.MaxLateralAcceleration + NextSignedDelta(mutationStrength * 80.0f), 20.0f, 220.0f),
            MaxBrakeAcceleration = Clamp(source.MaxBrakeAcceleration + NextSignedDelta(mutationStrength * 100.0f), 40.0f, 320.0f),
            MaxTurnRate = Clamp(source.MaxTurnRate + NextSignedDelta(mutationStrength * 2.0f), 0.4f, 8.0f),
            LinearDrag = Clamp(source.LinearDrag + NextSignedDelta(mutationStrength * 0.3f), 0.0f, 1.5f),
            MaxEnergy = Clamp(source.MaxEnergy + NextSignedDelta(mutationStrength * 30.0f), 20.0f, 300.0f)
        };
    }

    private float NextSignedDelta(float magnitude)
    {
        return ((float)_random.NextDouble() * 2.0f - 1.0f) * magnitude;
    }

    private static float Clamp(float value, float minValue, float maxValue)
    {
        return Math.Clamp(value, minValue, maxValue);
    }
}
