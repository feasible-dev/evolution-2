using System;
using System.Collections.Generic;
using System.Diagnostics;
using Godot;

namespace EvoPrototype.Core;

/// <summary>
/// Owns the simulation loop and keeps the engine-facing layer thin.
/// Each update runs three phases: update decisions, apply continuous physics, resolve overlaps.
/// </summary>
public sealed class Simulation
{
    private static readonly Dictionary<Vector2I, List<int>> EmptyObservationGrid = new();
    private const float BoundaryCollisionPadding = 0.5f;
    private const float AdaptiveCollisionSpeedThresholdFraction = 0.42f;
    private const int AdaptiveCollisionNeighborhoodThreshold = 3;
    private const float AxisImmobilityMovementEpsilonPixels = 0.35f;
    private static readonly Vector2I[] NeighborCellOffsets =
    {
        new(-1, -1),
        new(0, -1),
        new(1, -1),
        new(-1, 0),
        new(0, 0),
        new(1, 0),
        new(-1, 1),
        new(0, 1),
        new(1, 1)
    };

    private readonly Random _random;

    public Simulation(SimulationSettings settings, int? seed = null)
    {
        Settings = settings;
        WorldState = new WorldState(settings.WorldSize);
        _random = seed.HasValue ? new Random(seed.Value) : new Random();
    }

    public SimulationSettings Settings { get; private set; }

    public WorldState WorldState { get; }

    public SimulationFrameMetrics LastUpdateMetrics { get; private set; } = SimulationFrameMetrics.Empty;

    public void UpdateSettings(SimulationSettings settings)
    {
        Settings = settings;
    }

    /// <summary>
    /// Creates a fresh population using randomized genomes and physical profiles.
    /// </summary>
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
        var decisionUsec = 0UL;
        var movementUsec = 0UL;
        var overlapUsec = 0UL;
        var decisionAgentCount = 0;
        var observationCellSize = GetObservationCellSize();
        var observationGridBuildStart = GetTicksUsec();
        var observationGrid = WorldState.Agents.Exists(agent => RequiresNeighborhood(agent.Genome.Program))
            ? BuildSpatialGrid(observationCellSize) : EmptyObservationGrid;
        decisionUsec += GetTicksUsec() - observationGridBuildStart;

        foreach (var agent in WorldState.Agents)
        {
            var decisionStartUsec = GetTicksUsec();
            UpdateDecision(agent, delta, observationGrid, observationCellSize);
            decisionUsec += GetTicksUsec() - decisionStartUsec;
            decisionAgentCount += 1;

            var movementStartUsec = GetTicksUsec();
            ApplyMovement(agent, delta);
            movementUsec += GetTicksUsec() - movementStartUsec;
        }

        var overlapStartUsec = GetTicksUsec();
        var overlapMetrics = ResolveOverlaps();
        overlapUsec += GetTicksUsec() - overlapStartUsec;

