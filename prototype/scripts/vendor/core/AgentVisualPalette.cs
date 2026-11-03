using System;
using Godot;

namespace EvoPrototype.Core;

public static class AgentVisualPalette
{
    private static readonly Color[] BrightAccentPalette =
    {
        new(1.00f, 0.16f, 0.20f, 1.0f),
        new(0.10f, 0.58f, 1.00f, 1.0f),
        new(0.18f, 1.00f, 0.30f, 1.0f),
        new(1.00f, 0.92f, 0.08f, 1.0f),
        new(1.00f, 1.00f, 1.00f, 1.0f),
        new(1.00f, 0.18f, 0.84f, 1.0f),
        new(0.16f, 1.00f, 0.92f, 1.0f),
        new(1.00f, 0.50f, 0.05f, 1.0f)
    };

    public static Color CreateRandomAccent(Random random)
    {
        var baseColor = BrightAccentPalette[random.Next(BrightAccentPalette.Length)];
        return ApplyAccentVariation(baseColor, ((float)random.NextDouble() * 2.0f) - 1.0f);
    }

    public static Color MutateAccent(Random random, Color parentColor)
    {
        return ApplyAccentVariation(parentColor, ((float)random.NextDouble() * 2.0f) - 1.0f);
    }

    public static Color GetConditionColor(GeneConditionKind kind)
    {
        return kind switch
        {
            GeneConditionKind.Always => new Color(1.00f, 1.00f, 1.00f, 1.0f),
            GeneConditionKind.TimeGreaterOrEqual => new Color(1.00f, 0.28f, 0.22f, 1.0f),
            GeneConditionKind.DistanceToPointLessOrEqual => new Color(0.14f, 0.72f, 1.00f, 1.0f),
            GeneConditionKind.NeighborCountGreaterOrEqual => new Color(0.18f, 1.00f, 0.28f, 1.0f),
            GeneConditionKind.InsideHalfSpace => new Color(0.86f, 0.24f, 1.00f, 1.0f),
            _ => new Color(1.00f, 1.00f, 1.00f, 1.0f)
        };
    }

    public static Color GetActionColor(GeneActionKind kind)
    {
        return kind switch
        {
            GeneActionKind.None => new Color(1.00f, 0.92f, 0.08f, 1.0f),
            GeneActionKind.AddTurnBias => new Color(1.00f, 0.54f, 0.08f, 1.0f),
            GeneActionKind.AddThrottleBias => new Color(1.00f, 0.92f, 0.08f, 1.0f),
            GeneActionKind.SteerToTarget => new Color(0.12f, 0.76f, 1.00f, 1.0f),
            GeneActionKind.OrbitTarget => new Color(0.18f, 0.94f, 1.00f, 1.0f),
            GeneActionKind.AddBrake => new Color(1.00f, 0.12f, 0.16f, 1.0f),
            _ => new Color(1.00f, 1.00f, 1.00f, 1.0f)
        };
    }

    public static Color BlendLogicColor(Color logicColor, Color accent)
    {
        return logicColor.Lerp(accent, 0.18f);
    }

    public static GeneRule GetDisplayRule(GeneProgram program)
    {
        if (program.PrimaryRule.Enabled)
        {
            return program.PrimaryRule;
        }

        if (program.SecondaryRule.Enabled)
        {
            return program.SecondaryRule;
        }

        if (program.TertiaryRule.Enabled)
        {
            return program.TertiaryRule;
        }

        return GeneRule.Disabled;
    }

    private static Color ApplyAccentVariation(Color baseColor, float variation)
    {
        var hue = Mathf.PosMod(baseColor.H + (variation * 0.035f), 1.0f);
        var saturation = Mathf.Clamp(baseColor.S + (variation * 0.08f), 0.78f, 1.0f);
        var value = Mathf.Clamp(baseColor.V + (variation * 0.04f), 0.94f, 1.0f);
        return Color.FromHsv(hue, saturation, value);
    }
}
