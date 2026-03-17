using Godot;

namespace EvoPrototype.Core;

/// <summary>
/// A single rule-level action request emitted before the final intent is resolved.
/// </summary>
public readonly record struct ActionContribution(
    ActionChannel Channel,
    Vector2 VectorValue,
    float ScalarValue,
    float Weight = 1.0f)
{
    public static ActionContribution WorldAcceleration(Vector2 value, float weight = 1.0f) =>
        new(ActionChannel.WorldAcceleration, value, 0.0f, weight);

    public static ActionContribution LocalAcceleration(Vector2 value, float weight = 1.0f) =>
        new(ActionChannel.LocalAcceleration, value, 0.0f, weight);

    public static ActionContribution Turn(float value, float weight = 1.0f) =>
        new(ActionChannel.Turn, Vector2.Zero, value, weight);

    public static ActionContribution Brake(float value, float weight = 1.0f) =>
        new(ActionChannel.Brake, Vector2.Zero, value, weight);
}
