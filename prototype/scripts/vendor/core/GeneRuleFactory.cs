using System;
using EvoPrototype.Evolution;
using Godot;

namespace EvoPrototype.Core;

/// <summary>
/// Centralizes rule generation and mutation so future card effects can constrain the active gene pool.
/// </summary>
public static class GeneRuleFactory
{
    public static float MutateDirection(float angle, float sample, float strength)
        => Mathf.PosMod(angle + (sample * 2 - 1) * MathF.Max(0, strength), 1);
    public static GeneProgram CreateRandomProgram(Random random, GenePoolDefinition genePool, float generationDurationSeconds)
    {
        var primaryRule = CreateRandomRule(random, genePool, generationDurationSeconds, disableChance: 0.35d, RuleMutationProfile.Empty);
        var secondaryRule = genePool.MaxRulesPerAgent >= 2
            ? CreateRandomRule(random, genePool, generationDurationSeconds, disableChance: 0.55d, RuleMutationProfile.Empty)
            : GeneRule.Disabled;
        var tertiaryRule = genePool.MaxRulesPerAgent >= 3
            ? CreateRandomRule(random, genePool, generationDurationSeconds, disableChance: 0.70d, RuleMutationProfile.Empty)
            : GeneRule.Disabled;

        return new GeneProgram(primaryRule, secondaryRule, tertiaryRule);
    }

    public static GeneProgram MutateProgram(
        Random random,
        GeneProgram source,
        GenePoolDefinition genePool,
        MutationPolicy mutationPolicy,
        float generationDurationSeconds,
        float mutationStrength,
        RuleMutationProfile ruleMutationProfile)
    {
        if (random.NextDouble() >= mutationPolicy.ProgramMutationChance)
        {
            return source;
        }

        var primaryRule = MutateRule(
            random,
                source.PrimaryRule,
                genePool,
                mutationPolicy,
                generationDurationSeconds,
                mutationStrength,
                ruleMutationProfile);
        var secondaryRule = genePool.MaxRulesPerAgent >= 2
            ? MutateRule(
                random,
                source.SecondaryRule,
                genePool,
                mutationPolicy,
                generationDurationSeconds,
                mutationStrength,
                ruleMutationProfile)
            : GeneRule.Disabled;
        var tertiaryRule = genePool.MaxRulesPerAgent >= 3
            ? MutateRule(
                random,
                source.TertiaryRule,
                genePool,
                mutationPolicy,
                generationDurationSeconds,
                mutationStrength,
                ruleMutationProfile)
            : GeneRule.Disabled;

        return new GeneProgram(primaryRule, secondaryRule, tertiaryRule);
    }

    public static GeneRule CreateRandomRule(Random random, GenePoolDefinition genePool, float generationDurationSeconds, double disableChance, RuleMutationProfile ruleMutationProfile)
    {
        if (random.NextDouble() < disableChance)
        {
            return GeneRule.Disabled;
        }

        for (var attempt = 0; attempt < 16; attempt++)
        {
            var conditionKind = PickConditionKind(random, genePool, ruleMutationProfile);
            var actionKind = PickActionKind(random, genePool, ruleMutationProfile);
            var candidate = new GeneRule(
                Enabled: true,
                ConditionKind: conditionKind,
                ConditionThreshold: GetConditionThresholdSpec(conditionKind, generationDurationSeconds).SampleInitial(random),
                ActionKind: actionKind,
                Scalar: GetActionScalarSpec(actionKind).SampleInitial(random),
                TargetPointNormalized: new Vector2(
                    GetTargetCoordinateSpec().SampleInitial(random),
                    GetTargetCoordinateSpec().SampleInitial(random)))
            {
                ConditionPointNormalized = new Vector2(
                    GetTargetCoordinateSpec().SampleInitial(random),
                    GetTargetCoordinateSpec().SampleInitial(random)),
                ConditionOrientationNormalized = GetConditionOrientationSpec(conditionKind).SampleInitial(random),
                DirectionAngleNormalized = actionKind == GeneActionKind.AccelerateDirection ? (float)random.NextDouble() : 0
            };

            if (!IsForbiddenRule(candidate))
            {
                return candidate;
            }
        }

        return new GeneRule(
            Enabled: true,
            ConditionKind: GeneConditionKind.TimeGreaterOrEqual,
            ConditionThreshold: GetConditionThresholdSpec(GeneConditionKind.TimeGreaterOrEqual, generationDurationSeconds).SampleInitial(random),
            ActionKind: GeneActionKind.AddBrake,
            Scalar: GetActionScalarSpec(GeneActionKind.AddBrake).SampleInitial(random),
            TargetPointNormalized: new Vector2(0.5f, 0.5f))
        {
            ConditionPointNormalized = new Vector2(0.5f, 0.5f),
            ConditionOrientationNormalized = 0.0f
        };
    }

