namespace EvoPrototype.Evolution;

public sealed class EvolutionSettings
{
    public float GenerationDurationSeconds { get; init; } = 20.0f;

    public float SelectionDurationSeconds { get; init; } = 1.5f;

    public int TargetPopulationSize { get; init; } = 200;

    public float MutationStrength { get; init; } = 0.12f;

    public float SurvivalBandThickness { get; init; } = 64.0f;
}
