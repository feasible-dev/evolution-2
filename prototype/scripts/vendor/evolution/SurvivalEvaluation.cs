using EvoPrototype.Core;

namespace EvoPrototype.Evolution;

public readonly record struct SurvivalOutcome(bool Survives, float EndOfLifeScore);

public readonly record struct SurvivorSelection(Agent Agent, float EndOfLifeScore);
