using Godot;

namespace EvoPrototype.Core;

public static class AgentLineageFormatter
{
    public static string FormatAgentId(string agentId)
    {
        return string.IsNullOrWhiteSpace(agentId)
            ? "unknown"
            : (agentId.Length <= 8 ? agentId : agentId[..8]);
    }

    public static string FormatPosition(Vector2 position)
    {
        return $"({position.X:0}, {position.Y:0})";
    }

    public static float ComputeBirthDistance(Agent agent)
    {
        if (!agent.HasParent)
        {
            return 0.0f;
        }

        return agent.SpawnOrigin.DistanceTo(agent.ParentSpawnOrigin);
    }
}
