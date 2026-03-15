
# Evolution Game Prototype — Agent Guide

## Project Overview

This repository contains the early prototype of an evolutionary simulation game built using **Godot 4 with C#**.

The long-term goal is to build a **small but polished simulation-based game that could eventually be released on Steam**.

The core idea is a **world containing many simple agents** that move, interact, consume energy, reproduce, mutate, and evolve over time.

The prototype phase focuses on building the **simulation core and rendering loop** in a clean and extensible architecture.

This project is **code-first** and optimized for development with tools like:

- VS Code
- Git
- AI coding assistants (Copilot, Codex, Claude Code, etc.)

The Godot editor is primarily used for:

- running the game
- rendering the world
- building UI and scenes

Most logic should live in **clean C# code files**.

---

# Long-Term Vision

The final game may include:

- multiple simulation worlds
- sandbox experimentation
- challenge modes
- evolutionary experiments
- player progression
- statistics and achievements
- persistent saves
- multiple simulation configurations
- visual dashboards and metrics

Because of this, the architecture must remain **clean, modular, and extensible**.

---

# MVP Goal

The first milestone is extremely simple.

The MVP should achieve:

- a 2D simulation world
- ~200 agents
- agents move continuously
- simple movement rules
- agents bounce off world boundaries
- agents have color / state
- simulation updates every frame

This MVP should run smoothly and be easily extensible.

No advanced gameplay systems are required yet.

---

# Architectural Principles

## 1. Simulation must be engine-independent

The **simulation core should not depend heavily on Godot**.

Simulation code should mostly use pure C# types and logic.

Example:

```
Simulation
  -> manages agents
  -> updates world state
  -> applies rules
```

Rendering should be separate.

```
Renderer
  -> reads simulation state
  -> draws agents
```

Benefits:

- easier testing
- easier refactoring
- faster simulations
- potential headless simulation
- clean separation of responsibilities

---

## 2. Clear separation of layers

The project should evolve into three logical layers.

### Core Simulation Layer

Pure logic.

Responsible for:

- agents
- world state
- movement
- energy
- reproduction
- mutation
- rules

No UI.

Minimal Godot dependencies.

Example files:

```
Agent.cs
Simulation.cs
WorldState.cs
Rules.cs
```

---

### Game Layer

Controls a running simulation session.

Responsible for:

- starting simulations
- pause/reset
- configuration
- scenario rules

Example files:

```
SandboxController.cs
RunConfiguration.cs
```

---

### Presentation Layer

Godot-specific rendering and UI.

Responsible for:

- scenes
- rendering agents
- menus
- HUD
- player input

Example files:

```
SandboxRenderer.cs
MainMenuController.cs
```

---

# Suggested Repository Structure

The following structure should guide development.

```
prototype/

scenes/
    sandbox/
        SandboxScene.tscn

scripts/

    core/
        Agent.cs
        Simulation.cs
        WorldState.cs

    game/
        SandboxController.cs
        RunConfiguration.cs

    ui/
        MainMenuController.cs

    rendering/
        SandboxRenderer.cs
```

Not all files exist yet, but this structure should guide development.

---

# Typical Development Workflow

Development typically follows this loop:

1. Edit code in **VS Code**
2. Use AI assistants to generate or refactor logic
3. Switch to Godot editor
4. Run the scene
5. Observe behavior
6. Iterate

Godot should mainly be used as:

- a runtime
- a renderer
- a UI builder

The **simulation logic should live in C# code files**.

---

# Coding Guidelines

These conventions should be respected.

## Prefer descriptive names

Avoid overly abbreviated names.

Bad:

```
sim
ag
cfg
```

Good:

```
simulation
agentList
runConfiguration
```

Variable and method names should be **readable and descriptive**, even if slightly longer.

---

## Keep classes focused

Each class should have a clear responsibility.

Example:

```
Simulation
    manages world update

Agent
    stores agent state

SandboxRenderer
    draws simulation state
```

Avoid classes that try to do everything.

---

## Avoid premature complexity

The prototype should stay simple.

Prefer:

- simple lists
- simple math
- simple rules

Avoid:

- complex inheritance hierarchies
- overengineering
- unnecessary design patterns

Architecture should remain **clear and flexible**.

---

# Simulation Style

The simulation should use a **data-driven model**.

Agents are simple objects containing:

- position
- velocity
- energy
- color / state
- possibly genome later

Example concept:

```
Agent
    position
    velocity
    energy
```

Simulation step:

```
for each agent:
    update movement
    apply world constraints
    update energy
```

Later versions may include:

- mutation
- reproduction
- food resources
- predators
- selection pressure

---

# Performance Expectations

The simulation should eventually support:

- hundreds or thousands of agents

Therefore:

- avoid heavy scene instancing per agent
- avoid physics nodes when possible
- prefer data arrays + manual updates

Rendering should simply visualize simulation state.

---

# Long-Term Extensibility

Future features may include:

- multiple worlds
- different evolutionary environments
- simulation presets
- player statistics
- persistent saves
- challenge scenarios

Therefore code should remain **modular and maintainable**.

---

# Important Principle

This project is primarily a **simulation-driven system**, not a traditional entity-heavy game.

The simulation logic is the most important part of the project.

Rendering and UI exist to **observe and interact with the simulation**, not to drive it.

---

# Immediate Next Step

The next step is to implement:

- `SandboxScene`
- `SandboxController`
- `Simulation`
- `Agent`

And render **200 moving agents in a 2D world**.

Once this works, the evolutionary mechanics can start being added incrementally.

---

End of document.
