namespace EvolutionTwo.Presentation;

/// <summary>A local opponent commits its ten points before the round, independently of our cards.</summary>
public static class OpponentStrategy
{
    public static BudgetPlan Choose(int round)
    {
        // Favor inherited targets for the right-edge challenge, with a little exploration.
        var random = new Random(1907 + round * 73);
        var target = random.Next(6, 10);
        var turn = random.Next(0, Math.Min(2, BudgetPlan.Limit - target) + 1);
        return new BudgetPlan(turn, BudgetPlan.Limit - target - turn, target);
    }
}