    public static GeneRule MutateRule(
        Random random,
        GeneRule source,
        GenePoolDefinition genePool,
        MutationPolicy mutationPolicy,
        float generationDurationSeconds,
        float mutationStrength,
        RuleMutationProfile ruleMutationProfile)
    {
        var enabled = source.Enabled;

        if (enabled)
        {
            if (random.NextDouble() < mutationPolicy.RemoveRuleChance)
            {
                enabled = false;
            }
        }
        else if (random.NextDouble() < mutationPolicy.AddRuleChance)
        {
            return CreateRandomRule(random, genePool, generationDurationSeconds, disableChance: 0.0d, ruleMutationProfile);
        }

        if (!enabled)
        {
            return GeneRule.Disabled;
        }

        var conditionKind = source.ConditionKind;
        var conditionPopularity = ruleMutationProfile.GetConditionPopularity(source.ConditionKind, 1.0f);
        var conditionKindMutationChance = ruleMutationProfile.GetConditionMutationRate(
            source.ConditionKind,
            mutationPolicy.ConditionKindMutationChance);
        conditionKindMutationChance = Clamp01(conditionKindMutationChance + ((1.0f - conditionPopularity) * 0.20f));
        if (random.NextDouble() < conditionKindMutationChance)
        {
            conditionKind = PickConditionKind(random, genePool, ruleMutationProfile);
        }
        else if (!ConditionAllowed(source.ConditionKind, genePool))
        {
            conditionKind = PickConditionKind(random, genePool, ruleMutationProfile);
        }

        var actionKind = source.ActionKind;
        var actionPopularity = ruleMutationProfile.GetActionPopularity(source.ActionKind, 1.0f);
        var actionKindMutationChance = ruleMutationProfile.GetActionMutationRate(
            source.ActionKind,
            mutationPolicy.ActionKindMutationChance);
        actionKindMutationChance = Clamp01(actionKindMutationChance + ((1.0f - actionPopularity) * 0.20f));
        if (actionKind == GeneActionKind.None || random.NextDouble() < actionKindMutationChance)
        {
            actionKind = PickActionKind(random, genePool, ruleMutationProfile);
        }
        else if (!ActionAllowed(source.ActionKind, genePool))
        {
            actionKind = PickActionKind(random, genePool, ruleMutationProfile);
        }

        var threshold = source.ConditionThreshold;
        var conditionMutationStrength = mutationStrength * ruleMutationProfile.GetConditionMutationAmplitude(source.ConditionKind, 1.0f);
        if (conditionKind != GeneConditionKind.Always)
        {
            var conditionParameterMutationChance = ruleMutationProfile.GetConditionMutationRate(
                source.ConditionKind,
                mutationPolicy.ConditionParameterMutationChance);
            if (source.ConditionKind != conditionKind || random.NextDouble() < conditionParameterMutationChance)
            {
                threshold = GetConditionThresholdSpec(conditionKind, generationDurationSeconds).Mutate(
                    random,
                    GetConditionDefaultThreshold(conditionKind, source.ConditionThreshold, generationDurationSeconds),
                    conditionMutationStrength);
            }
            else
            {
                var thresholdSpec = GetConditionThresholdSpec(conditionKind, generationDurationSeconds);
                threshold = Clamp(source.ConditionThreshold, thresholdSpec.MinValue, thresholdSpec.MaxValue);
            }
        }
        else
        {
            threshold = 0.0f;
        }

        var conditionPoint = source.ConditionPointNormalized;
        var conditionOrientation = source.ConditionOrientationNormalized;
        if (conditionKind == GeneConditionKind.DistanceToPointLessOrEqual &&
            (source.ConditionKind != conditionKind || random.NextDouble() < ruleMutationProfile.GetConditionMutationRate(
                source.ConditionKind,
                mutationPolicy.ConditionParameterMutationChance)))
        {
            conditionPoint = new Vector2(
                GetTargetCoordinateSpec().Mutate(random, source.ConditionPointNormalized.X, conditionMutationStrength),
                GetTargetCoordinateSpec().Mutate(random, source.ConditionPointNormalized.Y, conditionMutationStrength));
        }
        else if (conditionKind == GeneConditionKind.DistanceToPointLessOrEqual)
        {
            conditionPoint = new Vector2(
                Clamp(source.ConditionPointNormalized.X, 0.0f, 1.0f),
                Clamp(source.ConditionPointNormalized.Y, 0.0f, 1.0f));
        }
        else
        {
            conditionPoint = new Vector2(0.5f, 0.5f);
        }

        if (conditionKind == GeneConditionKind.InsideHalfSpace &&
            (source.ConditionKind != conditionKind || random.NextDouble() < ruleMutationProfile.GetConditionMutationRate(
                source.ConditionKind,
                mutationPolicy.ConditionParameterMutationChance)))
        {
            conditionPoint = new Vector2(
                GetTargetCoordinateSpec().Mutate(random, source.ConditionPointNormalized.X, conditionMutationStrength),
                GetTargetCoordinateSpec().Mutate(random, source.ConditionPointNormalized.Y, conditionMutationStrength));
            conditionOrientation = GetConditionOrientationSpec(conditionKind).Mutate(
                random,
                source.ConditionKind == conditionKind ? source.ConditionOrientationNormalized : 0.0f,
                conditionMutationStrength);
        }
        else if (conditionKind == GeneConditionKind.InsideHalfSpace)
        {
            conditionPoint = new Vector2(
                Clamp(source.ConditionPointNormalized.X, 0.0f, 1.0f),
                Clamp(source.ConditionPointNormalized.Y, 0.0f, 1.0f));
            conditionOrientation = Clamp(source.ConditionOrientationNormalized, 0.0f, 1.0f);
        }
        else
        {
            conditionOrientation = 0.0f;
        }

        var sourceScalar = source.ActionKind == actionKind
            ? source.Scalar
            : GetActionScalarDefault(actionKind);

        var scalar = sourceScalar;
        var actionMutationStrength = mutationStrength * ruleMutationProfile.GetActionMutationAmplitude(source.ActionKind, 1.0f);
        var actionParameterMutationChance = ruleMutationProfile.GetActionMutationRate(
            source.ActionKind,
            mutationPolicy.ActionParameterMutationChance);
        if (source.ActionKind != actionKind || random.NextDouble() < actionParameterMutationChance)
        {
            scalar = GetActionScalarSpec(actionKind).Mutate(
                random,
                sourceScalar,
                actionMutationStrength);
        }
        else
        {
            scalar = Clamp(sourceScalar, GetActionScalarSpec(actionKind).MinValue, GetActionScalarSpec(actionKind).MaxValue);
        }

        var targetX = source.TargetPointNormalized.X;
        var targetY = source.TargetPointNormalized.Y;
        if (source.ActionKind != actionKind || random.NextDouble() < actionParameterMutationChance)
        {
            targetX = GetTargetCoordinateSpec().Mutate(random, source.TargetPointNormalized.X, actionMutationStrength);
            targetY = GetTargetCoordinateSpec().Mutate(random, source.TargetPointNormalized.Y, actionMutationStrength);
        }
        else
        {
            targetX = Clamp(source.TargetPointNormalized.X, 0.0f, 1.0f);
            targetY = Clamp(source.TargetPointNormalized.Y, 0.0f, 1.0f);
        }

        var direction = source.DirectionAngleNormalized;
        if (actionKind == GeneActionKind.AccelerateDirection)
        {
            if (source.ActionKind != actionKind) direction = (float)random.NextDouble();
            else if (random.NextDouble() < actionParameterMutationChance)
                direction = MutateDirection(direction, (float)random.NextDouble(), actionMutationStrength);
        }
        var result = new GeneRule(
            Enabled: true,
            ConditionKind: conditionKind,
            ConditionThreshold: threshold,
            ActionKind: actionKind,
            Scalar: scalar,
            TargetPointNormalized: new Vector2(targetX, targetY))
        {
            ConditionPointNormalized = conditionPoint,
            ConditionOrientationNormalized = conditionOrientation,
            DirectionAngleNormalized = direction
        };

        if (IsForbiddenRule(result))
        {
            result = result with
            {
                ConditionKind = GeneConditionKind.TimeGreaterOrEqual,
                ConditionThreshold = GetConditionThresholdSpec(GeneConditionKind.TimeGreaterOrEqual, generationDurationSeconds).SampleInitial(random)
            };
        }

        return result;
    }

