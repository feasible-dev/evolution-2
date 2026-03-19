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
        if (!rule.Enabled || !ConditionMatches(rule, agent, observation, settings))
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

            case GeneActionKind.AddBrake:
                contributions.Add(ActionContribution.Brake(Mathf.Clamp(rule.Scalar, 0.0f, 1.0f)));
                break;

            case GeneActionKind.AddRandomTurn:
                contributions.Add(ActionContribution.Turn(agent.DecisionTurnNoise * rule.Scalar));
                break;
        }
    }

    private static bool ConditionMatches(GeneRule rule, Agent agent, AgentObservation observation, SimulationSettings settings)
    {
        return rule.ConditionKind switch
        {
            GeneConditionKind.Always => true,
            GeneConditionKind.TimeGreaterOrEqual => agent.BrainClock >= rule.ConditionThreshold,
            GeneConditionKind.DistanceToPointLessOrEqual => DistanceConditionMatches(rule, agent, settings.WorldSize),
            GeneConditionKind.NeighborCountGreaterOrEqual => observation.NeighborCountWithinAwareness >= Mathf.RoundToInt(Mathf.Clamp(rule.ConditionThreshold, 1.0f, 12.0f)),
            GeneConditionKind.InsideHalfSpace => HalfSpaceConditionMatches(rule, agent, settings.WorldSize),
            _ => false
        };
    }

    private static bool DistanceConditionMatches(GeneRule rule, Agent agent, Vector2 worldSize)
    {
        var targetWorld = new Vector2(
            rule.ConditionPointNormalized.X * worldSize.X,
            rule.ConditionPointNormalized.Y * worldSize.Y);
        var maxDimension = Mathf.Max(Mathf.Min(worldSize.X, worldSize.Y), 1.0f);
        var distanceLimit = Mathf.Clamp(rule.ConditionThreshold, 0.02f, 0.5f) * maxDimension;
        return agent.Position.DistanceTo(targetWorld) <= distanceLimit;
    }

    private static bool HalfSpaceConditionMatches(GeneRule rule, Agent agent, Vector2 worldSize)
    {
        var pointOnLine = new Vector2(
            rule.ConditionPointNormalized.X * worldSize.X,
            rule.ConditionPointNormalized.Y * worldSize.Y);
        var angle = rule.ConditionOrientationNormalized * Mathf.Tau;
        var normal = Vector2.FromAngle(angle);
        return (agent.Position - pointOnLine).Dot(normal) >= 0.0f;
    }
}
