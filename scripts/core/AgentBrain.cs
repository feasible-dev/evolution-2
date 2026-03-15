using Godot;

namespace EvoPrototype.Core;

public static class AgentBrain
{
    public static AgentDecision Decide(Agent agent, AgentObservation observation, SimulationSettings settings)
    {
        var turnRate = agent.Genome.BaseTurnRate;
        turnRate += Mathf.Sin(agent.BrainClock * agent.Genome.WanderFrequency) * agent.Genome.WanderAmplitude;

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

        var throttle = Mathf.Clamp(agent.Genome.BaseThrottle, 0.0f, 1.0f);
        return new AgentDecision(turnRate, throttle);
    }
}
