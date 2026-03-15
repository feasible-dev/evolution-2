using EvoPrototype.Core;
using Godot;

var failures = new List<string>();

Run("Wall clamp stops outward movement without bounce", TestWallClampStopsOutwardMovementWithoutBounce);
Run("Overlap resolution separates agents by radius", TestOverlapResolutionSeparatesAgentsByRadius);
Run("Brain turns toward open space near a wall", TestBrainTurnsTowardOpenSpaceNearWall);
Run("Brain adds unstuck turn near a wall at low speed", TestBrainAddsUnstuckTurnNearWallAtLowSpeed);
Run("Decision timers differ per agent because of jitter", TestDecisionTimersDifferPerAgentBecauseOfJitter);

if (failures.Count > 0)
{
    foreach (var failure in failures)
    {
        Console.Error.WriteLine(failure);
    }

    return 1;
}

Console.WriteLine("All simulation tests passed.");
return 0;

void Run(string testName, Action testAction)
{
    try
    {
        testAction();
        Console.WriteLine($"PASS: {testName}");
    }
    catch (Exception exception)
    {
        failures.Add($"FAIL: {testName} -> {exception.Message}");
    }
}

void TestWallClampStopsOutwardMovementWithoutBounce()
{
    var simulation = new Simulation(new SimulationSettings
    {
        WorldSize = new Vector2(100.0f, 100.0f),
        AgentCount = 0,
        AgentRadius = 5.0f,
        Acceleration = 0.0f,
        Drag = 0.0f,
        MaxSpeed = 200.0f
    });

    var agent = new Agent
    {
        Genome = AgentGenome.Default,
        Position = new Vector2(95.0f, 50.0f),
        Velocity = new Vector2(20.0f, 0.0f),
        Heading = 0.0f,
        Radius = 5.0f,
        DecisionTimer = 999.0f
    };

    simulation.WorldState.Agents.Add(agent);
    simulation.Update(1.0f);

    AssertNearlyEqual(95.0f, agent.Position.X, 0.01f, "Agent should stay clamped to the right wall.");
    AssertNearlyEqual(0.0f, agent.Velocity.X, 0.01f, "Agent should lose outward horizontal velocity instead of bouncing.");
}

void TestOverlapResolutionSeparatesAgentsByRadius()
{
    var simulation = new Simulation(new SimulationSettings
    {
        WorldSize = new Vector2(100.0f, 100.0f),
        AgentCount = 0,
        AgentRadius = 5.0f,
        Acceleration = 0.0f,
        Drag = 0.0f,
        MaxSpeed = 0.0f
    });

    var firstAgent = new Agent
    {
        Genome = AgentGenome.Default,
        Position = new Vector2(50.0f, 50.0f),
        Velocity = Vector2.Zero,
        Heading = 0.0f,
        Radius = 5.0f,
        DecisionTimer = 999.0f
    };

    var secondAgent = new Agent
    {
        Genome = AgentGenome.Default,
        Position = new Vector2(53.0f, 50.0f),
        Velocity = Vector2.Zero,
        Heading = 0.0f,
        Radius = 5.0f,
        DecisionTimer = 999.0f
    };

    simulation.WorldState.Agents.Add(firstAgent);
    simulation.WorldState.Agents.Add(secondAgent);
    simulation.Update(0.0f);

    var distance = firstAgent.Position.DistanceTo(secondAgent.Position);
    AssertTrue(distance >= 9.99f, "Agents should be separated to at least the sum of their radii.");
}

void TestBrainTurnsTowardOpenSpaceNearWall()
{
    var agent = new Agent
    {
        Genome = new AgentGenome(
            BaseThrottle: 0.8f,
            BaseTurnRate: 0.0f,
            WallAvoidanceWeight: 1.5f,
            NeighborAvoidanceWeight: 0.0f,
            DecisionIntervalSeconds: 0.2f,
            DecisionIntervalJitterSeconds: 0.0f,
            WanderAmplitude: 0.0f,
            WanderFrequency: 1.0f,
            StuckTurnBias: 2.0f),
        Position = Vector2.Zero,
        Velocity = Vector2.Zero,
        Heading = 0.0f,
        Radius = 4.0f,
        BrainClock = 0.0f
    };

    var observation = new AgentObservation(
        ForwardWallDistance: 12.0f,
        LeftWallDistance: 10.0f,
        RightWallDistance: 40.0f,
        NearestNeighborLocalOffset: Vector2.Zero,
        HasNeighbor: false,
        Speed: 12.0f,
        IsTouchingWall: false);

    var decision = AgentBrain.Decide(agent, observation, new SimulationSettings
    {
        WallAvoidanceDistance = 60.0f
    });

    AssertTrue(decision.TurnRate > 0.0f, "Agent should turn toward the side with more clearance.");
}

void TestBrainAddsUnstuckTurnNearWallAtLowSpeed()
{
    var agent = new Agent
    {
        Genome = new AgentGenome(
            BaseThrottle: 0.8f,
            BaseTurnRate: 0.0f,
            WallAvoidanceWeight: 0.0f,
            NeighborAvoidanceWeight: 0.0f,
            DecisionIntervalSeconds: 0.2f,
            DecisionIntervalJitterSeconds: 0.0f,
            WanderAmplitude: 0.0f,
            WanderFrequency: 1.0f,
            StuckTurnBias: 2.5f),
        Position = Vector2.Zero,
        Velocity = Vector2.Zero,
        Heading = 0.0f,
        Radius = 4.0f,
        BrainClock = 0.0f
    };

    var observation = new AgentObservation(
        ForwardWallDistance: 5.5f,
        LeftWallDistance: 8.0f,
        RightWallDistance: 25.0f,
        NearestNeighborLocalOffset: Vector2.Zero,
        HasNeighbor: false,
        Speed: 1.0f,
        IsTouchingWall: true);

    var decision = AgentBrain.Decide(agent, observation, new SimulationSettings
    {
        WallAvoidanceDistance = 60.0f,
        StuckSpeedThreshold = 8.0f
    });

    AssertTrue(decision.TurnRate > 2.0f, "Agent should get a strong unstuck turn when slow and pressed against a wall.");
}

void TestDecisionTimersDifferPerAgentBecauseOfJitter()
{
    var simulation = new Simulation(new SimulationSettings
    {
        WorldSize = new Vector2(200.0f, 120.0f),
        AgentCount = 2,
        AgentRadius = 4.0f
    }, seed: 123);

    simulation.InitializeAgents();

    var firstTimer = simulation.WorldState.Agents[0].DecisionTimer;
    var secondTimer = simulation.WorldState.Agents[1].DecisionTimer;

    AssertTrue(MathF.Abs(firstTimer - secondTimer) > 0.0001f, "Agents should not share the exact same next decision time.");
}

void AssertTrue(bool condition, string message)
{
    if (!condition)
    {
        throw new InvalidOperationException(message);
    }
}

void AssertNearlyEqual(float expected, float actual, float tolerance, string message)
{
    if (MathF.Abs(expected - actual) > tolerance)
    {
        throw new InvalidOperationException($"{message} Expected {expected}, got {actual}.");
    }
}
