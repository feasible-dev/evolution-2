# Evolution Wars — architecture and verification

Read `docs/CURRENT_STATE.md` for approved scope before changing anything.

## Two projects

| Project | Role | Entry point |
| --- | --- | --- |
| `prototype/EvolutionTwo.csproj` | Current playable broadcast slice | `prototype/Main.tscn` |
| `evo-prototype.csproj` | Older experiment sandbox and original engine | `scenes/sandbox/SandboxScene.tscn` |

The playable slice uses its own copied engine in `prototype/scripts/vendor/`.
Its manifests document upstream provenance and local changes. Editing root
`scripts/` alone does not update gameplay in `prototype/`. Never blindly replace
the vendored engine or assume both copies are identical.

## Responsibilities

- `prototype/scripts/presentation/LevelOneSession.cs`: current run and duel flow.
- `BroadcastScene.cs`: Godot scene coordination and controls.
- `BroadcastAnimationAssets.cs` / `BroadcastAnimationDirector.cs`: presenter art and timing.
- `StudioBackdrop.cs`, `TickerStrip.cs`: studio layers and LED display.
- `CompetitorProfile.cs`, `CompetitorIdentity.cs`, `OpponentReactionDirector.cs`: opponents.
- Engine `core/`: state, physics, observations and decisions.
- Engine `evolution/`: survival, mutation, reproduction and generation cycle.
- Engine `analytics/`: history and metrics; `rendering/` and `ui/`: drawing.

Keep model logic as engine-light as practical. Godot orchestrates and draws;
do not create a heavy node per simulated agent. Use explicit loops and plain C#.
Comments and project-facing text should be English. Preserve equal team physics.

## Checks

From the repository root:

```powershell
.\Lancer-Evolution2.ps1 -Test
dotnet build evo-prototype.csproj
dotnet run --project tests/EvoPrototype.Tests/EvoPrototype.Tests.csproj
```

The launcher tests the actual playable project. Root console tests protect the
older core's physics, evolution, analytics and cards; they do not replace the
playable project's smoke checks. Godot `-- --broadcast-review` additionally checks
presenter and studio assets. In headless mode it checks without taking screenshots.
Builds can fail if a running Godot game holds its DLL; stop the relevant Play
session before rebuilding. Do not close unrelated editors or discard unsaved work.

Capture output goes to `.cache/captures/`; logs and Godot imports stay ignored.
For art preview commands and source locations, see `art-direction/README.md`.
