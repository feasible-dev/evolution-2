# Evolution Game Prototype

Godot 4 + C# prototype for a simple 2D simulation.

## Current Status

The project now contains a basic MVP:

- a sandbox scene
- a pure C# simulation
- 200 agents
- continuous movement
- bouncing at world boundaries
- simple 2D rendering in Godot

## Structure

```text
prototype/
  scenes/sandbox/
  scripts/core/
  scripts/game/
  scripts/rendering/
  scripts/ui/
```

## First Run

1. Open the `prototype/` folder in Godot 4 with .NET/C# support.
2. If Godot asks to generate the C# project files, accept.
3. Wait for the C# project generation to finish.
4. Run the project.

The main scene is:

`res://scenes/sandbox/SandboxScene.tscn`

## Important Files

- `scripts/core/Simulation.cs`: main simulation loop
- `scripts/core/Agent.cs`: agent data
- `scripts/game/SandboxController.cs`: Godot orchestration
- `scripts/rendering/SandboxRenderer.cs`: agent rendering

## Next Steps

- add pause / reset
- display a few on-screen stats
- introduce energy
- add reproduction and mutation
