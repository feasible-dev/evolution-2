using EvoPrototype.Core;
using Godot;

public static class TestFixtures
{
    public static Agent CreateTestAgent(Vector2 position)
    {
        return new Agent
        {
            Genome = AgentGenome.Default,
            PhysicsProfile = PhysicsProfile.Default,
            Position = position,
            Velocity = Vector2.Zero,
            Heading = 0.0f,
            Radius = 4.0f,
            Energy = 100.0f,
            DecisionTimer = 999.0f,
            CurrentIntent = AgentIntent.Neutral
        };
    }

    public static Agent CreateAnalyticsAgent(Vector2 position, float mass, float baseThrottle)
    {
        return new Agent
        {
            Genome = AgentGenome.Default with { BaseThrottle = baseThrottle },
            PhysicsProfile = PhysicsProfile.Default with { Mass = mass },
            Position = position,
            Velocity = Vector2.Zero,
            Heading = 0.0f,
            Radius = 4.0f,
            Energy = 100.0f,
            DecisionTimer = 999.0f,
            CurrentIntent = AgentIntent.Neutral
        };
    }

    public static Vector2 SimulateSingleStepVelocity(Agent sourceAgent, float delta)
    {
        var simulation = new Simulation(new SimulationSettings
        {
            WorldSize = new Vector2(400.0f, 200.0f),
            AgentCount = 0,
            AgentRadius = 4.0f,
            MaxSpeed = 500.0f
        }, seed: 1);

        simulation.WorldState.Agents.Add(sourceAgent);
        sourceAgent.DecisionTimer = 999.0f;
        simulation.Update(delta);
        return sourceAgent.Velocity;
    }

    public static void SimulateSingleStep(Agent sourceAgent, float delta)
    {
        var simulation = new Simulation(new SimulationSettings
        {
            WorldSize = new Vector2(400.0f, 200.0f),
            AgentCount = 0,
            AgentRadius = 4.0f,
            MaxSpeed = 500.0f
        }, seed: 1);

        simulation.WorldState.Agents.Add(sourceAgent);
        sourceAgent.DecisionTimer = 999.0f;
        simulation.Update(delta);
    }
}
