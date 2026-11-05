namespace EvolutionTwo.Presentation;

/// <summary>A local opponent commits its ten points before the round, independently of our cards.</summary>
public static class OpponentStrategy
{
    public static BudgetPlan Choose(int round)
    {
        // Favor the simple direction action, without setting an angle or knowing the safe zone.
        var random = new Random(1907 + round * 73);
        var direction = random.Next(6, 10);
        var turn = random.Next(0, Math.Min(2, BudgetPlan.Limit - direction) + 1);
        return new BudgetPlan(turn, BudgetPlan.Limit - direction - turn, direction);
    }
}
