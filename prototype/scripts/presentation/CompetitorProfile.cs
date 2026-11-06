namespace EvolutionTwo.Presentation;

// Presentation identity is separate from the simulation and the card strategy.
// A future character/opponent picker can supply its portrait and stable character ID here.
public sealed record CompetitorProfile(string Id, string NameKey, string CharacterKey, string? PortraitPath = null)
{
    public static CompetitorProfile Player { get; } = new("player", "profile.player", "profile.generic");
    public static CompetitorProfile Opponent { get; } = new("nico-valdes", "profile.nico.name", "profile.nico.origin",
        "res://assets/studio-v52/nico.png");
}
