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
            DecisionIntervalSeconds: NextFloat(0.08f, 0.3f),
            DecisionIntervalJitterSeconds: NextFloat(0.03f, 0.12f),
            WanderAmplitude: NextFloat(0.3f, 0.9f),
            WanderFrequency: NextFloat(0.6f, 2.4f),
            StuckTurnBias: NextFloat(1.6f, 3.2f));
        var physicsProfile = PhysicsProfile.Default with
        {
            Mass = NextFloat(0.8f, 1.4f),
            MaxForwardAcceleration = NextFloat(180.0f, 260.0f),
            MaxLateralAcceleration = NextFloat(80.0f, 130.0f),
            MaxBrakeAcceleration = NextFloat(140.0f, 210.0f),
            MaxTurnRate = NextFloat(2.2f, 4.2f),
            LinearDrag = NextFloat(0.25f, 0.45f),
            MaxEnergy = NextFloat(70.0f, 130.0f),
            IdleEnergyDrainPerSecond = NextFloat(0.08f, 0.22f),
            AccelerationEnergyCost = NextFloat(0.03f, 0.06f),
            BrakeEnergyCost = NextFloat(0.015f, 0.03f),
            TurnEnergyCost = NextFloat(0.008f, 0.018f)
        };

        return new Agent
        {
            Genome = genome,
            PhysicsProfile = physicsProfile,
            Position = position,
            Velocity = HeadingToVector(heading) * (NextFloat(0.2f, 0.8f) * Settings.MaxSpeed),
            Heading = heading,
            Radius = radius,
            Energy = physicsProfile.MaxEnergy,
            DecisionTimer = NextDecisionDelay(genome),
            TimeSinceLastDecision = 0.0f,
            TimeBelowSpeedThreshold = 0.0f,
            IsTouchingWall = false,
            BrainClock = NextFloat(0.0f, Mathf.Tau),
            Color = Color.FromHsv(NextFloat(0.0f, 1.0f), 0.65f, 0.95f)
        };
    }

    private void UpdateDecision(Agent agent, float delta)
    {
        agent.BrainClock += delta;
        agent.DecisionTimer -= delta;
        agent.TimeSinceLastDecision += delta;

        if (agent.Velocity.Length() < Settings.StuckSpeedThreshold)
        {
            agent.TimeBelowSpeedThreshold += delta;
        }
        else
        {
            agent.TimeBelowSpeedThreshold = 0.0f;
        }

        if (!ShouldReevaluateDecision(agent))
        {
            return;
        }

        var observation = BuildObservation(agent);
        agent.CurrentIntent = AgentBrain.Decide(agent, observation, Settings);
        agent.DecisionTimer = NextDecisionDelay(agent.Genome);
        agent.TimeSinceLastDecision = 0.0f;
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
            HasNeighbor: nearestNeighborDistance < float.MaxValue,
            Speed: agent.Velocity.Length(),
            IsTouchingWall: agent.IsTouchingWall);
    }

    private void ApplyMovement(Agent agent, float delta)
    {
        if (agent.Energy <= 0.0f)
        {
            agent.CurrentIntent = agent.CurrentIntent with
            {
                WorldAccelerationWish = Vector2.Zero,
                LocalAccelerationWish = Vector2.Zero,
                TurnWish = 0.0f,
                BrakeWish = 0.0f
            };
        }

        var turnRate = Mathf.Clamp(
            agent.CurrentIntent.TurnWish,
            -agent.PhysicsProfile.MaxTurnRate,
            agent.PhysicsProfile.MaxTurnRate);

        var energyLimitedTurnRate = ApplyTurnEnergyBudget(agent, turnRate, delta);
        agent.Heading = Mathf.Wrap(agent.Heading + (energyLimitedTurnRate * delta), 0.0f, Mathf.Tau);

        var worldAcceleration = ResolveWorldAcceleration(agent, delta);
        agent.Velocity += worldAcceleration * delta;

        var brakeAmount = Mathf.Clamp(agent.CurrentIntent.BrakeWish, 0.0f, 1.0f);
        if (brakeAmount > 0.0f && agent.Velocity.LengthSquared() > 0.0001f)
        {
            var brakeAcceleration = ApplyBrakeEnergyBudget(agent, brakeAmount, delta);
            agent.Velocity = agent.Velocity.MoveToward(Vector2.Zero, brakeAcceleration * delta);
        }

        var dragFactor = Mathf.Clamp(1.0f - (agent.PhysicsProfile.LinearDrag * delta), 0.0f, 1.0f);
        agent.Velocity *= dragFactor;
        agent.Velocity = agent.Velocity.LimitLength(Settings.MaxSpeed);
        agent.Energy = Mathf.Clamp(agent.Energy - (agent.PhysicsProfile.IdleEnergyDrainPerSecond * delta), 0.0f, agent.PhysicsProfile.MaxEnergy);

        var nextPosition = agent.Position + (agent.Velocity * delta);
        agent.Position = ClampToWorld(nextPosition, agent.Radius);
        ClampVelocityAgainstWalls(agent);
    }

    private Vector2 ResolveWorldAcceleration(Agent agent, float delta)
    {
        var forward = HeadingToVector(agent.Heading);
        var right = forward.Orthogonal();
        var clampedLocalWish = new Vector2(
            Mathf.Clamp(agent.CurrentIntent.LocalAccelerationWish.X, -1.0f, 1.0f),
            Mathf.Clamp(agent.CurrentIntent.LocalAccelerationWish.Y, -1.0f, 1.0f));

        var localAcceleration =
            (right * (clampedLocalWish.X * agent.PhysicsProfile.MaxLateralAcceleration)) +
            (forward * (clampedLocalWish.Y * agent.PhysicsProfile.MaxForwardAcceleration));

        var worldWish = agent.CurrentIntent.WorldAccelerationWish;
        var maxWorldWish = agent.PhysicsProfile.MaxForwardAcceleration;
        if (worldWish.Length() > maxWorldWish)
        {
            worldWish = worldWish.Normalized() * maxWorldWish;
        }

        var requestedAcceleration = localAcceleration + worldWish;
        var energyLimitedAcceleration = ApplyAccelerationEnergyBudget(agent, requestedAcceleration, delta);
        return energyLimitedAcceleration / agent.PhysicsProfile.Mass;
    }

    private Vector2 ApplyAccelerationEnergyBudget(Agent agent, Vector2 requestedAcceleration, float delta)
    {
        if (requestedAcceleration.LengthSquared() <= 0.0001f || agent.Energy <= 0.0f)
        {
            return Vector2.Zero;
        }

        var requestedMagnitude = requestedAcceleration.Length();
        var energyNeeded = requestedMagnitude * agent.PhysicsProfile.AccelerationEnergyCost * delta;
        if (energyNeeded <= agent.Energy)
        {
            agent.Energy -= energyNeeded;
            return requestedAcceleration;
        }

        var scale = agent.Energy / Mathf.Max(energyNeeded, 0.0001f);
        agent.Energy = 0.0f;
        return requestedAcceleration * scale;
    }

    private float ApplyBrakeEnergyBudget(Agent agent, float brakeAmount, float delta)
    {
        if (brakeAmount <= 0.0f || agent.Energy <= 0.0f)
        {
            return 0.0f;
        }

        var requestedBrake = agent.PhysicsProfile.MaxBrakeAcceleration * brakeAmount;
        var energyNeeded = requestedBrake * agent.PhysicsProfile.BrakeEnergyCost * delta;
        if (energyNeeded <= agent.Energy)
        {
            agent.Energy -= energyNeeded;
            return requestedBrake;
        }

        var scale = agent.Energy / Mathf.Max(energyNeeded, 0.0001f);
        agent.Energy = 0.0f;
        return requestedBrake * scale;
    }

    private float ApplyTurnEnergyBudget(Agent agent, float turnRate, float delta)
    {
        if (Mathf.Abs(turnRate) <= 0.0001f || agent.Energy <= 0.0f)
        {
            return 0.0f;
        }

        var energyNeeded = Mathf.Abs(turnRate) * agent.PhysicsProfile.TurnEnergyCost * delta;
        if (energyNeeded <= agent.Energy)
        {
            agent.Energy -= energyNeeded;
            return turnRate;
        }

        var scale = agent.Energy / Mathf.Max(energyNeeded, 0.0001f);
        agent.Energy = 0.0f;
        return turnRate * scale;
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
        var isTouchingWall = false;

        if (agent.Position.X <= leftLimit && agent.Velocity.X < 0.0f)
        {
            agent.Velocity = new Vector2(0.0f, agent.Velocity.Y);
            isTouchingWall = true;
        }

        if (agent.Position.X >= rightLimit && agent.Velocity.X > 0.0f)
        {
            agent.Velocity = new Vector2(0.0f, agent.Velocity.Y);
            isTouchingWall = true;
        }

        if (agent.Position.Y <= topLimit && agent.Velocity.Y < 0.0f)
        {
            agent.Velocity = new Vector2(agent.Velocity.X, 0.0f);
            isTouchingWall = true;
        }

        if (agent.Position.Y >= bottomLimit && agent.Velocity.Y > 0.0f)
        {
            agent.Velocity = new Vector2(agent.Velocity.X, 0.0f);
            isTouchingWall = true;
        }

        if (!isTouchingWall)
        {
            isTouchingWall =
                agent.Position.X <= leftLimit ||
                agent.Position.X >= rightLimit ||
                agent.Position.Y <= topLimit ||
                agent.Position.Y >= bottomLimit;
        }

        agent.IsTouchingWall = isTouchingWall;
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

    private bool ShouldReevaluateDecision(Agent agent)
    {
        if (agent.DecisionTimer > 0.0f)
        {
            return false;
        }

        if (agent.TimeSinceLastDecision < Settings.MinimumDecisionIntervalSeconds)
        {
            return false;
        }

        if (agent.IsTouchingWall)
        {
            return true;
        }

        if (agent.TimeBelowSpeedThreshold >= Settings.LowSpeedReconsiderationSeconds)
        {
            return true;
        }

        return true;
    }

    private float NextDecisionDelay(AgentGenome genome)
    {
        var jitter = NextFloat(-genome.DecisionIntervalJitterSeconds, genome.DecisionIntervalJitterSeconds);
        return Mathf.Max(Settings.MinimumDecisionIntervalSeconds, genome.DecisionIntervalSeconds + jitter);
    }

    private float NextFloat(float minValue, float maxValue)
    {
        return minValue + ((float)_random.NextDouble() * (maxValue - minValue));
    }
}
