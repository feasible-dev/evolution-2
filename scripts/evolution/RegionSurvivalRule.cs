using System.Collections.Generic;
using EvoPrototype.Core;

namespace EvoPrototype.Evolution;

/// <summary>
/// First survival rule: an agent lives if its center falls inside at least one configured region.
/// </summary>
public sealed class RegionSurvivalRule : ISurvivalRule
{
    private readonly IReadOnlyList<SurvivalRegion> _regions;

    public RegionSurvivalRule(IReadOnlyList<SurvivalRegion> regions)
    {
        _regions = regions;
    }

    public bool Survives(Agent agent, WorldState worldState, EvolutionSettings settings)
    {
        foreach (var region in _regions)
        {
            if (region.Contains(agent.Position))
            {
                return true;
            }
        }

        return false;
    }
}
