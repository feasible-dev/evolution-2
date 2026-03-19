using Godot;

namespace EvoPrototype.Evolution;

public interface ISurvivalZone
{
    bool Contains(Vector2 position);
}
