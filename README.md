# Evolution Game Prototype

Prototype Godot 4 + C# d'une simulation 2D simple.

## Etat actuel

Le projet contient maintenant un MVP de base:

- une scene sandbox
- une simulation pure C#
- 200 agents
- deplacement continu
- rebond sur les bords du monde
- rendu 2D simple dans Godot

## Structure

```text
prototype/
  scenes/sandbox/
  scripts/core/
  scripts/game/
  scripts/rendering/
  scripts/ui/
```

## Premier lancement

1. Ouvrir le dossier `prototype/` dans Godot 4 avec support .NET/C#.
2. Si Godot demande de generer les fichiers C#, accepter.
3. Attendre la generation du projet C#.
4. Lancer le projet.

La scene principale est:

`res://scenes/sandbox/SandboxScene.tscn`

## Fichiers importants

- `scripts/core/Simulation.cs`: boucle principale de simulation
- `scripts/core/Agent.cs`: donnees d'un agent
- `scripts/game/SandboxController.cs`: orchestration Godot
- `scripts/rendering/SandboxRenderer.cs`: dessin des agents

## Etapes suivantes

- ajouter pause / reset
- afficher quelques stats a l'ecran
- introduire energie
- ajouter reproduction et mutation
