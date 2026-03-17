namespace EvoPrototype.Cards;

/// <summary>
/// Immutable card data. The UI can later display and drag these definitions directly.
/// </summary>
public readonly record struct CardDefinition(
    string Id,
    string Name,
    string Description,
    CardEffectKind EffectKind,
    float ScalarValue = 0.0f,
    int IntValue = 0);
