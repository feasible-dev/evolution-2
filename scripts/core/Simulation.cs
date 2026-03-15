using Godot;

namespace EvoPrototype.Core;

public sealed class Simulation
{
    private readonly RandomNumberGenerator _random = new();

    public Simulation(Vector2 worldSize)
    {
        WorldState = new WorldState(worldSize);
    }

    public WorldState WorldState { get; }

    public void InitializeAgents(int agentCount)
    {
        WorldState.Agents.Clear();

        for (var index = 0; index < agentCount; index++)
        {
            var position = new Vector2(
                _random.RandfRange(0.0f, WorldState.Size.X),
                _random.RandfRange(0.0f, WorldState.Size.Y));

            var velocity = Vector2.FromAngle(_random.RandfRange(0.0f, Mathf.Tau))
                * _random.RandfRange(40.0f, 110.0f);

            var color = Color.FromHsv(
                _random.RandfRange(0.0f, 1.0f),
                0.65f,
                0.95f);

            WorldState.Agents.Add(new Agent
            {
                Position = position,
                Velocity = velocity,
                Energy = 100.0f,
                Color = color
            });
        }
    }

    public void Update(float delta)
    {
        foreach (var agent in WorldState.Agents)
        {
            var nextPosition = agent.Position + (agent.Velocity * delta);

            if (nextPosition.X < 0.0f || nextPosition.X > WorldState.Size.X)
            {
                agent.Velocity = new Vector2(-agent.Velocity.X, agent.Velocity.Y);
                nextPosition.X = Mathf.Clamp(nextPosition.X, 0.0f, WorldState.Size.X);
            }

            if (nextPosition.Y < 0.0f || nextPosition.Y > WorldState.Size.Y)
            {
                agent.Velocity = new Vector2(agent.Velocity.X, -agent.Velocity.Y);
                nextPosition.Y = Mathf.Clamp(nextPosition.Y, 0.0f, WorldState.Size.Y);
            }

            agent.Position = nextPosition;
        }
    }
}
