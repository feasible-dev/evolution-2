using EvoPrototype.Core;

namespace EvoPrototype.Evolution;

public interface ISurvivalRule
{
    bool Survives(Agent agent, WorldState worldState, EvolutionSettings settings);
}
