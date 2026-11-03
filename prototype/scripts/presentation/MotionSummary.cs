namespace EvolutionTwo.Presentation;

public readonly record struct MotionSummary(float Speed, float EnergyPercent, float AccelerationCapacity,
    float Mass, float Drag, int Exhausted)
{
    public static MotionSummary Measure(LevelOneSession team)
    {
        var agents = team.Simulation.WorldState.Agents;
        if (agents.Count == 0) return default;
        float speed = 0, energy = 0, capacity = 0, mass = 0, drag = 0;
        var exhausted = 0;
        foreach (var agent in agents)
        {
            var profile = agent.PhysicsProfile;
            speed += agent.Velocity.Length();
            energy += profile.MaxEnergy > 0 ? agent.Energy / profile.MaxEnergy * 100 : 0;
            capacity += profile.MaxForwardAcceleration / profile.Mass;
            mass += profile.Mass; drag += profile.LinearDrag;
            if (agent.Energy <= 0) exhausted++;
        }
        return new(speed / agents.Count, energy / agents.Count, capacity / agents.Count,
            mass / agents.Count, drag / agents.Count, exhausted);
    }
}
