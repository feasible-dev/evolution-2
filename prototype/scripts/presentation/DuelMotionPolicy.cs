using EvoPrototype.Core;

namespace EvolutionTwo.Presentation;

/// <summary>Shared action strengths and cadence. Behavior choices and absolute directions evolve.</summary>
public static class DuelMotionPolicy
{
    public const float DirectionAcceleration = 44;
    public const float DecisionInterval = .2f;
    public const float ExplorationWeight = .121f;

    public static AgentGenome Normalize(AgentGenome genome) => genome with
    {
        DecisionIntervalSeconds = DecisionInterval,
        DecisionIntervalJitterSeconds = 0,
        Program = new GeneProgram(Normalize(genome.Program.PrimaryRule),
            Normalize(genome.Program.SecondaryRule), Normalize(genome.Program.TertiaryRule))
    };

    private static GeneRule Normalize(GeneRule rule)
    {
        if (!rule.Enabled) return rule;
        return rule.ActionKind switch
        {
            GeneActionKind.SteerToTarget => rule with { Scalar = 1 },
            GeneActionKind.AccelerateDirection => rule with { Scalar = 1 },
            GeneActionKind.AddThrottleBias => rule with { Scalar = 1 },
            GeneActionKind.AddTurnBias => rule with { Scalar = rule.Scalar < 0 ? -1 : 1 },
            _ => rule
        };
    }
}
