using System;
using System.Collections.Generic;
using EvoPrototype.Core;
using Godot;

namespace EvoPrototype.Evolution;

/// <summary>
/// Rebuilds a full generation from survivors, using clonal reproduction and small mutations.
/// </summary>
public sealed class ReproductionSystem
{
    private readonly Random _random;
    private readonly MutationSystem _mutationSystem;

    public ReproductionSystem(MutationSystem mutationSystem, int? seed = null)
    {
        _mutationSystem = mutationSystem;
        _random = seed.HasValue ? new Random(seed.Value) : new Random();
    }

    public List<Agent> BuildNextGeneration(
        IReadOnlyList<Agent> survivors,
        EvolutionSettings evolutionSettings,
        SimulationSettings simulationSettings)
    {
        if (survivors.Count == 0)
        {
            return BuildFallbackPopulation(evolutionSettings, simulationSettings);
        }

        var nextGeneration = new List<Agent>(evolutionSettings.TargetPopulationSize);
        for (var index = 0; index < evolutionSettings.TargetPopulationSize; index++)
        {
            var parent = survivors[_random.Next(survivors.Count)];
            var genome = _mutationSystem.MutateGenome(parent.Genome, evolutionSettings.MutationStrength);
            var physicsProfile = _mutationSystem.MutatePhysicsProfile(parent.PhysicsProfile, evolutionSettings.MutationStrength);
            var heading = NextFloat(0.0f, Mathf.Tau);
            var radius = simulationSettings.AgentRadius;

            nextGeneration.Add(new Agent
            {
                Genome = genome,
                PhysicsProfile = physicsProfile,
                Position = RandomPosition(simulationSettings.WorldSize, radius),
                Velocity = Vector2.Zero,
                Heading = heading,
                Radius = radius,
                Energy = physicsProfile.MaxEnergy,
                DecisionTimer = genome.DecisionIntervalSeconds,
                TimeSinceLastDecision = 0.0f,
                TimeBelowSpeedThreshold = 0.0f,
                IsTouchingWall = false,
                BrainClock = NextFloat(0.0f, Mathf.Tau),
                Color = parent.Color
            });
        }

        return nextGeneration;
    }

    private List<Agent> BuildFallbackPopulation(EvolutionSettings evolutionSettings, SimulationSettings simulationSettings)
    {
        var population = new List<Agent>(evolutionSettings.TargetPopulationSize);

        for (var index = 0; index < evolutionSettings.TargetPopulationSize; index++)
        {
            var genome = _mutationSystem.MutateGenome(AgentGenome.Default, evolutionSettings.MutationStrength);
            var physicsProfile = _mutationSystem.MutatePhysicsProfile(PhysicsProfile.Default, evolutionSettings.MutationStrength);
            var heading = NextFloat(0.0f, Mathf.Tau);
            var radius = simulationSettings.AgentRadius;

            population.Add(new Agent
            {
                Genome = genome,
                PhysicsProfile = physicsProfile,
                Position = RandomPosition(simulationSettings.WorldSize, radius),
                Velocity = Vector2.Zero,
                Heading = heading,
                Radius = radius,
                Energy = physicsProfile.MaxEnergy,
                DecisionTimer = genome.DecisionIntervalSeconds,
                TimeSinceLastDecision = 0.0f,
                TimeBelowSpeedThreshold = 0.0f,
                IsTouchingWall = false,
                BrainClock = NextFloat(0.0f, Mathf.Tau),
                Color = Color.FromHsv(NextFloat(0.0f, 1.0f), 0.65f, 0.95f)
            });
        }

        return population;
    }

    private Vector2 RandomPosition(Vector2 worldSize, float radius)
    {
        return new Vector2(
            NextFloat(radius, worldSize.X - radius),
            NextFloat(radius, worldSize.Y - radius));
    }

    private float NextFloat(float minValue, float maxValue)
    {
        return minValue + ((float)_random.NextDouble() * (maxValue - minValue));
    }
}
