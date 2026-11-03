using System.Collections.Generic;
using Godot;

namespace EvoPrototype.Core;

/// <summary>
/// Evaluates gene rules into action contributions on top of the baseline stochastic movement.
/// </summary>
public static class GeneRuleEvaluator
{
    public static void AppendContributions(
        List<ActionContribution> contributions,
        Agent agent,
        AgentObservation observation,
        SimulationSettings settings,
        float weight = 1.0f)
    {
        EvaluateRule(agent.Genome.Program.PrimaryRule, contributions, agent, observation, settings, weight);
        EvaluateRule(agent.Genome.Program.SecondaryRule, contributions, agent, observation, settings, weight);
        EvaluateRule(agent.Genome.Program.TertiaryRule, contributions, agent, observation, settings, weight);
    }

    private static void EvaluateRule(
        GeneRule rule,
        List<ActionContribution> contributions,
        Agent agent,
        AgentObservation observation,
        SimulationSettings settings,
        float weight)
    {
        if (!rule.Enabled || !ConditionMatches(rule, agent, observation, settings))
        {
            return;
        }

        switch (rule.ActionKind)
        {
            case GeneActionKind.AddTurnBias:
                contributions.Add(ActionContribution.Turn(rule.Scalar, weight));
                break;

            case GeneActionKind.AddThrottleBias:
                contributions.Add(ActionContribution.LocalAcceleration(new Vector2(0.0f, Mathf.Clamp(rule.Scalar, -1.0f, 1.0f)), weight));
                break;

            case GeneActionKind.SteerToTarget:
                var targetWorld = new Vector2(
                    rule.TargetPointNormalized.X * settings.WorldSize.X,
                    rule.TargetPointNormalized.Y * settings.WorldSize.Y);
                var offset = targetWorld - agent.Position;
                if (offset.LengthSquared() > 0.001f)
                {
                    contributions.Add(ActionContribution.WorldAcceleration(
                        offset.Normalized() * (rule.Scalar * settings.Acceleration * 0.20f),
                        weight));
                }
                break;

            case GeneActionKind.OrbitTarget:
                var orbitCenter = new Vector2(
                    rule.TargetPointNormalized.X * settings.WorldSize.X,
                    rule.TargetPointNormalized.Y * settings.WorldSize.Y);
                var radialOffset = agent.Position - orbitCenter;
                if (radialOffset.LengthSquared() > 0.001f)
                {
                    var orbitStrength = Mathf.Abs(rule.Scalar);
                    var radialDirection = radialOffset.Normalized();
                    var directionSign = rule.Scalar >= 0.0f ? 1.0f : -1.0f;
                    var tangentialDirection = new Vector2(-radialDirection.Y, radialDirection.X) * directionSign;
                    var tangentialAcceleration = tangentialDirection * (orbitStrength * settings.Acceleration * 0.26f);
                    var radialCorrection = observation.IsTouchingWall
                        ? radialDirection * (settings.Acceleration * 0.04f)
                        : -radialDirection * (settings.Acceleration * 0.045f);
                    var desiredHeading = tangentialDirection.Angle();
                    var headingDelta = Mathf.Wrap(desiredHeading - agent.Heading, -Mathf.Pi, Mathf.Pi);
                    var turnWish = Mathf.Clamp(headingDelta / Mathf.Pi, -1.0f, 1.0f) * 1.2f;
                    contributions.Add(ActionContribution.WorldAcceleration(
                        tangentialAcceleration + radialCorrection,
                        weight));
                    contributions.Add(ActionContribution.LocalAcceleration(
                        new Vector2(0.0f, Mathf.Clamp(0.28f + (orbitStrength * 0.22f), 0.0f, 1.0f)),
                        weight));
                    contributions.Add(ActionContribution.Turn(turnWish, weight));
                }
                break;

            case GeneActionKind.AddBrake:
                contributions.Add(ActionContribution.Brake(Mathf.Clamp(rule.Scalar, 0.0f, 1.0f), weight));
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
