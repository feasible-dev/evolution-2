using System;
using Godot;

namespace EvoPrototype.Core;

/// <summary>
/// Centralizes rule generation and mutation so future card effects can constrain the active gene pool.
/// </summary>
public static class GeneRuleFactory
{
    public static GeneProgram CreateRandomProgram(Random random, GenePoolDefinition genePool, float generationDurationSeconds)
    {
        var primaryRule = CreateRandomRule(random, genePool, generationDurationSeconds, disableChance: 0.35d);
        var secondaryRule = genePool.MaxRulesPerAgent >= 2
            ? CreateRandomRule(random, genePool, generationDurationSeconds, disableChance: 0.55d)
            : GeneRule.Disabled;

        return new GeneProgram(primaryRule, secondaryRule);
    }

    public static GeneProgram MutateProgram(
        Random random,
        GeneProgram source,
        GenePoolDefinition genePool,
        float generationDurationSeconds,
        float mutationStrength)
    {
        var primaryRule = MutateRule(random, source.PrimaryRule, genePool, generationDurationSeconds, mutationStrength);
        var secondaryRule = genePool.MaxRulesPerAgent >= 2
            ? MutateRule(random, source.SecondaryRule, genePool, generationDurationSeconds, mutationStrength)
            : GeneRule.Disabled;

        return new GeneProgram(primaryRule, secondaryRule);
    }

    public static GeneRule CreateRandomRule(Random random, GenePoolDefinition genePool, float generationDurationSeconds, double disableChance)
    {
        if (random.NextDouble() < disableChance)
        {
            return GeneRule.Disabled;
        }

        var conditionKind = PickConditionKind(random, genePool);
        var actionKind = PickActionKind(random, genePool);

        return new GeneRule(
            Enabled: true,
            ConditionKind: conditionKind,
            ConditionThreshold: conditionKind == GeneConditionKind.TimeGreaterOrEqual
                ? GetConditionThresholdSpec(generationDurationSeconds).SampleInitial(random)
                : 0.0f,
            ActionKind: actionKind,
            Scalar: GetActionScalarSpec(actionKind).SampleInitial(random),
            TargetPointNormalized: new Vector2(
                GetTargetCoordinateSpec().SampleInitial(random),
                GetTargetCoordinateSpec().SampleInitial(random)));
    }

    public static GeneRule MutateRule(Random random, GeneRule source, GenePoolDefinition genePool, float generationDurationSeconds, float mutationStrength)
    {
        var enabled = source.Enabled;
        if (random.NextDouble() < mutationStrength * 0.8f)
        {
            enabled = !enabled;
        }

        if (!enabled)
        {
            return GeneRule.Disabled;
        }

        var conditionKind = source.ConditionKind;
        if (random.NextDouble() < mutationStrength * 0.5f)
        {
            conditionKind = PickConditionKind(random, genePool);
        }
        else if (!ConditionAllowed(source.ConditionKind, genePool))
        {
            conditionKind = PickConditionKind(random, genePool);
        }

        var actionKind = source.ActionKind;
        if (actionKind == GeneActionKind.None || random.NextDouble() < mutationStrength * 0.6f)
        {
            actionKind = PickActionKind(random, genePool);
        }
        else if (!ActionAllowed(source.ActionKind, genePool))
        {
            actionKind = PickActionKind(random, genePool);
        }

        var threshold = conditionKind == GeneConditionKind.TimeGreaterOrEqual
            ? GetConditionThresholdSpec(generationDurationSeconds).Mutate(
                random,
                source.ConditionThreshold > 0.0f ? source.ConditionThreshold : generationDurationSeconds * 0.5f,
                mutationStrength)
            : 0.0f;

        var sourceScalar = source.ActionKind == actionKind
            ? source.Scalar
            : GetActionScalarDefault(actionKind);

        var scalar = GetActionScalarSpec(actionKind).Mutate(
            random,
            sourceScalar,
            mutationStrength);

        return new GeneRule(
            Enabled: true,
            ConditionKind: conditionKind,
            ConditionThreshold: threshold,
            ActionKind: actionKind,
            Scalar: scalar,
            TargetPointNormalized: new Vector2(
                GetTargetCoordinateSpec().Mutate(random, source.TargetPointNormalized.X, mutationStrength),
                GetTargetCoordinateSpec().Mutate(random, source.TargetPointNormalized.Y, mutationStrength)));
    }

    private static GeneConditionKind PickConditionKind(Random random, GenePoolDefinition genePool)
    {
        if (genePool.AllowTimeCondition && random.NextDouble() < 0.5d)
        {
            return GeneConditionKind.TimeGreaterOrEqual;
        }

        return GeneConditionKind.Always;
    }