    private static bool IsForbiddenRule(GeneRule rule)
    {
        return rule.Enabled &&
               rule.ConditionKind == GeneConditionKind.Always &&
               rule.ActionKind == GeneActionKind.AddBrake;
    }

    private static GeneConditionKind PickConditionKind(Random random, GenePoolDefinition genePool, RuleMutationProfile ruleMutationProfile)
    {
        Span<GeneConditionKind> available = stackalloc GeneConditionKind[5];
        Span<float> weights = stackalloc float[5];
        var count = 0;

        available[count] = GeneConditionKind.Always;
        weights[count++] = 1.0f;

        if (genePool.AllowTimeCondition)
        {
            available[count] = GeneConditionKind.TimeGreaterOrEqual;
            weights[count++] = GetWeightedPopularity(ruleMutationProfile.GetConditionPopularity(GeneConditionKind.TimeGreaterOrEqual, 0.55f));
        }

        if (genePool.AllowDistanceToPointCondition)
        {
            available[count] = GeneConditionKind.DistanceToPointLessOrEqual;
            weights[count++] = GetWeightedPopularity(ruleMutationProfile.GetConditionPopularity(GeneConditionKind.DistanceToPointLessOrEqual, 0.55f));
        }

        if (genePool.AllowNeighborCountCondition)
        {
            available[count] = GeneConditionKind.NeighborCountGreaterOrEqual;
            weights[count++] = GetWeightedPopularity(ruleMutationProfile.GetConditionPopularity(GeneConditionKind.NeighborCountGreaterOrEqual, 0.55f));
        }

        if (genePool.AllowHalfSpaceCondition)
        {
            available[count] = GeneConditionKind.InsideHalfSpace;
            weights[count++] = GetWeightedPopularity(ruleMutationProfile.GetConditionPopularity(GeneConditionKind.InsideHalfSpace, 0.55f));
        }

        return PickWeighted(random, available, weights, count);
    }

