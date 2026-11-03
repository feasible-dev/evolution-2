using System.Collections.Generic;
using Godot;

namespace EvoPrototype.Core;

/// <summary>
/// Resolves rule-level contributions into the movement intent consumed by the current physics layer.
/// </summary>
public static class ActionSetResolver
{
    public static AgentIntent Resolve(IReadOnlyList<ActionContribution> contributions)
    {
        var worldAcceleration = Vector2.Zero;
        var localAcceleration = Vector2.Zero;
        var turn = 0.0f;
        var brake = 0.0f;

        foreach (var contribution in contributions)
        {
            switch (contribution.Channel)
            {
                case ActionChannel.WorldAcceleration:
                    worldAcceleration += contribution.VectorValue * contribution.Weight;
                    break;
                case ActionChannel.LocalAcceleration:
                    localAcceleration += contribution.VectorValue * contribution.Weight;
                    break;
                case ActionChannel.Turn:
                    turn += contribution.ScalarValue * contribution.Weight;
                    break;
                case ActionChannel.Brake:
                    brake += contribution.ScalarValue * contribution.Weight;
                    break;
            }
        }

        return new AgentIntent(
            WorldAccelerationWish: worldAcceleration,
            LocalAccelerationWish: localAcceleration,
            TurnWish: turn,
            BrakeWish: brake);
    }
}