    private static GeneActionKind PickActionKind(Random random, GenePoolDefinition genePool)
    {
        Span<GeneActionKind> available = stackalloc GeneActionKind[3];
        var count = 0;

        if (genePool.AllowTurnBiasAction)
        {
            available[count++] = GeneActionKind.AddTurnBias;
        }

        if (genePool.AllowThrottleBiasAction)
        {
            available[count++] = GeneActionKind.AddThrottleBias;
        }

        if (genePool.AllowSteerToTargetAction)
        {
            available[count++] = GeneActionKind.SteerToTarget;
        }

        return count == 0 ? GeneActionKind.None : available[random.Next(count)];
    }

    private static bool ConditionAllowed(GeneConditionKind conditionKind, GenePoolDefinition genePool)
    {
        return conditionKind switch
        {
            GeneConditionKind.Always => true,
            GeneConditionKind.TimeGreaterOrEqual => genePool.AllowTimeCondition,
            _ => false
        };
    }

    private static ParameterSpec GetConditionThresholdSpec(float generationDurationSeconds)
    {
        return new ParameterSpec(
            MinValue: 0.0f,
            MaxValue: MathF.Max(generationDurationSeconds, 0.01f),
            InitialDistribution: ParameterDistributionKind.Beta,
            MutationDistribution: ParameterDistributionKind.Beta,
            InitialAlpha: 2.0f,
            InitialBeta: 2.0f,
            MutationConcentrationMin: 3.5f,
            MutationConcentrationMax: 36.0f);
    }

    private static bool ActionAllowed(GeneActionKind actionKind, GenePoolDefinition genePool)
    {
        return actionKind switch
        {
            GeneActionKind.AddTurnBias => genePool.AllowTurnBiasAction,
            GeneActionKind.AddThrottleBias => genePool.AllowThrottleBiasAction,
            GeneActionKind.SteerToTarget => genePool.AllowSteerToTargetAction,
            GeneActionKind.None => true,
            _ => false
        };
    }

    private static ParameterSpec GetActionScalarSpec(GeneActionKind actionKind)
    {
        return actionKind switch
        {
            GeneActionKind.AddTurnBias => new ParameterSpec(
                MinValue: -2.2f,
                MaxValue: 2.2f,
                InitialDistribution: ParameterDistributionKind.Beta,
                MutationDistribution: ParameterDistributionKind.Beta,
                InitialAlpha: 2.0f,
                InitialBeta: 2.0f,
                MutationConcentrationMin: 3.0f,
                MutationConcentrationMax: 28.0f),
            GeneActionKind.AddThrottleBias => new ParameterSpec(
                MinValue: -1.0f,
                MaxValue: 1.0f,
                InitialDistribution: ParameterDistributionKind.Beta,
                MutationDistribution: ParameterDistributionKind.Beta,
                InitialAlpha: 2.2f,
                InitialBeta: 1.8f,
                MutationConcentrationMin: 3.0f,
                MutationConcentrationMax: 26.0f),
            GeneActionKind.SteerToTarget => new ParameterSpec(
                MinValue: 0.0f,
                MaxValue: 2.0f,
                InitialDistribution: ParameterDistributionKind.Beta,
                MutationDistribution: ParameterDistributionKind.Beta,
                InitialAlpha: 1.8f,
                InitialBeta: 2.2f,
                MutationConcentrationMin: 3.0f,
                MutationConcentrationMax: 26.0f),
            _ => new ParameterSpec(0.0f, 0.0f, ParameterDistributionKind.Uniform, ParameterDistributionKind.Uniform)
        };
    }

    private static float GetActionScalarDefault(GeneActionKind actionKind)
    {
        return actionKind switch
        {
            GeneActionKind.AddTurnBias => 0.0f,
            GeneActionKind.AddThrottleBias => 0.0f,
            GeneActionKind.SteerToTarget => 1.0f,
            _ => 0.0f
        };
    }

    private static ParameterSpec GetTargetCoordinateSpec()
    {
        return new ParameterSpec(
            MinValue: 0.0f,
            MaxValue: 1.0f,
            InitialDistribution: ParameterDistributionKind.Beta,
            MutationDistribution: ParameterDistributionKind.Beta,
            InitialAlpha: 2.0f,
            InitialBeta: 2.0f,
            MutationConcentrationMin: 3.0f,
            MutationConcentrationMax: 30.0f);
    }

    private static float NextFloat(Random random, float minValue, float maxValue)
    {
        return minValue + ((float)random.NextDouble() * (maxValue - minValue));
    }

    private static float Clamp(float value, float minValue, float maxValue)
    {
        return Math.Clamp(value, minValue, maxValue);
    }
}
