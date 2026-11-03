using System;
using System.Collections.Generic;
using System.Linq;
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
    public readonly record struct Snapshot(
        int GenerationIndex,
        float GenerationElapsedSeconds,
        float SelectionElapsedSeconds,
        int LastSurvivorCount,
        EvolutionPhase Phase,
        bool ManualAdvanceEnabled,
        bool WaitingForManualAdvance);

    private readonly ISurvivalRule _survivalRule;
    private readonly ReproductionSystem _reproductionSystem;
    private EvolutionSettings _evolutionSettings;
    private SimulationSettings _simulationSettings;
    private bool _manualAdvanceEnabled;
    private bool _waitingForManualAdvance;

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

    public IReadOnlyList<SurvivorSelection> SelectedSurvivorSelections { get; private set; } = System.Array.Empty<SurvivorSelection>();

    public bool ManualAdvanceEnabled => _manualAdvanceEnabled;

    public bool WaitingForManualAdvance => _waitingForManualAdvance;

    public float SelectionProgress =>
        _evolutionSettings.GetEffectiveSelectionDurationSeconds() <= 0.0f
            ? 1.0f
            : Math.Clamp(SelectionElapsedSeconds / _evolutionSettings.GetEffectiveSelectionDurationSeconds(), 0.0f, 1.0f);

    /// <summary>
    /// Running phase advances in simulation time, but selection readability advances in real time.
    /// Keeping both deltas explicit avoids coupling UI/selection readability to the player's time-speed slider.
    /// </summary>
    public bool Update(WorldState worldState, float simulationDelta, float realDelta)
    {
        if (Phase == EvolutionPhase.Running)
        {
            GenerationElapsedSeconds += simulationDelta;
            if (GenerationElapsedSeconds < _evolutionSettings.GenerationDurationSeconds)
            {
                return false;
            }

            var survivors = EvaluateSurvivorSelections(worldState);
            LastSurvivorCount = survivors.Count;
            SelectedSurvivorSelections = survivors;
            SelectedSurvivors = [.. survivors.Select(selection => selection.Agent)];
            SelectionElapsedSeconds = 0.0f;
            Phase = EvolutionPhase.Selection;
            return false;
        }

        SelectionElapsedSeconds += realDelta;
        if (SelectionElapsedSeconds < _evolutionSettings.GetEffectiveSelectionDurationSeconds())
        {
            return false;
        }

        if (_manualAdvanceEnabled && !_waitingForManualAdvance)
        {
            _waitingForManualAdvance = true;
            return false;
        }

        if (_manualAdvanceEnabled && _waitingForManualAdvance)
        {
            return false;
        }

        var nextGeneration = _reproductionSystem.BuildNextGeneration(
            SelectedSurvivorSelections,
            _evolutionSettings,
            _simulationSettings);

        worldState.Agents.Clear();
        worldState.Agents.AddRange(nextGeneration);

        GenerationIndex += 1;
        GenerationElapsedSeconds = 0.0f;
        SelectionElapsedSeconds = 0.0f;
        SelectedSurvivors = System.Array.Empty<Agent>();
        SelectedSurvivorSelections = System.Array.Empty<SurvivorSelection>();
        _waitingForManualAdvance = false;
        Phase = EvolutionPhase.Running;
        return true;
    }

    public void SetManualAdvanceEnabled(bool enabled)
    {
        _manualAdvanceEnabled = enabled;
        if (!enabled)
        {
            _waitingForManualAdvance = false;
        }
    }

    public bool AdvanceToNextGeneration(WorldState worldState)
    {
        if (!_manualAdvanceEnabled || !_waitingForManualAdvance || Phase != EvolutionPhase.Selection)
        {
            return false;
        }

        var nextGeneration = _reproductionSystem.BuildNextGeneration(
            SelectedSurvivorSelections,
            _evolutionSettings,
            _simulationSettings);

        worldState.Agents.Clear();
        worldState.Agents.AddRange(nextGeneration);

        GenerationIndex += 1;
        GenerationElapsedSeconds = 0.0f;
        SelectionElapsedSeconds = 0.0f;
        SelectedSurvivors = System.Array.Empty<Agent>();
        SelectedSurvivorSelections = System.Array.Empty<SurvivorSelection>();
        _waitingForManualAdvance = false;
        Phase = EvolutionPhase.Running;
        return true;
    }

    public Snapshot CreateSnapshot()
    {
        return new Snapshot(
            GenerationIndex,
            GenerationElapsedSeconds,
            SelectionElapsedSeconds,
            LastSurvivorCount,
            Phase,
            _manualAdvanceEnabled,
            _waitingForManualAdvance);
    }

    public void RestoreSnapshot(Snapshot snapshot, WorldState worldState, IReadOnlyCollection<Agent> selectedSurvivors)
    {
        GenerationIndex = snapshot.GenerationIndex;
        GenerationElapsedSeconds = snapshot.GenerationElapsedSeconds;
        SelectionElapsedSeconds = snapshot.SelectionElapsedSeconds;
        LastSurvivorCount = snapshot.LastSurvivorCount;
        Phase = snapshot.Phase;
        SelectedSurvivors = selectedSurvivors;
        SelectedSurvivorSelections = snapshot.Phase == EvolutionPhase.Selection
            ? [.. selectedSurvivors.Select(agent => EvaluateSurvivorSelection(agent, worldState))]
            : System.Array.Empty<SurvivorSelection>();
        _manualAdvanceEnabled = snapshot.ManualAdvanceEnabled;
        _waitingForManualAdvance = snapshot.WaitingForManualAdvance;
    }

    public List<Agent> EvaluateSurvivors(WorldState worldState)
    {
        return [.. EvaluateSurvivorSelections(worldState).Select(selection => selection.Agent)];
    }

    public List<SurvivorSelection> EvaluateSurvivorSelections(WorldState worldState)
    {
        var survivors = new List<SurvivorSelection>();

        foreach (var agent in worldState.Agents)
        {
            var evaluation = _survivalRule.Evaluate(agent, worldState, _evolutionSettings);
            if (evaluation.Survives)
            {
                survivors.Add(new SurvivorSelection(agent, evaluation.EndOfLifeScore));
            }
        }

        return survivors;
    }

    private SurvivorSelection EvaluateSurvivorSelection(Agent agent, WorldState worldState)
    {
        var evaluation = _survivalRule.Evaluate(agent, worldState, _evolutionSettings);
        return new SurvivorSelection(agent, evaluation.EndOfLifeScore);
    }

    public void UpdateSettings(EvolutionSettings evolutionSettings, SimulationSettings simulationSettings)
    {
        _evolutionSettings = evolutionSettings;
        _simulationSettings = simulationSettings;
    }
}
