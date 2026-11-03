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
        IReadOnlyList<SurvivorSelection> survivors,
        EvolutionSettings evolutionSettings,
        SimulationSettings simulationSettings)
    {
        if (survivors.Count == 0)
        {
            return BuildFallbackPopulation(evolutionSettings, simulationSettings);
        }

        var randomImmigrantCount = ComputeRandomImmigrantCount(survivors.Count, evolutionSettings);
        var clonalOffspringCount = Math.Max(0, evolutionSettings.TargetPopulationSize - randomImmigrantCount);
        var nextGeneration = new List<Agent>(evolutionSettings.TargetPopulationSize);
        foreach (var allocation in AllocateChildCounts(survivors, evolutionSettings, clonalOffspringCount))
        {
            if (allocation.ChildCount <= 0)
            {
                continue;
            }

            var parent = survivors[allocation.ParentIndex].Agent;
            var radius = simulationSettings.AgentRadius;
            for (var childIndex = 0; childIndex < allocation.ChildCount; childIndex++)
            {
                var genome = _mutationSystem.MutateGenome(
                    parent.Genome,
                    evolutionSettings.MutationStrength,
                    evolutionSettings.MutationPolicy,
                    simulationSettings.GenePool,
                    simulationSettings.GenerationDurationSeconds,
                    evolutionSettings.RuleMutationProfile);
                var physicsProfile = evolutionSettings.FixedPhysicsProfile ?? _mutationSystem.MutatePhysicsProfile(parent.PhysicsProfile, evolutionSettings.MutationStrength);
                var spawnPosition = DetermineChildSpawnPosition(
                    parent,
                    childIndex,
                    allocation.ChildCount,
                    evolutionSettings,
                    simulationSettings,
                    radius);

                nextGeneration.Add(CreateAgent(
                    parent,
                    genome,
                    physicsProfile,
                    radius,
                    AgentVisualPalette.MutateAccent(_random, parent.Color),
                    spawnPosition,
                    allocation.ChildCount));
            }
        }

        for (var index = nextGeneration.Count; index < evolutionSettings.TargetPopulationSize; index++)
        {
            nextGeneration.Add(CreateRandomImmigrant(evolutionSettings, simulationSettings));
        }

        return nextGeneration;
    }

    private List<Agent> BuildFallbackPopulation(EvolutionSettings evolutionSettings, SimulationSettings simulationSettings)
    {
        var population = new List<Agent>(evolutionSettings.TargetPopulationSize);

        for (var index = 0; index < evolutionSettings.TargetPopulationSize; index++)
        {
            var seededGenome = AgentGenome.Default with
            {
                Program = GeneRuleFactory.CreateRandomProgram(
                    _random,
                    simulationSettings.GenePool,
                    simulationSettings.GenerationDurationSeconds)
            };
            var genome = _mutationSystem.MutateGenome(
                seededGenome,
                evolutionSettings.MutationStrength,
                evolutionSettings.MutationPolicy,
                simulationSettings.GenePool,
                simulationSettings.GenerationDurationSeconds,
                evolutionSettings.RuleMutationProfile);
            var physicsProfile = evolutionSettings.FixedPhysicsProfile ?? _mutationSystem.MutatePhysicsProfile(PhysicsProfile.Default, evolutionSettings.MutationStrength);
            var radius = simulationSettings.AgentRadius;
            var spawnPosition = RandomPosition(simulationSettings, radius);
            population.Add(CreateAgent(
                genome,
                physicsProfile,
                radius,
                AgentVisualPalette.CreateRandomAccent(_random),
                spawnPosition));
        }

        return population;
    }

    private int ComputeRandomImmigrantCount(int survivorCount, EvolutionSettings evolutionSettings)
    {
        var targetPopulationSize = evolutionSettings.TargetPopulationSize;
        if (targetPopulationSize <= 0 || survivorCount <= 0)
        {
            return 0;
        }

        var mortalityCount = Math.Max(0, targetPopulationSize - survivorCount);
        var randomImmigrantShare = Mathf.Clamp(evolutionSettings.RandomImmigrantMortalityShare, 0.0f, 1.0f);
        return Mathf.Clamp(
            Mathf.RoundToInt(mortalityCount * randomImmigrantShare),
            0,
            targetPopulationSize);
    }

    private Agent CreateRandomImmigrant(EvolutionSettings evolutionSettings, SimulationSettings simulationSettings)
    {
        var randomGenePool = simulationSettings.GenePool with
        {
            MaxRulesPerAgent = Math.Max(1, simulationSettings.GenePool.MaxRulesPerAgent - 1)
        };
        var genome = AgentGenome.Default with
        {
            Program = GeneRuleFactory.CreateRandomProgram(
                _random,
                randomGenePool,
                simulationSettings.GenerationDurationSeconds)
        };
        var physicsProfile = evolutionSettings.FixedPhysicsProfile ?? _mutationSystem.MutatePhysicsProfile(PhysicsProfile.Default, evolutionSettings.MutationStrength);
        var radius = simulationSettings.AgentRadius;
        var spawnPosition = RandomPosition(simulationSettings, radius);
        return CreateAgent(
            genome,
            physicsProfile,
            radius,
            AgentVisualPalette.CreateRandomAccent(_random),
            spawnPosition);
    }

    private IEnumerable<ChildAllocation> AllocateChildCounts(
        IReadOnlyList<SurvivorSelection> survivors,
        EvolutionSettings evolutionSettings,
        int targetPopulationSize)
    {
        var survivorCount = survivors.Count;
        if (survivorCount <= 0 || targetPopulationSize <= 0)
        {
            yield break;
        }

        var freshestLineageAge = int.MaxValue;
        for (var index = 0; index < survivorCount; index++)
        {
            freshestLineageAge = Mathf.Min(freshestLineageAge, survivors[index].Agent.LineageAge);
        }

        var freshestIndices = new List<int>(survivorCount);
        for (var index = 0; index < survivorCount; index++)
        {
            if (survivors[index].Agent.LineageAge == freshestLineageAge)
            {
                freshestIndices.Add(index);
            }
        }

        var reservedFreshChildren = Mathf.Clamp(
            Mathf.RoundToInt(targetPopulationSize * Mathf.Clamp(evolutionSettings.FreshLineageOffspringShare, 0.0f, 1.0f)),
            0,
            targetPopulationSize);
        foreach (var allocation in AllocateChildCountsInternal(survivors, targetPopulationSize, reservedFreshChildren, freshestIndices))
        {
            yield return allocation;
        }
    }

    private IEnumerable<ChildAllocation> AllocateChildCountsInternal(
        IReadOnlyList<SurvivorSelection> survivors,
        int targetPopulationSize,
        int reservedFreshChildren,
        List<int> freshestIndices)
    {
        var survivorCount = survivors.Count;
        var childCounts = new int[survivorCount];

        var generalChildren = targetPopulationSize - reservedFreshChildren;
        DistributeQuotaAcrossIndices(childCounts, survivors, generalChildren, null);

        if (reservedFreshChildren > 0 && freshestIndices.Count > 0)
        {
            DistributeQuotaAcrossIndices(childCounts, survivors, reservedFreshChildren, freshestIndices);
        }

        for (var index = 0; index < survivorCount; index++)
        {
            yield return new ChildAllocation(index, childCounts[index]);
        }
    }

    private void DistributeQuotaAcrossIndices(
        int[] childCounts,
        IReadOnlyList<SurvivorSelection> survivors,
        int quota,
        List<int>? eligibleIndices)
    {
        if (quota <= 0)
        {
            return;
        }

        var indices = eligibleIndices ?? CreateAllIndices(survivors.Count);
        if (indices.Count == 0)
        {
            return;
        }

        var baseChildren = quota / indices.Count;
        var remainder = quota % indices.Count;
        foreach (var index in indices)
        {
            childCounts[index] += baseChildren;
        }

        if (remainder <= 0)
        {
            return;
        }

        var prioritizedIndices = new List<int>(indices);
        Shuffle(prioritizedIndices);
        prioritizedIndices.Sort((left, right) =>
        {
            var scoreComparison = survivors[right].EndOfLifeScore.CompareTo(survivors[left].EndOfLifeScore);
            if (scoreComparison != 0)
            {
                return scoreComparison;
            }

            return survivors[left].Agent.LineageAge.CompareTo(survivors[right].Agent.LineageAge);
        });

        for (var index = 0; index < remainder; index++)
        {
            childCounts[prioritizedIndices[index]] += 1;
        }
    }

    private static List<int> CreateAllIndices(int count)
    {
        var indices = new List<int>(count);
        for (var index = 0; index < count; index++)
        {
            indices.Add(index);
        }

        return indices;
    }

    private Vector2 DetermineChildSpawnPosition(
        Agent parent,
        int childIndex,
        int childCount,
        EvolutionSettings evolutionSettings,
        SimulationSettings simulationSettings,
        float radius)
    {
        if (childCount <= 1)
        {
            return RandomPosition(simulationSettings, radius);
        }

        // Keep reproduction mostly global/random. Only larger broods start to
        // produce some locally clustered births around the parent's own origin.
        var localSpawnProbability = Mathf.Clamp((childCount - 2) / 10.0f, 0.0f, 0.55f);
        if ((float)_random.NextDouble() > localSpawnProbability)
        {
            return RandomPosition(simulationSettings, radius);
        }

        var boardWidth = Mathf.Max(simulationSettings.WorldSize.X, 1.0f);
        var minRadius = Mathf.Max(boardWidth * evolutionSettings.ReproductionLocalSpawnMinRadiusFactor, radius * 2.0f);
        var maxRadius = Mathf.Max(boardWidth * evolutionSettings.ReproductionLocalSpawnMaxRadiusFactor, minRadius);
        var locality = childCount <= 1 ? 1.0f : Mathf.Pow(childIndex / (float)(childCount - 1), 1.35f);
        var diskRadius = Mathf.Lerp(maxRadius, minRadius, locality);

        for (var attempt = 0; attempt < evolutionSettings.ReproductionLocalSpawnAttempts; attempt++)
        {
            var angle = NextFloat(0.0f, Mathf.Tau);
            var distance = Mathf.Sqrt((float)_random.NextDouble()) * diskRadius;
            var candidate = parent.SpawnOrigin + (Vector2.FromAngle(angle) * distance);
            if (BoardGeometry.IsInsidePlayableArea(
                candidate,
                simulationSettings.WorldSize,
                simulationSettings.BoardShape,
                simulationSettings.Obstacles,
                radius))
            {
                return candidate;
            }
        }

        return BoardGeometry.ClampPoint(
            parent.SpawnOrigin,
            simulationSettings.WorldSize,
            simulationSettings.BoardShape,
            simulationSettings.Obstacles,
            radius);
    }

    private Agent CreateAgent(
        AgentGenome genome,
        PhysicsProfile physicsProfile,
        float radius,
        Color color,
        Vector2 spawnPosition)
    {
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
            Position = spawnPosition,
            SpawnOrigin = spawnPosition,
            ParentSpawnOrigin = spawnPosition,
            MaxDistanceFromBirth = 0.0f,
            Velocity = Vector2.Zero,
            Heading = NextFloat(0.0f, Mathf.Tau),
            Radius = radius,
            Energy = physicsProfile.MaxEnergy,
            DecisionTimer = genome.DecisionIntervalSeconds,
            TimeSinceLastDecision = 0.0f,
            TimeBelowSpeedThreshold = 0.0f,
            ImmobileAxisXSeconds = 0.0f,
            ImmobileAxisYSeconds = 0.0f,
            IsTouchingWall = false,
            BrainClock = NextFloat(0.0f, Mathf.Tau),
            DecisionTurnNoise = NextFloat(-1.0f, 1.0f),
            DecisionThrottleNoise = NextFloat(-1.0f, 1.0f),
            Color = color
        };
    }

    private Agent CreateAgent(
        Agent parent,
        AgentGenome genome,
        PhysicsProfile physicsProfile,
        float radius,
        Color color,
        Vector2 spawnPosition,
        int childCount)
    {
        var agent = CreateAgent(genome, physicsProfile, radius, color, spawnPosition);
        agent.ParentAgentId = parent.AgentId;
        agent.HasParent = true;
        agent.GenerationNumber = parent.GenerationNumber + 1;
        agent.ParentChildCount = childCount;
        agent.LineageAge = genome.Program.Equals(parent.Genome.Program) ? parent.LineageAge + 1 : 0;
        agent.ParentSpawnOrigin = parent.SpawnOrigin;
        return agent;
    }

    private Vector2 RandomPosition(SimulationSettings simulationSettings, float radius)
    {
        return BoardGeometry.RandomPointInside(
            _random,
            simulationSettings.WorldSize,
            simulationSettings.BoardShape,
            simulationSettings.Obstacles,
            radius);
    }

    private float NextFloat(float minValue, float maxValue)
    {
        return minValue + ((float)_random.NextDouble() * (maxValue - minValue));
    }

    private void Shuffle(List<int> values)
    {
        for (var index = values.Count - 1; index > 0; index--)
        {
            var swapIndex = _random.Next(index + 1);
            (values[index], values[swapIndex]) = (values[swapIndex], values[index]);
        }
    }

    private readonly record struct ChildAllocation(int ParentIndex, int ChildCount);
}
