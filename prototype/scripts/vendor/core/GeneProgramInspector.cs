using System.Collections.Generic;

namespace EvoPrototype.Core;

public static class GeneProgramInspector
{
    public static List<GeneRule> GetEnabledRules(GeneProgram program)
    {
        var enabledRules = new List<GeneRule>(3);
        AddIfEnabled(enabledRules, program.PrimaryRule);
        AddIfEnabled(enabledRules, program.SecondaryRule);
        AddIfEnabled(enabledRules, program.TertiaryRule);
        return enabledRules;
    }

    public static IEnumerable<GeneRule> EnumerateEnabledRules(Agent agent)
    {
        var program = agent.Genome.Program;
        if (IsEnabled(program.PrimaryRule))
        {
            yield return program.PrimaryRule;
        }

        if (IsEnabled(program.SecondaryRule))
        {
            yield return program.SecondaryRule;
        }

        if (IsEnabled(program.TertiaryRule))
        {
            yield return program.TertiaryRule;
        }
    }

    private static void AddIfEnabled(ICollection<GeneRule> rules, GeneRule rule)
    {
        if (IsEnabled(rule))
        {
            rules.Add(rule);
        }
    }

    private static bool IsEnabled(GeneRule rule)
    {
        return rule.Enabled && rule.ActionKind != GeneActionKind.None;
    }
}
