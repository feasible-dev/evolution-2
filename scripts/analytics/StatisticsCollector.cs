using System.Collections.Generic;
using EvoPrototype.Core;

namespace EvoPrototype.Analytics;

/// <summary>
/// Aggregates a population into one compact generation snapshot.
/// </summary>
public static class StatisticsCollector
{
    public static GenerationStatistics Capture(
        int generationIndex,
        IReadOnlyCollection<Agent> population,
        IReadOnlyCollection<Agent> survivors)
    {
        var totalMass = 0.0f;
        var totalBaseThrottle = 0.0f;

        foreach (var agent in population)
        {
            totalMass += agent.PhysicsProfile.Mass;
            totalBaseThrottle += agent.Genome.BaseThrottle;
        }

        var populationSize = population.Count;
        var survivorCount = survivors.Count;
        var deathCount = populationSize - survivorCount;
        var inversePopulation = populationSize > 0 ? 1.0f / populationSize : 0.0f;

        return new GenerationStatistics(
            GenerationIndex: generationIndex,
            PopulationSize: populationSize,
            SurvivorCount: survivorCount,
            DeathCount: deathCount,
            SurvivalRate: populationSize > 0 ? (float)survivorCount / populationSize : 0.0f,
            AverageMass: totalMass * inversePopulation,
            AverageBaseThrottle: totalBaseThrottle * inversePopulation);
    }
}
