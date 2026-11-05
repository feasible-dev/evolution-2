using Godot;
using System.Collections.Generic;

namespace EvoPrototype.Core;

public static class AgentBrain
{
    private const float BrownianWorldAccelerationFraction = 0.55f;
    private const float BrownianLocalSidePushFraction = 0.42f;
    private const float BrownianTurnBiasStrength = 0.45f;
    private const float BrownianAmplitudeShape = 0.35f;
    private const float MinimumBaselineExplorationWeight = 0.22f;

    /// <summary>
    /// Converts the baseline stochastic impulse plus rule-driven contributions into a high-level intent.
    /// The physics layer remains responsible for inertia, drag, collisions, walls, and energy budgets.
    /// </summary>
    public static AgentIntent Decide(Agent agent, AgentObservation observation, SimulationSettings settings)
    {
        var contributions = DecideContributions(agent, observation, settings);
        return ActionSetResolver.Resolve(contributions);
    }

    public static AgentIntent DecideRuleIntent(Agent agent, AgentObservation observation, SimulationSettings settings)
    {
        var enabledRuleCount = GeneProgramInspector.GetEnabledRules(agent.Genome.Program).Count;
        if (enabledRuleCount == 0)
        {
            return AgentIntent.Neutral;
        }

        var contributions = new List<ActionContribution>(enabledRuleCount);
        GeneRuleEvaluator.AppendContributions(contributions, agent, observation, settings, 1.0f / enabledRuleCount);
        var intent = ActionSetResolver.Resolve(contributions);
        if (settings.FixedWorldRuleAcceleration is { } strength && intent.WorldAccelerationWish.LengthSquared() > .000001f)
            intent = intent with { WorldAccelerationWish = intent.WorldAccelerationWish.Normalized() * strength };
        return intent;
    }

    public static AgentIntent DecideBaselineBrownianIntent(Agent agent, SimulationSettings settings)
    {
        var baselineWeight = GetBaselineExplorationWeight(agent, settings);
        if (baselineWeight <= 0.0001f)
        {
            return AgentIntent.Neutral;
        }

        var contributions = new List<ActionContribution>(3);
        AppendBaselineRandomContributions(contributions, agent, settings, baselineWeight);
        return ActionSetResolver.Resolve(contributions);
    }

    /// <summary>
    /// Produces a small baseline random jerk, then layers gene-rule contributions on top.
    /// </summary>
    public static List<ActionContribution> DecideContributions(Agent agent, AgentObservation observation, SimulationSettings settings)
    {
        var enabledRuleCount = GeneProgramInspector.GetEnabledRules(agent.Genome.Program).Count;
        var contributions = new List<ActionContribution>(Mathf.Max(enabledRuleCount + 1, 1));

        if (enabledRuleCount == 0)
        {
            AppendBaselineRandomContributions(contributions, agent, settings);
        }
        else
        {
            var ruleWeight = 1.0f / enabledRuleCount;
            var baselineWeight = GetBaselineExplorationWeight(agent, settings);
            if (baselineWeight > 0.0001f)
            {
                AppendBaselineRandomContributions(contributions, agent, settings, baselineWeight);
            }

            GeneRuleEvaluator.AppendContributions(contributions, agent, observation, settings, ruleWeight);
        }

        return contributions;
    }

    private static float GetBaselineExplorationWeight(Agent agent, SimulationSettings settings)
    {
        if (settings.FixedExplorationWeight is { } fixedWeight) return fixedWeight;
        var enabledRuleCount = GeneProgramInspector.GetEnabledRules(agent.Genome.Program).Count;
        if (enabledRuleCount == 0)
        {
            return 1.0f;
        }

        var lineageFade = settings.BaseRandomJerkFadeGenerations <= 0
            ? 0.0f
            : Mathf.Clamp(1.0f - (agent.LineageAge / (float)settings.BaseRandomJerkFadeGenerations), 0.0f, 1.0f);
        var baselineBlend = Mathf.Lerp(MinimumBaselineExplorationWeight, 1.0f, lineageFade);
        return settings.BaseRandomJerkRuleWeight * (1.0f / enabledRuleCount) * baselineBlend;
    }

    private static void AppendBaselineRandomContributions(
        List<ActionContribution> contributions,
        Agent agent,
        SimulationSettings settings,
        float weight = 1.0f)
    {
        var jerkDirection = ((agent.DecisionTurnNoise * 0.5f) + 0.5f) * Mathf.Tau;
        var jerkUnit = Vector2.FromAngle(jerkDirection);
        var signedAmplitude = Mathf.Clamp(agent.DecisionThrottleNoise, -1.0f, 1.0f);
        var brownianAmplitude = Mathf.Pow(Mathf.Abs(signedAmplitude), BrownianAmplitudeShape);
        var localSidePush = Mathf.Sign(signedAmplitude == 0.0f ? agent.DecisionTurnNoise : signedAmplitude) *
            brownianAmplitude *
            BrownianLocalSidePushFraction;
        var worldAccelerationWish = jerkUnit * (brownianAmplitude * settings.Acceleration * BrownianWorldAccelerationFraction);
        var turnWish = agent.DecisionTurnNoise * brownianAmplitude * BrownianTurnBiasStrength;
        contributions.Add(ActionContribution.LocalAcceleration(new Vector2(localSidePush, 0.0f), weight));
        contributions.Add(ActionContribution.Turn(turnWish, weight));
        contributions.Add(ActionContribution.WorldAcceleration(worldAccelerationWish, weight));
    }
}
