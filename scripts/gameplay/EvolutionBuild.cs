using System.Collections.Generic;
using EvoPrototype.Cards;
using EvoPrototype.Core;
using EvoPrototype.Evolution;

namespace EvoPrototype.Gameplay;

/// <summary>
/// Mutable sandbox or round build produced by cards before compiling to runtime settings.
/// </summary>
public sealed class EvolutionBuild
{
    public float MutationStrength { get; set; }

    public MutationPolicy MutationPolicy { get; set; }

    public GenePoolDefinition GenePool { get; set; }

    public SurvivalZoneDefinition SurvivalZoneDefinition { get; set; }

    public List<CardDefinition> AppliedCards { get; } = new();

    public static EvolutionBuild FromDefaults(
        float mutationStrength,
        MutationPolicy mutationPolicy,
        GenePoolDefinition genePool,
        SurvivalZoneDefinition survivalZoneDefinition)
    {
        return new EvolutionBuild
        {
            MutationStrength = mutationStrength,
            MutationPolicy = mutationPolicy,
            GenePool = genePool,
            SurvivalZoneDefinition = survivalZoneDefinition
        };
    }
}