    private static GeneActionKind PickActionKind(Random random, GenePoolDefinition genePool, RuleMutationProfile ruleMutationProfile)
    {
        Span<GeneActionKind> available = stackalloc GeneActionKind[6];
        Span<float> weights = stackalloc float[6];
        var count = 0;

        if (genePool.AllowTurnBiasAction)
        {
            available[count] = GeneActionKind.AddTurnBias;
            weights[count++] = GetWeightedPopularity(ruleMutationProfile.GetActionPopularity(GeneActionKind.AddTurnBias, 0.55f));
        }

        if (genePool.AllowThrottleBiasAction)
        {
            available[count] = GeneActionKind.AddThrottleBias;
            weights[count++] = GetWeightedPopularity(ruleMutationProfile.GetActionPopularity(GeneActionKind.AddThrottleBias, 0.55f));
        }

        if (genePool.AllowDirectionAction)
        {
            available[count] = GeneActionKind.AccelerateDirection;
            weights[count++] = GetWeightedPopularity(ruleMutationProfile.GetActionPopularity(GeneActionKind.AccelerateDirection, .55f));
        }
        if (genePool.AllowSteerToTargetAction)
        {
            available[count] = GeneActionKind.SteerToTarget;
            weights[count++] = GetWeightedPopularity(ruleMutationProfile.GetActionPopularity(GeneActionKind.SteerToTarget, 0.55f));
        }

        if (genePool.AllowOrbitTargetAction)
        {
            available[count] = GeneActionKind.OrbitTarget;
            weights[count++] = GetWeightedPopularity(ruleMutationProfile.GetActionPopularity(GeneActionKind.OrbitTarget, 0.55f));
        }

        if (genePool.AllowBrakeAction)
        {
            available[count] = GeneActionKind.AddBrake;
            weights[count++] = GetWeightedPopularity(ruleMutationProfile.GetActionPopularity(GeneActionKind.AddBrake, 0.55f));
        }

        return count == 0 ? GeneActionKind.None : PickWeighted(random, available, weights, count);
    }

