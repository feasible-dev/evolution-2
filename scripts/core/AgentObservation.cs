using Godot;

namespace EvoPrototype.Core;

public readonly record struct AgentObservation(
    float ForwardWallDistance,
    float LeftWallDistance,
    float RightWallDistance,
    Vector2 NearestNeighborLocalOffset,
    bool HasNeighbor);
