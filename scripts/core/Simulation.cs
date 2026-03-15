using System;
using Godot;

namespace EvoPrototype.Core;

public sealed class Simulation
{
    private readonly Random _random;

    public Simulation(SimulationSettings settings, int? seed = null)
    {
        Settings = settings;
        WorldState = new WorldState(settings.WorldSize);
        _random = seed.HasValue ? new Random(seed.Value) : new Random();
    }

    public SimulationSettings Settings { get; }

    public WorldState WorldState { get; }

    public void InitializeAgents()
    {
        WorldState.Agents.Clear();

        for (var index = 0; index < Settings.AgentCount; index++)
        {
            WorldState.Agents.Add(CreateRandomAgent());
        }

        ResolveOverlaps();
    }

    public void Update(float delta)
    {
        foreach (var agent in WorldState.Agents)
        {
            UpdateDecision(agent, delta);
            ApplyMovement(agent, delta);
        }

        ResolveOverlaps();
    }

    private Agent CreateRandomAgent()
    {
        var radius = Settings.AgentRadius;
        var position = new Vector2(
            NextFloat(radius, Settings.WorldSize.X - radius),
            NextFloat(radius, Settings.WorldSize.Y - radius));

        var heading = NextFloat(0.0f, Mathf.Tau);
        var genome = new AgentGenome(
            BaseThrottle: NextFloat(0.55f, 1.0f),
            BaseTurnRate: NextFloat(-1.6f, 1.6f),
            WallAvoidanceWeight: NextFloat(0.4f, 1.8f),
            NeighborAvoidanceWeight: NextFloat(0.2f, 1.2f),
            DecisionIntervalSeconds: NextFloat(0.08f, 0.3f));

        return new Agent
        {
            Genome = genome,
            Position = position,
            Velocity = HeadingToVector(heading) * (NextFloat(0.2f, 0.8f) * Settings.MaxSpeed),
            Heading = heading,
            Radius = radius,
            Energy = 100.0f,
            DecisionTimer = 0.0f,
            Color = Color.FromHsv(NextFloat(0.0f, 1.0f), 0.65f, 0.95f)
        };
    }

    private void UpdateDecision(Agent agent, float delta)
    {
        agent.DecisionTimer -= delta;
        if (agent.DecisionTimer > 0.0f)
        {
            return;
        }

        var observation = BuildObservation(agent);
        agent.CurrentDecision = AgentBrain.Decide(agent, observation, Settings);
        agent.DecisionTimer = Mathf.Max(0.01f, agent.Genome.DecisionIntervalSeconds);
    }

    private AgentObservation BuildObservation(Agent agent)
    {
        var forward = HeadingToVector(agent.Heading);
        var right = forward.Orthogonal();

        var nearestNeighborDistance = float.MaxValue;
        var nearestNeighborLocalOffset = Vector2.Zero;

        foreach (var otherAgent in WorldState.Agents)
        {
            if (ReferenceEquals(agent, otherAgent))
            {
                continue;
            }

            var offset = otherAgent.Position - agent.Position;
            var distance = offset.Length();
            if (distance < nearestNeighborDistance)
            {
                nearestNeighborDistance = distance;
                nearestNeighborLocalOffset = new Vector2(offset.Dot(right), offset.Dot(forward));
            }
        }

        return new AgentObservation(
            ForwardWallDistance: DistanceToBoundary(agent.Position, forward),
            LeftWallDistance: DistanceToBoundary(agent.Position, -right),
            RightWallDistance: DistanceToBoundary(agent.Position, right),
            NearestNeighborLocalOffset: nearestNeighborLocalOffset,
            HasNeighbor: nearestNeighborDistance < float.MaxValue);
    }

    private void ApplyMovement(Agent agent, float delta)
    {
        agent.Heading = Mathf.Wrap(agent.Heading + (agent.CurrentDecision.TurnRate * delta), 0.0f, Mathf.Tau);

        var forward = HeadingToVector(agent.Heading);
        agent.Velocity += forward * (CurrentThrottleAcceleration(agent) * delta);
        agent.Velocity = agent.Velocity.MoveToward(Vector2.Zero, Settings.Drag * Settings.MaxSpeed * delta);
        agent.Velocity = agent.Velocity.LimitLength(Settings.MaxSpeed);

        var nextPosition = agent.Position + (agent.Velocity * delta);
        agent.Position = ClampToWorld(nextPosition, agent.Radius);
        ClampVelocityAgainstWalls(agent);
    }