        LastUpdateMetrics = new SimulationFrameMetrics(
            DecisionMs: decisionUsec / 1000.0f,
            MovementMs: movementUsec / 1000.0f,
            OverlapMs: overlapUsec / 1000.0f,
            DecisionAgents: decisionAgentCount,
            CheckedPairs: overlapMetrics.CheckedPairs,
            ResolvedPairs: overlapMetrics.ResolvedPairs,
            CollisionActiveAgents: overlapMetrics.ActiveAgents,
            CollisionSkippedAgents: overlapMetrics.SkippedAgents);
    }

    public void ResolvePopulationLayout()
    {
        ResolveOverlaps();
    }

    private static ulong GetTicksUsec()
    {
        return (ulong)((Stopwatch.GetTimestamp() * 1_000_000.0) / Stopwatch.Frequency);
    }

    private Agent CreateRandomAgent()
    {
        var radius = Settings.AgentRadius;
        var position = BoardGeometry.RandomPointInside(_random, Settings.WorldSize, Settings.BoardShape, Settings.Obstacles, GetBoundaryCollisionRadius(radius));

        var heading = NextFloat(0.0f, Mathf.Tau);
        var genome = new AgentGenome(
            DecisionIntervalSeconds: NextFloat(0.08f, 0.3f),
            DecisionIntervalJitterSeconds: NextFloat(0.03f, 0.12f),
            Program: GeneRuleFactory.CreateRandomProgram(_random, Settings.GenePool, Settings.GenerationDurationSeconds));
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
            AgentId = Guid.NewGuid().ToString("N"),
            ParentAgentId = null,
            HasParent = false,
            GenerationNumber = 0,
            ParentChildCount = 0,
            LineageAge = 0,
            Genome = genome,
            PhysicsProfile = physicsProfile,
            Position = position,
            SpawnOrigin = position,
            ParentSpawnOrigin = position,
            MaxDistanceFromBirth = 0.0f,
            Velocity = HeadingToVector(heading) * (NextFloat(0.2f, 0.8f) * Settings.MaxSpeed),
            Heading = heading,
            Radius = radius,
            Energy = physicsProfile.MaxEnergy,
            DecisionTimer = NextDecisionDelay(genome),
            TimeSinceLastDecision = 0.0f,
            TimeBelowSpeedThreshold = 0.0f,
            ImmobileAxisXSeconds = 0.0f,
            ImmobileAxisYSeconds = 0.0f,
            IsTouchingWall = false,
            BrainClock = NextFloat(0.0f, Mathf.Tau),
            DecisionTurnNoise = NextFloat(-1.0f, 1.0f),
            DecisionThrottleNoise = NextFloat(-1.0f, 1.0f),
            Color = AgentVisualPalette.CreateRandomAccent(_random)
        };
    }

    private void UpdateDecision(Agent agent, float delta, Dictionary<Vector2I, List<int>> observationGrid, float observationCellSize)
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

        var observation = RequiresNeighborhood(agent.Genome.Program)
            ? BuildObservation(agent, observationGrid, observationCellSize)
            : new AgentObservation(0, 0, 0, Vector2.Zero, false, 0, agent.Velocity.Length(), agent.IsTouchingWall);
        agent.DecisionTurnNoise = NextFloat(-1.0f, 1.0f);
        agent.DecisionThrottleNoise = NextFloat(-1.0f, 1.0f);
        agent.CurrentIntent = Settings.ContinuousBaselineBrownianMotion
            ? AgentBrain.DecideRuleIntent(agent, observation, Settings)
            : AgentBrain.Decide(agent, observation, Settings);
        agent.DecisionTimer = NextDecisionDelay(agent.Genome);
        agent.TimeSinceLastDecision = 0.0f;
    }

    // All existing actions need only IsTouchingWall; only NeighborCount needs the spatial observation.
    // Genes already inherited remain authoritative even if their family was removed from the current pool.
    internal static bool RequiresNeighborhood(GeneProgram program)
    {
        static bool Needs(GeneRule rule) => rule.Enabled && rule.ConditionKind == GeneConditionKind.NeighborCountGreaterOrEqual;
        return Needs(program.PrimaryRule) || Needs(program.SecondaryRule) || Needs(program.TertiaryRule);
    }

    private AgentObservation BuildObservation(Agent agent, Dictionary<Vector2I, List<int>> observationGrid, float observationCellSize)
    {
        var forward = HeadingToVector(agent.Heading);
        var right = forward.Orthogonal();

        var nearestNeighborDistance = float.MaxValue;
        var nearestNeighborDistanceSquared = float.MaxValue;
        var nearestNeighborLocalOffset = Vector2.Zero;
        var neighborCountWithinAwareness = 0;
        var awarenessDistanceSquared = Settings.NeighborAwarenessDistance * Settings.NeighborAwarenessDistance;

        foreach (var otherIndex in EnumerateNearbyAgentIndices(agent.Position, observationGrid, observationCellSize))
        {
            var otherAgent = WorldState.Agents[otherIndex];
            if (ReferenceEquals(agent, otherAgent))
            {
                continue;
            }

            var offset = otherAgent.Position - agent.Position;
            var distanceSquared = offset.LengthSquared();
            if (distanceSquared < awarenessDistanceSquared)
            {
                neighborCountWithinAwareness += 1;
            }
            if (distanceSquared < nearestNeighborDistanceSquared)
            {
                nearestNeighborDistanceSquared = distanceSquared;
                nearestNeighborDistance = Mathf.Sqrt(distanceSquared);
                nearestNeighborLocalOffset = new Vector2(offset.Dot(right), offset.Dot(forward));
            }
        }

        return new AgentObservation(
            ForwardWallDistance: DistanceToBoundary(agent.Position, forward),
            LeftWallDistance: DistanceToBoundary(agent.Position, -right),
            RightWallDistance: DistanceToBoundary(agent.Position, right),
            NearestNeighborLocalOffset: nearestNeighborLocalOffset,
            HasNeighbor: nearestNeighborDistance < float.MaxValue,
            NeighborCountWithinAwareness: neighborCountWithinAwareness,
            Speed: agent.Velocity.Length(),
            IsTouchingWall: agent.IsTouchingWall);
    }

    private void ApplyMovement(Agent agent, float delta)
    {
        var activeIntent = GetActiveIntent(agent);

        // When an agent runs out of energy, inertia and drag still apply, but active control stops.
        if (agent.Energy <= 0.0f)
        {
            activeIntent = AgentIntent.Neutral;
        }

        var turnRate = Mathf.Clamp(
            activeIntent.TurnWish,
            -agent.PhysicsProfile.MaxTurnRate,
            agent.PhysicsProfile.MaxTurnRate);

        var energyLimitedTurnRate = ApplyTurnEnergyBudget(agent, turnRate, delta);
        agent.Heading = Mathf.Wrap(agent.Heading + (energyLimitedTurnRate * delta), 0.0f, Mathf.Tau);

        var worldAcceleration = ResolveWorldAcceleration(agent, activeIntent, delta);
        agent.Velocity += worldAcceleration * delta;

        var brakeAmount = Mathf.Clamp(activeIntent.BrakeWish, 0.0f, 1.0f);
        if (brakeAmount > 0.0f && agent.Velocity.LengthSquared() > 0.0001f)
        {
            var brakeAcceleration = ApplyBrakeEnergyBudget(agent, brakeAmount, delta);
            agent.Velocity = agent.Velocity.MoveToward(Vector2.Zero, brakeAcceleration * delta);
        }

        var dragFactor = Mathf.Clamp(1.0f - (agent.PhysicsProfile.LinearDrag * delta), 0.0f, 1.0f);
        agent.Velocity *= dragFactor;
        agent.Velocity = agent.Velocity.LimitLength(Settings.MaxSpeed);
        agent.Energy = Mathf.Clamp(agent.Energy - (agent.PhysicsProfile.IdleEnergyDrainPerSecond * delta), 0.0f, agent.PhysicsProfile.MaxEnergy);

        var previousPosition = agent.Position;
        var nextPosition = previousPosition + (agent.Velocity * delta);
        var clampedPosition = BoardGeometry.ClampPoint(nextPosition, Settings.WorldSize, Settings.BoardShape, Settings.Obstacles, GetBoundaryCollisionRadius(agent.Radius));
        var hitBoundary = clampedPosition.DistanceSquaredTo(nextPosition) > 0.0001f;
        agent.Position = clampedPosition;
        UpdateAxisImmobility(agent, previousPosition, clampedPosition, delta);
        agent.MaxDistanceFromBirth = Mathf.Max(agent.MaxDistanceFromBirth, agent.Position.DistanceTo(agent.SpawnOrigin));
        ClampVelocityAgainstWalls(agent, hitBoundary);
    }

    private AgentIntent GetActiveIntent(Agent agent)
    {
        if (!Settings.ContinuousBaselineBrownianMotion)
        {
            return agent.CurrentIntent;
        }

        agent.DecisionTurnNoise = NextFloat(-1.0f, 1.0f);
        agent.DecisionThrottleNoise = NextFloat(-1.0f, 1.0f);
        var brownianIntent = AgentBrain.DecideBaselineBrownianIntent(agent, Settings);
        return new AgentIntent(
            WorldAccelerationWish: agent.CurrentIntent.WorldAccelerationWish + brownianIntent.WorldAccelerationWish,
            LocalAccelerationWish: agent.CurrentIntent.LocalAccelerationWish + brownianIntent.LocalAccelerationWish,
            TurnWish: agent.CurrentIntent.TurnWish + brownianIntent.TurnWish,
            BrakeWish: agent.CurrentIntent.BrakeWish + brownianIntent.BrakeWish);
    }

    private Vector2 ResolveWorldAcceleration(Agent agent, AgentIntent activeIntent, float delta)
    {
        var forward = HeadingToVector(agent.Heading);
        var right = forward.Orthogonal();
        var clampedLocalWish = new Vector2(
            Mathf.Clamp(activeIntent.LocalAccelerationWish.X, -1.0f, 1.0f),
            Mathf.Clamp(activeIntent.LocalAccelerationWish.Y, -1.0f, 1.0f));

        var localAcceleration =
            (right * (clampedLocalWish.X * agent.PhysicsProfile.MaxLateralAcceleration)) +
            (forward * (clampedLocalWish.Y * agent.PhysicsProfile.MaxForwardAcceleration));

        var worldWish = activeIntent.WorldAccelerationWish;
        var maxWorldWish = agent.PhysicsProfile.MaxForwardAcceleration;
        if (worldWish.Length() > maxWorldWish)
        {
            worldWish = worldWish.Normalized() * maxWorldWish;
        }

        // Local wishes are relative to heading; world wishes stay in absolute coordinates.
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

    private void ClampVelocityAgainstWalls(Agent agent, bool hitBoundary)
    {
        if (hitBoundary &&
            BoardGeometry.TryGetObstacleBoundaryNormal(agent.Position, Settings.Obstacles, GetBoundaryCollisionRadius(agent.Radius), out var obstacleNormal))
        {
            var inwardSpeed = agent.Velocity.Dot(obstacleNormal);
            if (inwardSpeed < 0.0f)
            {
                agent.Velocity -= obstacleNormal * inwardSpeed;
            }
        }
        else if (hitBoundary)
        {
            var boundaryNormal = BoardGeometry.GetBoundaryNormal(agent.Position, Settings.WorldSize, Settings.BoardShape);
            var outwardSpeed = agent.Velocity.Dot(boundaryNormal);
            if (hitBoundary && outwardSpeed > 0.0f)
            {
                agent.Velocity -= boundaryNormal * outwardSpeed;
            }
        }

        agent.IsTouchingWall =
            hitBoundary ||
            !BoardGeometry.IsInsidePlayableArea(agent.Position, Settings.WorldSize, Settings.BoardShape, Settings.Obstacles, GetBoundaryCollisionRadius(agent.Radius));
    }

    private void UpdateAxisImmobility(Agent agent, Vector2 previousPosition, Vector2 currentPosition, float delta)
    {
        var deltaPosition = currentPosition - previousPosition;
        agent.ImmobileAxisXSeconds = Mathf.Abs(deltaPosition.X) <= AxisImmobilityMovementEpsilonPixels
            ? agent.ImmobileAxisXSeconds + delta
            : 0.0f;
        agent.ImmobileAxisYSeconds = Mathf.Abs(deltaPosition.Y) <= AxisImmobilityMovementEpsilonPixels
            ? agent.ImmobileAxisYSeconds + delta
            : 0.0f;
    }

    private OverlapResolutionMetrics ResolveOverlaps()
    {
        var collisionFactor = Mathf.Max(0.01f, Settings.AgentAgentCollisionRadiusFactor);
        var checkedPairs = 0;
        var resolvedPairs = 0;
        var skippedAgents = 0;
        var activeAgents = 0;

        for (var iteration = 0; iteration < Settings.OverlapSolverIterations; iteration++)
        {
            var resolvedBeforeIteration = resolvedPairs;
            var overlapCellSize = GetOverlapCellSize(collisionFactor);
            var overlapGrid = BuildSpatialGrid(overlapCellSize);

            for (var firstIndex = 0; firstIndex < WorldState.Agents.Count; firstIndex++)
            {
                var firstAgent = WorldState.Agents[firstIndex];
                if (Settings.UseAdaptiveAgentCollisions &&
                    ShouldSkipAdaptiveCollision(firstAgent, overlapGrid, overlapCellSize))
                {
                    skippedAgents += 1;
                    continue;
                }

                activeAgents += 1;

                foreach (var secondIndex in EnumeratePotentialOverlapIndices(firstIndex, firstAgent.Position, overlapGrid, overlapCellSize))
                {
                    checkedPairs += 1;
                    var secondAgent = WorldState.Agents[secondIndex];
                    var offset = secondAgent.Position - firstAgent.Position;
                    var minimumDistance = (firstAgent.Radius + secondAgent.Radius) * collisionFactor;
                    var minimumDistanceSquared = minimumDistance * minimumDistance;
                    var distanceSquared = offset.LengthSquared();

                    if (distanceSquared >= minimumDistanceSquared)
                    {
                        continue;
                    }

                    var distance = Mathf.Sqrt(Mathf.Max(distanceSquared, 0.0f));
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
                    resolvedPairs += 1;

                    firstAgent.Position = BoardGeometry.ClampPoint(firstAgent.Position - correction, Settings.WorldSize, Settings.BoardShape, Settings.Obstacles, GetBoundaryCollisionRadius(firstAgent.Radius));
                    secondAgent.Position = BoardGeometry.ClampPoint(secondAgent.Position + correction, Settings.WorldSize, Settings.BoardShape, Settings.Obstacles, GetBoundaryCollisionRadius(secondAgent.Radius));
                }
            }
            // No correction means later passes would see identical positions and repeat the same work.
            if (resolvedPairs == resolvedBeforeIteration) break;
        }

        return new OverlapResolutionMetrics(checkedPairs, resolvedPairs, activeAgents, skippedAgents);
    }

    private float DistanceToBoundary(Vector2 origin, Vector2 direction)
    {
        return BoardGeometry.DistanceToBoundary(origin, direction, Settings.WorldSize, Settings.BoardShape, Settings.Obstacles);
    }

    private Dictionary<Vector2I, List<int>> BuildSpatialGrid(float cellSize)
    {
        var safeCellSize = Mathf.Max(cellSize, 1.0f);
        var grid = new Dictionary<Vector2I, List<int>>(WorldState.Agents.Count);

        for (var agentIndex = 0; agentIndex < WorldState.Agents.Count; agentIndex++)
        {
            var agent = WorldState.Agents[agentIndex];
            var cell = GetGridCell(agent.Position, safeCellSize);
            if (!grid.TryGetValue(cell, out var bucket))
            {
                bucket = new List<int>();
                grid[cell] = bucket;
            }

            bucket.Add(agentIndex);
        }

        return grid;
    }

    private IEnumerable<int> EnumerateNearbyAgentIndices(Vector2 position, Dictionary<Vector2I, List<int>> grid, float cellSize)
    {
        var originCell = GetGridCell(position, cellSize);

        foreach (var offset in NeighborCellOffsets)
        {
            if (!grid.TryGetValue(originCell + offset, out var bucket))
            {
                continue;
            }

            foreach (var otherIndex in bucket)
            {
                yield return otherIndex;
            }
        }
    }

    private IEnumerable<int> EnumeratePotentialOverlapIndices(int firstIndex, Vector2 firstPosition, Dictionary<Vector2I, List<int>> grid, float cellSize)
    {
        var originCell = GetGridCell(firstPosition, cellSize);

        foreach (var offset in NeighborCellOffsets)
        {
            if (!grid.TryGetValue(originCell + offset, out var bucket))
            {
                continue;
            }

            foreach (var secondIndex in bucket)
            {
                if (secondIndex <= firstIndex)
                {
                    continue;
                }

                yield return secondIndex;
            }
        }
    }

    private float GetObservationCellSize()
    {
        return Mathf.Max(Mathf.Max(Settings.NeighborAwarenessDistance, Settings.AgentRadius * 2.0f), 1.0f);
    }

    private float GetOverlapCellSize(float collisionFactor)
    {
        // The grid must cover the largest actual contact diameter, not the visual diameter.
        var maxRadius = Settings.AgentRadius;
        foreach (var agent in WorldState.Agents) maxRadius = Mathf.Max(maxRadius, agent.Radius);
        return Mathf.Max(maxRadius * 2 * collisionFactor, 1.0f);
    }

    private bool ShouldSkipAdaptiveCollision(Agent agent, Dictionary<Vector2I, List<int>> grid, float cellSize)
    {
        var fastEnough = agent.Velocity.LengthSquared() >= Mathf.Pow(Settings.MaxSpeed * AdaptiveCollisionSpeedThresholdFraction, 2.0f);
        if (!fastEnough || agent.IsTouchingWall)
        {
            return false;
        }

        var localAgentCount = 0;
        var originCell = GetGridCell(agent.Position, cellSize);
        foreach (var offset in NeighborCellOffsets)
        {
            if (!grid.TryGetValue(originCell + offset, out var bucket))
            {
                continue;
            }

            localAgentCount += bucket.Count;
            if (localAgentCount >= AdaptiveCollisionNeighborhoodThreshold)
            {
                return false;
            }
        }

        return true;
    }

    private static Vector2I GetGridCell(Vector2 position, float cellSize)
    {
        var safeCellSize = Mathf.Max(cellSize, 1.0f);
        return new Vector2I(
            Mathf.FloorToInt(position.X / safeCellSize),
            Mathf.FloorToInt(position.Y / safeCellSize));
    }

    private static Vector2 HeadingToVector(float heading)
    {
        return Vector2.FromAngle(heading);
    }

    private static float GetBoundaryCollisionRadius(float agentRadius)
    {
        return agentRadius + BoundaryCollisionPadding;
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

    private readonly record struct OverlapResolutionMetrics(
        int CheckedPairs,
        int ResolvedPairs,
        int ActiveAgents,
        int SkippedAgents);
}
