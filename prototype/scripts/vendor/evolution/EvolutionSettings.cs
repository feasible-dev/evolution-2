using System;

namespace EvoPrototype.Evolution;

public sealed class EvolutionSettings
{
    // Optional shared body for fair duels; null preserves physical evolution.
    public EvoPrototype.Core.PhysicsProfile? FixedPhysicsProfile { get; init; }

    public float GenerationDurationSeconds { get; init; } = 20.0f;

    public float SelectionDurationSeconds { get; init; } = 1.5f;

    public float SelectionMinRealTimeSeconds { get; init; } = 0.3f;

    public float SelectionMaxRealTimeSeconds { get; init; } = 1.5f;

    public float TimeScale { get; init; } = 8.0f;

    public int TargetPopulationSize { get; init; } = 200;

    public float ReproductionLocalSpawnMinRadiusFactor { get; init; } = 0.10f;

    public float ReproductionLocalSpawnMaxRadiusFactor { get; init; } = 1.60f;

    public int ReproductionLocalSpawnAttempts { get; init; } = 48;

    public float FreshLineageOffspringShare { get; init; } = 0.10f;

    public float RandomImmigrantMortalityShare { get; init; } = 0.30f;

    public float MutationStrength { get; init; } = 0.12f;

    public float BaseRandomJerkRuleWeight { get; init; } = 1.0f;

    public int BaseRandomJerkFadeGenerations { get; init; } = 50;

    public MutationPolicy MutationPolicy { get; init; } = MutationPolicy.Default;

    public RuleMutationProfile RuleMutationProfile { get; init; } = new();

    public SurvivalZoneDefinition SurvivalZoneDefinition { get; init; } = SurvivalZoneDefinition.Default;

    /// <summary>
    /// Selection visuals should stay readable regardless of the player's time speed.
    /// This converts the simulation-owned selection duration into a real-time duration
    /// clamped by challenge-owned bounds.
    /// </summary>
    public float GetEffectiveSelectionDurationSeconds()
    {
        var safeTimeScale = MathF.Max(0.0001f, TimeScale);
        var unclampedRealTime = SelectionDurationSeconds / safeTimeScale;
        var minReal = MathF.Max(0.0f, SelectionMinRealTimeSeconds);
        var maxReal = MathF.Max(minReal, SelectionMaxRealTimeSeconds);
        return Math.Clamp(unclampedRealTime, minReal, maxReal);
    }
}
