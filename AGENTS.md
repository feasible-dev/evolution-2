# Evolution Wars agent notes

## Read first

1. `README.md` — run the correct project.
2. `docs/CURRENT_STATE.md` — approved art, current functionality, pending work.
3. `EVOLUTION_GAME_AGENT_GUIDE.md` — architecture and verification.
4. `FIRST_LEVEL_DIRECTION.md`, `MUTATION_CARD_DESIGN.md` for gameplay changes.

## Project boundaries

The current game is `prototype/project.godot` / `prototype/EvolutionTwo.csproj`,
Godot 4.6.1 .NET, C# on .NET 8. Root `project.godot` is the older sandbox.
Current runtime code is in `prototype/scripts/presentation/` plus the vendored
engine in `prototype/scripts/vendor/`. Root `scripts/` is not the active game's
engine. Inspect vendor manifests before porting changes between the two.

Keep responsibilities separate: core state/physics, evolution, analytics, game
orchestration, rendering and reusable UI. Keep simulation engine-light; no heavy
node per agent. Prefer plain C#, explicit loops, readable code and English comments.
Keep equal physics rules for both teams. Avoid layout shifts in empty charts.

For the legacy sandbox only, defaults live in both `scripts/game/RunConfiguration.cs`
and `scenes/sandbox/SandboxScene.tscn`; keep window/layout sizing consistent with
its `project.godot`. These are not the playable slice's defaults.

## Verification

- Active game: `.\Lancer-Evolution2.ps1 -Test`.
- Root engine: `dotnet build evo-prototype.csproj` and
  `dotnet run --project tests/EvoPrototype.Tests/EvoPrototype.Tests.csproj`.
- Art: approved review links and optional rebuilds in `art-direction/README.md`.
- Godot can lock compiled DLLs while playing. Stop relevant Play before rebuilding.

## Art and repository hygiene

`prototype/assets/` contains runtime assets. Do not delete older-numbered folders
solely because their version number looks stale; current code uses several of them.
`art-direction/` contains selected sources and review assets, not a second runtime.
Neon option 02 and opponent entrances are approved but still pending integration.
Keep both rink-facing and camera-facing opponent endings.
Write fresh captures, videos and temporary output under ignored `.cache/`.
Do not commit logs, imports, node_modules or per-frame exports. Preserve source
sheets, prompts and timing needed to reproduce retained assets.

## Git

Do not work directly on `master`. Use a topic branch and GitHub review for merging.
Inspect status, stage intended files, review the staged diff, then commit/push when
requested. When the user requests review-before-change, show the patch first.
Track stable Godot `.uid` files and project descriptors; ignore generated caches.
See `ai-skills/godot-git-workflow/SKILL.md` for the repository workflow.
