namespace EvoPrototype.Core;

/// <summary>
/// Defines the physical limits and energy economy for a single agent.
/// </summary>
public readonly record struct PhysicsProfile(
    float Mass,
    float MaxForwardAcceleration,
    float MaxLateralAcceleration,
    float MaxBrakeAcceleration,
    float MaxTurnRate,
    float LinearDrag,
    float MaxEnergy,
    float IdleEnergyDrainPerSecond,
    float AccelerationEnergyCost,
    float BrakeEnergyCost,
    float TurnEnergyCost)
{
    public static PhysicsProfile Default => new(
        Mass: 1.0f,
        MaxForwardAcceleration: 220.0f,
        MaxLateralAcceleration: 100.0f,
        MaxBrakeAcceleration: 160.0f,
        MaxTurnRate: 3.2f,
        LinearDrag: 0.35f,
        MaxEnergy: 100.0f,
        IdleEnergyDrainPerSecond: 0.15f,
        AccelerationEnergyCost: 0.045f,
        BrakeEnergyCost: 0.025f,
        TurnEnergyCost: 0.012f);
}
