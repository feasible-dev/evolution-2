using System.Collections.Generic;
using EvoPrototype.Core;
using Godot;

namespace EvoPrototype.Evolution;

/// <summary>
/// First survival rule: an agent lives if its center falls inside at least one configured region.
/// </summary>
public sealed class RegionSurvivalRule : ISurvivalRule
{
    private const float MinimumMovedDistancePixels = 6.0f;
    private readonly IReadOnlyList<ISurvivalZone> _zones;

    public RegionSurvivalRule(IReadOnlyList<ISurvivalZone> zones)
    {
        _zones = zones;
    }

    public SurvivalOutcome Evaluate(Agent agent, WorldState worldState, EvolutionSettings settings)
    {
        if (agent.MaxDistanceFromBirth < MinimumMovedDistancePixels)
        {
            return new SurvivalOutcome(false, 0.0f);
        }

        var bestScore = 0.0f;

        foreach (var zone in _zones)
        {
            if (zone.Contains(agent.Position))
            {
                bestScore = Mathf.Max(bestScore, zone.GetSelectionScore(agent.Position));
            }
        }

        return new SurvivalOutcome(bestScore > 0.0f, bestScore);
    }
}
