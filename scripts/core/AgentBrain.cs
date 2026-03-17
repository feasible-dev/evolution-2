using Godot;
using System.Collections.Generic;

namespace EvoPrototype.Core;

public static class AgentBrain
{
    /// <summary>
    /// Converts observations plus agent parameters into a high-level intent.
    /// The physics layer remains responsible for inertia, drag, walls, and energy budgets.
    /// </summary>
    public static AgentIntent Decide(Agent agent, AgentObservation observation, SimulationSettings settings)
    {
        var contributions = DecideContributions(agent, observation, settings);
        return ActionSetResolver.Resolve(contributions);
    }

    /// <summary>
    /// Produces rule-level action contributions so future channels can evolve independently from locomotion.
    /// </summary>
    public static List<ActionContribution> DecideContributions(Agent agent, AgentObservation observation, SimulationSettings settings)
    {
        var turnRate = agent.Genome.BaseTurnRate;
        turnRate += Mathf.Sin(agent.BrainClock * agent.Genome.WanderFrequency) * agent.Genome.WanderAmplitude;
        turnRate += agent.DecisionTurnNoise * agent.Genome.RandomTurnStrength;

        var targetWorld = new Vector2(
            agent.Genome.TargetPointNormalized.X * settings.WorldSize.X,
            agent.Genome.TargetPointNormalized.Y * settings.WorldSize.Y);
        var targetOffset = targetWorld - agent.Position;

        if (targetOffset.LengthSquared() > 0.001f)
        {
            var forward = Vector2.FromAngle(agent.Heading);
            var right = forward.Orthogonal();
            var normalizedTarget = targetOffset.Normalized();
            var targetLateralBias = Mathf.Clamp(normalizedTarget.Dot(right), -1.0f, 1.0f);
            turnRate += targetLateralBias * agent.Genome.TargetPointWeight;
        }

        if (observation.ForwardWallDistance < settings.WallAvoidanceDistance)
        {
            var wallBias = Mathf.Clamp(
                (observation.RightWallDistance - observation.LeftWallDistance) / settings.WallAvoidanceDistance,
                -1.0f,
                1.0f);

            turnRate += wallBias * agent.Genome.WallAvoidanceWeight;
        }

        if (observation.IsTouchingWall && observation.Speed < settings.StuckSpeedThreshold)
        {
            var unstuckDirection = observation.RightWallDistance >= observation.LeftWallDistance ? 1.0f : -1.0f;
            turnRate += unstuckDirection * agent.Genome.StuckTurnBias;
        }

        if (observation.HasNeighbor)
        {
            var distance = observation.NearestNeighborLocalOffset.Length();
            if (distance > 0.001f && distance < settings.NeighborAwarenessDistance)
            {
                var lateralBias = Mathf.Clamp(
                    observation.NearestNeighborLocalOffset.X / settings.NeighborAwarenessDistance,
                    -1.0f,
                    1.0f);

                turnRate -= lateralBias * agent.Genome.NeighborAvoidanceWeight;
            }
        }

        var targetSpeed = settings.MaxSpeed * agent.Genome.PreferredSpeedFraction;
        var speedError = targetSpeed - observation.Speed;
        var throttle = Mathf.Clamp(
            agent.Genome.BaseThrottle + agent.DecisionThrottleNoise * agent.Genome.RandomThrottleBias + (speedError / Mathf.Max(settings.MaxSpeed, 0.001f)),
            0.0f,
            1.0f);
        var brakeWish = 0.0f;

        if (speedError < 0.0f)
        {
            brakeWish = Mathf.Clamp(-speedError / Mathf.Max(targetSpeed + 1.0f, 1.0f), 0.0f, 1.0f);
        }

        if (observation.HasNeighbor && observation.NearestNeighborLocalOffset.Y > 0.0f)
        {
            var distance = observation.NearestNeighborLocalOffset.Length();
            if (distance < settings.NeighborBrakeDistance)
            {
                brakeWish = Mathf.Clamp(1.0f - (distance / settings.NeighborBrakeDistance), 0.0f, 1.0f);
            }
        }

        if (observation.IsTouchingWall && observation.Speed > settings.StuckSpeedThreshold)
        {
            brakeWish = Mathf.Max(brakeWish, 0.6f);
        }

        var localAccelerationWish = new Vector2(0.0f, throttle);
        var worldAccelerationWish = Vector2.Zero;

        if (targetOffset.LengthSquared() > 0.001f)
        {
            worldAccelerationWish = targetOffset.Normalized() * (agent.Genome.TargetPointWeight * settings.Acceleration * 0.18f);
        }

        var contributions = new List<ActionContribution>
        {
            ActionContribution.WorldAcceleration(worldAccelerationWish),
            ActionContribution.LocalAcceleration(localAccelerationWish),
            ActionContribution.Turn(turnRate),
            ActionContribution.Brake(brakeWish)
        };

        GeneRuleEvaluator.AppendContributions(contributions, agent, observation, settings);
        return contributions;
    }
}