    private static float GetWeightedPopularity(float normalizedPopularity)
    {
        return 0.01f + (Clamp(normalizedPopularity, 0.0f, 1.0f) * Clamp(normalizedPopularity, 0.0f, 1.0f) * 3.0f);
    }

    private static T PickWeighted<T>(Random random, Span<T> values, Span<float> weights, int count)
    {
        var total = 0.0f;
        for (var index = 0; index < count; index++)
        {
            total += weights[index];
        }

        if (total <= 0.0001f)
        {
            return values[random.Next(count)];
        }

        var sample = (float)random.NextDouble() * total;
        var cumulative = 0.0f;
        for (var index = 0; index < count; index++)
        {
            cumulative += weights[index];
            if (sample <= cumulative)
            {
                return values[index];
            }
        }

        return values[count - 1];
    }

    private static bool ConditionAllowed(GeneConditionKind conditionKind, GenePoolDefinition genePool)
    {
        return conditionKind switch
        {
            GeneConditionKind.Always => true,
            GeneConditionKind.TimeGreaterOrEqual => genePool.AllowTimeCondition,
            GeneConditionKind.DistanceToPointLessOrEqual => genePool.AllowDistanceToPointCondition,
            GeneConditionKind.NeighborCountGreaterOrEqual => genePool.AllowNeighborCountCondition,
            GeneConditionKind.InsideHalfSpace => genePool.AllowHalfSpaceCondition,
            _ => false
        };
    }

    private static ParameterSpec GetConditionThresholdSpec(GeneConditionKind conditionKind, float generationDurationSeconds)
    {
        return conditionKind switch
        {
            GeneConditionKind.TimeGreaterOrEqual => new ParameterSpec(
                MinValue: 0.0f,
                MaxValue: MathF.Max(generationDurationSeconds, 0.01f),
                InitialDistribution: ParameterDistributionKind.Beta,
                MutationDistribution: ParameterDistributionKind.Beta,
                InitialAlpha: 2.0f,
                InitialBeta: 2.0f,
                MutationConcentrationMin: 3.5f,
                MutationConcentrationMax: 36.0f),
            GeneConditionKind.DistanceToPointLessOrEqual => new ParameterSpec(
                MinValue: 0.02f,
                MaxValue: 0.50f,
                InitialDistribution: ParameterDistributionKind.Beta,
                MutationDistribution: ParameterDistributionKind.Beta,
                InitialAlpha: 2.1f,
                InitialBeta: 2.6f,
                MutationConcentrationMin: 3.0f,
                MutationConcentrationMax: 28.0f),
            GeneConditionKind.NeighborCountGreaterOrEqual => new ParameterSpec(
                MinValue: 1.0f,
                MaxValue: 8.0f,
                InitialDistribution: ParameterDistributionKind.Beta,
                MutationDistribution: ParameterDistributionKind.Beta,
                InitialAlpha: 1.8f,
                InitialBeta: 2.2f,
                MutationConcentrationMin: 3.0f,
                MutationConcentrationMax: 24.0f),
            GeneConditionKind.InsideHalfSpace => new ParameterSpec(
                MinValue: 0.0f,
                MaxValue: 1.0f,
                InitialDistribution: ParameterDistributionKind.Beta,
                MutationDistribution: ParameterDistributionKind.Beta,
                InitialAlpha: 2.0f,
                InitialBeta: 2.0f,
                MutationConcentrationMin: 3.0f,
                MutationConcentrationMax: 28.0f),
            _ => new ParameterSpec(0.0f, 0.0f, ParameterDistributionKind.Uniform, ParameterDistributionKind.Uniform)
        };
    }

