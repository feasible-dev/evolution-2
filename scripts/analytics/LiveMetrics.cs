using System.Collections.Generic;
using EvoPrototype.Core;

namespace EvoPrototype.Analytics;

/// <summary>
/// Computes lightweight aggregate values for the current world state.
/// </summary>
public readonly record struct LiveMetrics(int AgentCount, float AverageEnergy, float AverageSpeed)
{
    public static LiveMetrics FromAgents(IReadOnlyCollection<Agent> agents)
    {
        if (agents.Count == 0)
        {
            return new LiveMetrics(0, 0.0f, 0.0f);
        }

        var totalEnergy = 0.0f;
        var totalSpeed = 0.0f;

        foreach (var agent in agents)
        {
            totalEnergy += agent.Energy;
            totalSpeed += agent.Velocity.Length();
        }

        var inverseCount = 1.0f / agents.Count;
        return new LiveMetrics(
            AgentCount: agents.Count,
            AverageEnergy: totalEnergy * inverseCount,
            AverageSpeed: totalSpeed * inverseCount);
    }
}
