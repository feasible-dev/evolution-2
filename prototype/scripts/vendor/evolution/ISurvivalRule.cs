using EvoPrototype.Core;

namespace EvoPrototype.Evolution;

public interface ISurvivalRule
{
    SurvivalOutcome Evaluate(Agent agent, WorldState worldState, EvolutionSettings settings);
}
