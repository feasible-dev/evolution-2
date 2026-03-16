using EvoPrototype.Evolution;

namespace EvoPrototype.Game;

public sealed class GameSessionState
{
    public GamePhase CurrentPhase { get; set; } = GamePhase.Setup;

    public SurvivalZoneDefinition SurvivalZoneDefinition { get; set; } = SurvivalZoneDefinition.Default;
}
