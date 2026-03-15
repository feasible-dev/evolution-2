# Evolution Prototype Agent Notes

This file is for coding assistants working in this repository.

## Project Goal

This repository contains an early Godot 4 + C# prototype for an evolution game.

The current goal is a small, clear, simulation-first MVP:

- a 2D sandbox
- around 200 moving agents
- simple world boundary handling
- rendering that reads simulation state
- architecture that stays easy to extend

## Architectural Rules

Prefer a strict separation between these layers:

- `scripts/core/`: simulation logic and data models
- `scripts/game/`: runtime orchestration and session control
- `scripts/rendering/`: Godot rendering code
- `scripts/ui/`: menus and HUD

Simulation code should stay as engine-light as practical. Favor simple C# data and explicit update loops over deep Godot coupling.

## Coding Style

- Keep logic simple and explicit.
- Prefer descriptive names over short abbreviations.
- Avoid premature abstractions.
- Avoid complex inheritance.
- Keep comments and user-facing text in English.
- Prefer straightforward code that would feel readable to someone coming from Go or Python.

## Godot Guidance

- Use Godot primarily for scene wiring, rendering, and UI.
- Do not model each simulated agent as its own heavy scene/node unless there is a clear reason.
- Prefer drawing many agents from shared simulation state.
- Keep the MVP fast and easy to reason about.

## Git Workflow

- Do not work directly on `master`.
- Create a branch for every change.
- Push branches to the remote and merge into `master` through GitHub review only.
- Local hooks may block direct commits or pushes to `master`.
- Before editing, prefer proposing a patch first when the user asks for review-before-change.

## Expected Near-Term Work

Priority order:

1. stabilize the sandbox MVP
2. add pause/reset controls
3. add basic on-screen metrics
4. introduce energy
5. introduce reproduction and mutation

## Files To Read First

- `EVOLUTION_GAME_AGENT_GUIDE.md`
- `README.md`
- `project.godot`
- `scripts/core/Simulation.cs`
- `scripts/game/SandboxController.cs`
- `scripts/rendering/SandboxRenderer.cs`
