# Evolution Wars

Godot 4.6.1 .NET / C# evolution tournament prototype. The playable vertical slice
is in **`prototype/`**. The root Godot project is the older simulation sandbox.

## Start here

- [Current state and next session](docs/CURRENT_STATE.md)
- [Approved art and previews](art-direction/README.md)
- [Architecture and checks](EVOLUTION_GAME_AGENT_GUIDE.md)
- [First-level decisions](FIRST_LEVEL_DIRECTION.md)
- [Future card vocabulary](MUTATION_CARD_DESIGN.md)

## Run

Open **`prototype/project.godot`** in Godot 4.6.1 with .NET support, or from
PowerShell at the repository root:

```powershell
.\Lancer-Evolution2.ps1
.\Lancer-Evolution2.ps1 -Language fr
.\Lancer-Evolution2.ps1 -Test
```

The launcher builds `prototype/EvolutionTwo.csproj`, imports changed images, and
launches `Main.tscn`. It expects Godot under `C:\dev\tools\godot`; on another
machine, update its installation path and `prototype/NuGet.Config`.
.NET 8 is selected by `global.json`.

## Art review

```powershell
python -m http.server 8774 --bind 127.0.0.1 --directory art-direction
```

Open <http://127.0.0.1:8774/>. If that server is already running, use it.
The retained neon sign and opponent introductions are approved review assets,
**not yet integrated into the game**. Runtime textures remain in `prototype/assets`.

Generated captures and logs are local outputs, not versioned source. Work on a
topic branch and submit changes through GitHub review; do not commit to `master`.
