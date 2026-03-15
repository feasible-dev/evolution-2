# Evolution Prototype Agent Notes

This file is for coding assistants working in this repository.

## Project Goal

This repository contains a Godot 4.6 + C# evolution sandbox prototype.

The project is no longer just a movement MVP. The current baseline already includes:

- a 2D sandbox with continuous physics
- per-agent decision timing
- energy, drag, braking, and mass-aware movement
- generation-based selection, reproduction, and mutation
- a Godot HUD for live controls
- a first evolution graph panel with per-generation history

The next work should preserve that structure while extending the experiment surface for the player.

## Architectural Rules

Keep the project split by responsibility:

- `scripts/core/`: agent state, physics, simulation loop, observations, intents
- `scripts/evolution/`: survival rules, mutation, reproduction, generation cycle
- `scripts/analytics/`: generation snapshots, history, metric definitions
- `scripts/game/`: runtime orchestration and scene-level coordination
- `scripts/rendering/`: sandbox world rendering
- `scripts/ui/`: reusable UI drawing widgets such as charts

Simulation and evolution logic should stay as engine-light as practical. Godot should orchestrate, render, and expose controls, not own the model.

## Current Runtime Model

The main sandbox scene currently provides:

- a fixed world rectangle rendered on the left
- a right-side control panel for evolution and physics parameters
- a bottom graph panel for generation history

The live cycle is:

1. `Running` phase: agents move, spend energy, and decide asynchronously
2. `Selection` phase: survival bands appear and doomed agents show death markers
3. reproduction rebuilds the next generation through clonal mutation
4. analytics capture one generation snapshot for the graph

Current graph metrics:

- deaths per generation
- survival rate
- average mass
- average base throttle

## Important Defaults

The current tuning is intentionally faster than the original MVP:

- `Speed`: `8.0`
- `Life duration`: `10.0s`
- `Drag`: `0.08`
- `Mutation strength`: `0.03`
- `Band thickness`: `12.0`

If you change defaults, update both:

- `scripts/game/RunConfiguration.cs`
- `scenes/sandbox/SandboxScene.tscn`

and keep `project.godot` window sizing aligned with the scene layout.

## Testing Guidance

There is a custom console test runner in:

- `tests/EvoPrototype.Tests/Program.cs`

Use it to protect:

- physics invariants
- overlap resolution
- energy behavior
- reproduction and selection flow
- analytics and metric defaults

Preferred commands:

- `dotnet build evo-prototype.csproj`
- `dotnet run --project tests/EvoPrototype.Tests/EvoPrototype.Tests.csproj`

If Godot is running, the main build can fail because the DLL is locked. Stop `Play` or close Godot first.

## Coding Style

- Keep logic explicit and readable.
- Prefer plain C# data and direct update loops.
- Avoid clever abstractions unless they clearly reduce coupling.
- Keep comments and user-facing text in English.
- Prefer code that remains approachable to someone coming from Go or Python.

## Godot Guidance

- Do not create one heavy node per simulated agent.
- Prefer drawing from shared simulation state.
- Keep layout numbers consistent across `project.godot`, `RunConfiguration`, and the scene.
- When adjusting graph visuals, avoid layout shifts between empty and non-empty states.

## Git Workflow

- Do not work directly on `master`.
- Create a branch for each change and merge via GitHub review.
- Local hooks may block direct commit or push to `master`.
- When the user asks for review-before-change, propose the patch first.

## Files To Read First

- `EVOLUTION_GAME_AGENT_GUIDE.md`
- `README.md`
- `project.godot`
- `scripts/core/Simulation.cs`
- `scripts/evolution/EvolutionCycleController.cs`
- `scripts/game/SandboxController.cs`
- `scripts/ui/GenerationChart.cs`
