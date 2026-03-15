namespace EvoPrototype.Core;

public readonly record struct PhysicsProfile(
    float Mass,
    float MaxForwardAcceleration,
    float MaxLateralAcceleration,
    float MaxBrakeAcceleration,
    float MaxTurnRate,
    float LinearDrag)
{
    public static PhysicsProfile Default => new(
        Mass: 1.0f,
        MaxForwardAcceleration: 220.0f,
        MaxLateralAcceleration: 100.0f,
        MaxBrakeAcceleration: 160.0f,
        MaxTurnRate: 3.2f,
        LinearDrag: 0.35f);
}