    private static bool ActionAllowed(GeneActionKind actionKind, GenePoolDefinition genePool)
    {
        return actionKind switch
        {
            GeneActionKind.AddTurnBias => genePool.AllowTurnBiasAction,
            GeneActionKind.AddThrottleBias => genePool.AllowThrottleBiasAction,
            GeneActionKind.SteerToTarget => genePool.AllowSteerToTargetAction,
            GeneActionKind.AccelerateDirection => genePool.AllowDirectionAction,
            GeneActionKind.OrbitTarget => genePool.AllowOrbitTargetAction,
            GeneActionKind.AddBrake => genePool.AllowBrakeAction,
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
            GeneActionKind.AccelerateDirection => new ParameterSpec(1, 1, ParameterDistributionKind.Uniform, ParameterDistributionKind.Uniform),
            GeneActionKind.SteerToTarget => new ParameterSpec(
                MinValue: 0.0f,
                MaxValue: 2.0f,
                InitialDistribution: ParameterDistributionKind.Beta,
                MutationDistribution: ParameterDistributionKind.Beta,
                InitialAlpha: 1.8f,
                InitialBeta: 2.2f,
                MutationConcentrationMin: 3.0f,
                MutationConcentrationMax: 26.0f),
            GeneActionKind.OrbitTarget => new ParameterSpec(
                MinValue: -2.0f,
                MaxValue: 2.0f,
                InitialDistribution: ParameterDistributionKind.Beta,
                MutationDistribution: ParameterDistributionKind.Beta,
                InitialAlpha: 2.0f,
                InitialBeta: 2.0f,
                MutationConcentrationMin: 3.0f,
                MutationConcentrationMax: 26.0f),
            GeneActionKind.AddBrake => new ParameterSpec(
                MinValue: 0.1f,
                MaxValue: 1.0f,
                InitialDistribution: ParameterDistributionKind.Beta,
                MutationDistribution: ParameterDistributionKind.Beta,
                InitialAlpha: 2.0f,
                InitialBeta: 1.8f,
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
            GeneActionKind.AccelerateDirection => 1.0f,
            GeneActionKind.OrbitTarget => 1.0f,
            GeneActionKind.AddBrake => 0.5f,
            _ => 0.0f
        };
    }

    private static float GetConditionDefaultThreshold(GeneConditionKind conditionKind, float sourceValue, float generationDurationSeconds)
    {
        if (sourceValue > 0.0f)
        {
            return sourceValue;
        }

        return conditionKind switch
        {
            GeneConditionKind.TimeGreaterOrEqual => generationDurationSeconds * 0.5f,
            GeneConditionKind.DistanceToPointLessOrEqual => 0.18f,
            GeneConditionKind.NeighborCountGreaterOrEqual => 2.0f,
            GeneConditionKind.InsideHalfSpace => 0.0f,
            _ => 0.0f
        };
    }

    private static ParameterSpec GetConditionOrientationSpec(GeneConditionKind conditionKind)
    {
        return conditionKind == GeneConditionKind.InsideHalfSpace
            ? new ParameterSpec(
                MinValue: 0.0f,
                MaxValue: 1.0f,
                InitialDistribution: ParameterDistributionKind.Beta,
                MutationDistribution: ParameterDistributionKind.Beta,
                InitialAlpha: 2.0f,
                InitialBeta: 2.0f,
                MutationConcentrationMin: 3.0f,
                MutationConcentrationMax: 28.0f)
            : new ParameterSpec(0.0f, 0.0f, ParameterDistributionKind.Uniform, ParameterDistributionKind.Uniform);
    }

    private static ParameterSpec GetTargetCoordinateSpec()
    {
        return new ParameterSpec(
            MinValue: 0.0f,
            MaxValue: 1.0f,
            InitialDistribution: ParameterDistributionKind.Uniform,
            MutationDistribution: ParameterDistributionKind.Beta,
            InitialAlpha: 2.0f,
            InitialBeta: 2.0f,
            MutationConcentrationMin: 3.0f,
            MutationConcentrationMax: 30.0f);
    }

    private static float Clamp(float value, float minValue, float maxValue)
    {
        return Math.Clamp(value, minValue, maxValue);
    }

    private static float Clamp01(float value)
    {
        return Clamp(value, 0.0f, 1.0f);
    }
}
