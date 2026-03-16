using System.Collections.Generic;
using EvoPrototype.Core;

namespace EvoPrototype.Evolution;

/// <summary>
/// First survival rule: an agent lives if its center falls inside at least one configured region.
/// </summary>
public sealed class RegionSurvivalRule : ISurvivalRule
{
    private readonly IReadOnlyList<ISurvivalZone> _zones;

    public RegionSurvivalRule(IReadOnlyList<ISurvivalZone> zones)
    {
        _zones = zones;
    }

    public bool Survives(Agent agent, WorldState worldState, EvolutionSettings settings)
    {
        foreach (var zone in _zones)
        {
            if (zone.Contains(agent.Position))
            {
                return true;
            }
        }

        return false;
    }
}
