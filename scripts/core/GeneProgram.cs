namespace EvoPrototype.Core;

/// <summary>
/// Holds a small fixed-size rule set for the current prototype.
/// </summary>
public readonly record struct GeneProgram(
    GeneRule PrimaryRule,
    GeneRule SecondaryRule)
{
    public static GeneProgram Empty => new(GeneRule.Disabled, GeneRule.Disabled);
}