    private float CurrentThrottleAcceleration(Agent agent)
    {
        return Mathf.Clamp(agent.CurrentDecision.Throttle, 0.0f, 1.0f) * Settings.Acceleration;
    }

    private Vector2 ClampToWorld(Vector2 position, float radius)
    {
        return new Vector2(
            Mathf.Clamp(position.X, radius, Settings.WorldSize.X - radius),
            Mathf.Clamp(position.Y, radius, Settings.WorldSize.Y - radius));
    }

    private void ClampVelocityAgainstWalls(Agent agent)
    {
        var leftLimit = agent.Radius;
        var rightLimit = Settings.WorldSize.X - agent.Radius;
        var topLimit = agent.Radius;
        var bottomLimit = Settings.WorldSize.Y - agent.Radius;

        if (agent.Position.X <= leftLimit && agent.Velocity.X < 0.0f)
        {
            agent.Velocity = new Vector2(0.0f, agent.Velocity.Y);
        }

        if (agent.Position.X >= rightLimit && agent.Velocity.X > 0.0f)
        {
            agent.Velocity = new Vector2(0.0f, agent.Velocity.Y);
        }

        if (agent.Position.Y <= topLimit && agent.Velocity.Y < 0.0f)
        {
            agent.Velocity = new Vector2(agent.Velocity.X, 0.0f);
        }

        if (agent.Position.Y >= bottomLimit && agent.Velocity.Y > 0.0f)
        {
            agent.Velocity = new Vector2(agent.Velocity.X, 0.0f);
        }
    }

    private void ResolveOverlaps()
    {
        for (var firstIndex = 0; firstIndex < WorldState.Agents.Count; firstIndex++)
        {
            var firstAgent = WorldState.Agents[firstIndex];

            for (var secondIndex = firstIndex + 1; secondIndex < WorldState.Agents.Count; secondIndex++)
            {
                var secondAgent = WorldState.Agents[secondIndex];
                var offset = secondAgent.Position - firstAgent.Position;
                var distance = offset.Length();
                var minimumDistance = firstAgent.Radius + secondAgent.Radius;

                if (distance >= minimumDistance)
                {
                    continue;
                }

                Vector2 normal;
                if (distance <= 0.001f)
                {
                    normal = new Vector2(1.0f, 0.0f);
                    distance = 0.0f;
                }
                else
                {
                    normal = offset / distance;
                }

                var penetration = minimumDistance - distance;
                var correction = normal * (penetration * 0.5f);

                firstAgent.Position = ClampToWorld(firstAgent.Position - correction, firstAgent.Radius);
                secondAgent.Position = ClampToWorld(secondAgent.Position + correction, secondAgent.Radius);
            }
        }
    }

    private float DistanceToBoundary(Vector2 origin, Vector2 direction)
    {
        const float epsilon = 0.0001f;
        var normalizedDirection = direction.Normalized();
        var minDistance = float.MaxValue;

        if (Mathf.Abs(normalizedDirection.X) > epsilon)
        {
            var distanceToVertical = normalizedDirection.X > 0.0f
                ? (Settings.WorldSize.X - origin.X) / normalizedDirection.X
                : (0.0f - origin.X) / normalizedDirection.X;

            if (distanceToVertical >= 0.0f)
            {
                minDistance = Mathf.Min(minDistance, distanceToVertical);
            }
        }

        if (Mathf.Abs(normalizedDirection.Y) > epsilon)
        {
            var distanceToHorizontal = normalizedDirection.Y > 0.0f
                ? (Settings.WorldSize.Y - origin.Y) / normalizedDirection.Y
                : (0.0f - origin.Y) / normalizedDirection.Y;

            if (distanceToHorizontal >= 0.0f)
            {
                minDistance = Mathf.Min(minDistance, distanceToHorizontal);
            }
        }

        return minDistance == float.MaxValue ? 0.0f : minDistance;
    }

    private static Vector2 HeadingToVector(float heading)
    {
        return Vector2.FromAngle(heading);
    }

    private float NextFloat(float minValue, float maxValue)
    {
        return minValue + ((float)_random.NextDouble() * (maxValue - minValue));
    }
}
