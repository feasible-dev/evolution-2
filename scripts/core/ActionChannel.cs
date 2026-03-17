namespace EvoPrototype.Core;

/// <summary>
/// Groups action contributions by semantic channel so each family can later use its own resolver policy.
/// </summary>
public enum ActionChannel
{
    WorldAcceleration,
    LocalAcceleration,
    Turn,
    Brake
}
