using System.Collections.Generic;
using Godot;

namespace EvoPrototype.Core;

/// <summary>
/// Evaluates gene rules into action contributions on top of the baseline locomotion brain.
/// </summary>
public static class GeneRuleEvaluator
{
    public static void AppendContributions(
        List<ActionContribution> contributions,
        Agent agent,
        AgentObservation observation,
        SimulationSettings settings)
    {
        EvaluateRule(agent.Genome.Program.PrimaryRule, contributions, agent, observation, settings);
        EvaluateRule(agent.Genome.Program.SecondaryRule, contributions, agent, observation, settings);
    }

    private static void EvaluateRule(
        GeneRule rule,
        List<ActionContribution> contributions,
        Agent agent,
        AgentObservation observation,
        SimulationSettings settings)
    {
        if (!rule.Enabled || !ConditionMatches(rule, agent, observation))
        {
            return;
        }

        switch (rule.ActionKind)
        {
            case GeneActionKind.AddTurnBias:
                contributions.Add(ActionContribution.Turn(rule.Scalar));
                break;

            case GeneActionKind.AddThrottleBias:
                contributions.Add(ActionContribution.LocalAcceleration(new Vector2(0.0f, Mathf.Clamp(rule.Scalar, -1.0f, 1.0f))));
                break;

            case GeneActionKind.SteerToTarget:
                var targetWorld = new Vector2(
                    rule.TargetPointNormalized.X * settings.WorldSize.X,
                    rule.TargetPointNormalized.Y * settings.WorldSize.Y);
                var offset = targetWorld - agent.Position;
                if (offset.LengthSquared() > 0.001f)
                {
                    contributions.Add(ActionContribution.WorldAcceleration(
                        offset.Normalized() * (rule.Scalar * settings.Acceleration * 0.20f)));
                }
                break;
        }
    }

    private static bool ConditionMatches(GeneRule rule, Agent agent, AgentObservation observation)
    {
        return rule.ConditionKind switch
        {
            GeneConditionKind.Always => true,
            GeneConditionKind.TimeGreaterOrEqual => agent.BrainClock >= rule.ConditionThreshold,
            _ => false
        };
    }
}
