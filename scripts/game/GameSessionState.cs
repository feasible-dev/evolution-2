using EvoPrototype.Cards;
using EvoPrototype.Evolution;
using EvoPrototype.Gameplay;

namespace EvoPrototype.Game;

public sealed class GameSessionState
{
    public GameSessionState()
    {
        CardSelectionState = new CardSelectionState(
            CardCatalog.StarterSandboxDeck,
            CardCatalog.DefaultActiveCardIds);
    }

    public GamePhase CurrentPhase { get; set; } = GamePhase.Setup;

    public SurvivalZoneDefinition SurvivalZoneDefinition { get; set; } = SurvivalZoneDefinition.Default;

    public CardSelectionState CardSelectionState { get; }
}
