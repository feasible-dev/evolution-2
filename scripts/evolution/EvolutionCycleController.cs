using System;
using System.Collections.Generic;
using EvoPrototype.Core;

namespace EvoPrototype.Evolution;

public enum EvolutionPhase
{
    Running,
    Selection
}

/// <summary>
/// Tracks generation time, evaluates survivors, and requests the next generation when a cycle ends.
/// </summary>
public sealed class EvolutionCycleController
{
    private readonly ISurvivalRule _survivalRule;
    private readonly ReproductionSystem _reproductionSystem;
    private readonly EvolutionSettings _evolutionSettings;
    private readonly SimulationSettings _simulationSettings;

    public EvolutionCycleController(
        ISurvivalRule survivalRule,
        ReproductionSystem reproductionSystem,
        EvolutionSettings evolutionSettings,
        SimulationSettings simulationSettings)
    {
        _survivalRule = survivalRule;
        _reproductionSystem = reproductionSystem;
        _evolutionSettings = evolutionSettings;
        _simulationSettings = simulationSettings;
    }

    public int GenerationIndex { get; private set; }

    public float GenerationElapsedSeconds { get; private set; }

    public float SelectionElapsedSeconds { get; private set; }

    public int LastSurvivorCount { get; private set; }

    public EvolutionPhase Phase { get; private set; } = EvolutionPhase.Running;

    public IReadOnlyCollection<Agent> SelectedSurvivors { get; private set; } = System.Array.Empty<Agent>();

    public float SelectionProgress =>
        _evolutionSettings.SelectionDurationSeconds <= 0.0f
            ? 1.0f
            : Math.Clamp(SelectionElapsedSeconds / _evolutionSettings.SelectionDurationSeconds, 0.0f, 1.0f);

    public bool Update(WorldState worldState, float delta)
    {
        if (Phase == EvolutionPhase.Running)
        {
            GenerationElapsedSeconds += delta;
            if (GenerationElapsedSeconds < _evolutionSettings.GenerationDurationSeconds)
            {
                return false;
            }

            var survivors = EvaluateSurvivors(worldState);
            LastSurvivorCount = survivors.Count;
            SelectedSurvivors = survivors;
            SelectionElapsedSeconds = 0.0f;
            Phase = EvolutionPhase.Selection;
            return false;
        }

        SelectionElapsedSeconds += delta;
        if (SelectionElapsedSeconds < _evolutionSettings.SelectionDurationSeconds)
        {
            return false;
        }

        var nextGeneration = _reproductionSystem.BuildNextGeneration(
            [.. SelectedSurvivors],
            _evolutionSettings,
            _simulationSettings);

        worldState.Agents.Clear();
        worldState.Agents.AddRange(nextGeneration);

        GenerationIndex += 1;
        GenerationElapsedSeconds = 0.0f;
        SelectionElapsedSeconds = 0.0f;
        SelectedSurvivors = System.Array.Empty<Agent>();
        Phase = EvolutionPhase.Running;
        return true;
    }

    public List<Agent> EvaluateSurvivors(WorldState worldState)
    {
        var survivors = new List<Agent>();

        foreach (var agent in worldState.Agents)
        {
            if (_survivalRule.Survives(agent, worldState, _evolutionSettings))
            {
                survivors.Add(agent);
            }
        }

        return survivors;
    }
}
