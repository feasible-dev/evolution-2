namespace EvoPrototype.Core;

/// <summary>
/// Minimal set of action families currently supported by gene rules.
/// </summary>
public enum GeneActionKind
{
    None,
    AddTurnBias,
    AddThrottleBias,
    SteerToTarget,
    OrbitTarget,
    AddBrake
}
