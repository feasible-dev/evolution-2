using EvoPrototype.Core;
using EvoPrototype.Evolution;
using Godot;

namespace EvolutionTwo.Presentation;

public enum BehaviorChoice { Turn, Throttle, Target }

public readonly record struct BudgetPlan(int Turn, int Throttle, int Target)
{
    public const int Limit = 10;
    public static BudgetPlan Default => new(0, 0, 0);
    public int Total => Turn + Throttle + Target;
    public int Remaining => Limit - Total;
    public bool IsValid => Turn >= 0 && Throttle >= 0 && Target >= 0 && Total == Limit;
    public int this[BehaviorChoice choice] => choice switch
    {
        BehaviorChoice.Turn => Turn, BehaviorChoice.Throttle => Throttle, _ => Target
    };
    public float Chance(BehaviorChoice choice) => this[choice] / (float)Limit;
    public BudgetPlan Add(BehaviorChoice choice, int delta) => choice switch
    {
        BehaviorChoice.Turn => this with { Turn = Turn + delta },
        BehaviorChoice.Throttle => this with { Throttle = Throttle + delta },
        _ => this with { Target = Target + delta }
    };
    public static BudgetPlan Focus(BehaviorChoice choice) => choice switch
    {
        BehaviorChoice.Turn => new(Limit, 0, 0),
        BehaviorChoice.Throttle => new(0, Limit, 0),
        _ => new(0, 0, Limit)
    };

    public BudgetPlan Allocate(BehaviorChoice choice, int value)
    {
        if (!Enum.IsDefined(choice) || value < 0 || value > Limit)
            throw new ArgumentOutOfRangeException(nameof(value));
        return Add(choice, Math.Min(value, this[choice] + Remaining) - this[choice]);
    }

}

public sealed record BehaviorDefinition(BehaviorChoice Id, string Name, GeneActionKind Action, string Help)
{
    public Color Color => AgentVisualPalette.GetActionColor(Action);
}
public static class BehaviorCatalog
{
    public static readonly BehaviorDefinition[] All =
    {
        new(BehaviorChoice.Turn, L10n.Text("behavior.turn"), GeneActionKind.AddTurnBias, L10n.Text("behavior.turn_help")),
        new(BehaviorChoice.Throttle, L10n.Text("behavior.thrust"), GeneActionKind.AddThrottleBias, L10n.Text("behavior.thrust_help")),
        new(BehaviorChoice.Target, L10n.Text("behavior.target"), GeneActionKind.SteerToTarget, L10n.Text("behavior.target_help"))
    };
    public static BehaviorDefinition Get(BehaviorChoice id) => All.Single(d => d.Id == id);
    public static BehaviorDefinition? For(Agent agent)
    {
        var rule = AgentVisualPalette.GetDisplayRule(agent.Genome.Program);
        return rule.Enabled ? All.FirstOrDefault(d => d.Action == rule.ActionKind) : null;
    }
    public static RuleMutationProfile Profile(BudgetPlan plan)
    {
        var profile = new RuleMutationProfile();
        foreach (var rule in All)
        {
            var chance = plan.Chance(rule.Id);
            profile.ActionPopularities[rule.Action] = chance <= 0 ? 0 : MathF.Sqrt((chance * 3 - 0.01f) / 3);
        }
        return profile;
    }
    public static GenePoolDefinition Pool(BudgetPlan plan) => new(false, false, false, false,
        plan.Turn > 0, plan.Throttle > 0, plan.Target > 0, false, false, 1);
}
public sealed record RuleOutcome(int Total, int Survivors);
public sealed record PeriodOutcome(int Period, int Survivors, IReadOnlyDictionary<BehaviorChoice, RuleOutcome> Rules,
    RuleOutcome Neutral, BudgetPlan Plan)
{
    public float SurvivalRate => Survivors / (float)LevelOneSession.Population;
}
