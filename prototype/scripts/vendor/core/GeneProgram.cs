namespace EvoPrototype.Core;

/// <summary>
/// Holds the small bounded rule set used by the current prototype.
/// </summary>
public readonly record struct GeneProgram(
    GeneRule PrimaryRule,
    GeneRule SecondaryRule,
    GeneRule TertiaryRule)
{
    public static GeneProgram Empty => new(GeneRule.Disabled, GeneRule.Disabled, GeneRule.Disabled);
}
