using Godot;

namespace EvoPrototype.Core;

/// <summary>
/// A compact mutable rule: when its condition is true, it emits an action contribution.
/// </summary>
public readonly record struct GeneRule(
    bool Enabled,
    GeneConditionKind ConditionKind,
    float ConditionThreshold,
    GeneActionKind ActionKind,
    float Scalar,
    Vector2 TargetPointNormalized)
{
    public Vector2 ConditionPointNormalized { get; init; } = new Vector2(0.5f, 0.5f);
    public float ConditionOrientationNormalized { get; init; } = 0.0f;
    // Absolute world angle, in turns: 0 points right, .25 down. Inherited and circular.
    public float DirectionAngleNormalized { get; init; }

    public static GeneRule Disabled => new(
        Enabled: false,
        ConditionKind: GeneConditionKind.Always,
        ConditionThreshold: 0.0f,
        ActionKind: GeneActionKind.None,
        Scalar: 0.0f,
        TargetPointNormalized: new Vector2(0.5f, 0.5f));
}
