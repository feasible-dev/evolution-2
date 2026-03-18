using System.Collections.Generic;
using System.Linq;
using EvoPrototype.Cards;

namespace EvoPrototype.Gameplay;

/// <summary>
/// Keeps the sandbox card loadout separate from the simulation runtime.
/// </summary>
public sealed class CardSelectionState
{
    private readonly List<CardDefinition> _catalog;
    private readonly List<CardDefinition> _activeCards = new();

    public CardSelectionState(IReadOnlyList<CardDefinition> catalog, IReadOnlyList<string>? initiallyActiveCardIds = null)
    {
        _catalog = catalog.ToList();

        if (initiallyActiveCardIds is null)
        {
            return;
        }

        foreach (var cardId in initiallyActiveCardIds)
        {
            TryActivate(cardId);
        }
    }

    public IReadOnlyList<CardDefinition> ActiveCards => _activeCards;

    public IReadOnlyList<CardDefinition> AvailableCards =>
        _catalog.Where(card => !_activeCards.Contains(card)).ToArray();

    public bool IsActive(string cardId)
    {
        return _activeCards.Exists(card => card.Id == cardId);
    }

    public bool TryActivate(string cardId)
    {
        if (IsActive(cardId))
        {
            return false;
        }

        var card = _catalog.FirstOrDefault(entry => entry.Id == cardId);
        if (string.IsNullOrEmpty(card.Id))
        {
            return false;
        }

        _activeCards.Add(card);
        return true;
    }

    public bool TryDeactivate(string cardId)
    {
        var index = _activeCards.FindIndex(card => card.Id == cardId);
        if (index < 0)
        {
            return false;
        }

        _activeCards.RemoveAt(index);
        return true;
    }
}
