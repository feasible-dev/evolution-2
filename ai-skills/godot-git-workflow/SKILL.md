---
name: godot-git-workflow
description: Handle Git workflow for a Godot project with review-before-change, branch-based development, and correct tracking of Godot-generated project metadata. Use when Codex needs to prepare patches, stage changes, classify Godot-generated files, or suggest commit messages for this repository.
---

# Godot Git Workflow

Follow this workflow for all code and project-file changes in this repository.

## Core Rules

- Do not work directly on `master`.
- Work on a feature or development branch only.
- Prefer proposal-first: show the intended patch before applying it when the user asks for review-before-change.
- Keep `master` as the branch that only changes through reviewed merges on GitHub.

## Standard Change Loop

1. Inspect the working tree with `git status`.
2. Classify changed files before staging anything.
3. Explain what belongs in the next commit.
4. Stage only the intended files.
5. Review the staged diff with `git diff --cached`.
6. Suggest a focused commit message.
7. Commit only after the staged diff matches the intended unit of work.

Use PowerShell-safe commands. Do not chain commands with `&&`.

## File Classification

Treat files in these groups differently.

### Track normally

These usually belong in Git when intentionally created or updated:

- `scripts/**/*.cs`
- `scenes/**/*.tscn`
- `project.godot`
- `README.md`
- `AGENTS.md`
- `*.uid`
- `*.csproj`
- `*.sln`

### Ignore as local/generated cache

These should not be committed:

- `.godot/`
- `.import/`
- `.mono/`
- `bin/`
- `obj/`
- `.vs/`
- `.vscode/`
- `*.user`
- `*.rsuser`

## Godot-Generated Files

When Godot generates stable project metadata, track it.

Typical examples:

- script UID files like `Agent.cs.uid`
- C# project files like `evo-prototype.csproj`
- solution files like `evo-prototype.sln`

Do not confuse stable project metadata with cache directories.

## Review-Before-Commit Commands

Run these in order:

```powershell
git status
git add <intended files>
git diff --cached
```

If the staged diff is correct:

```powershell
git commit -m "type: short summary"
git push
```

## Commit Message Guidance

Use small, explicit commits.

Examples:

- `feat: add sandbox simulation loop`
- `fix: load sandbox scene as main scene`
- `chore: track Godot script uid files`
- `chore: track Godot generated project metadata`
- `docs: add agent workflow notes`

Prefer `chore` when the commit is about generated project files or repository maintenance rather than gameplay behavior.

## Recommended Name For This Versioning Step

Call this category:

`Godot generated project metadata`

Use that phrase for:

- `.uid` files
- `.csproj`
- `.sln`
- other stable Godot/.NET project descriptors

Preferred commit message when these appear together:

`chore: track Godot generated project metadata`

## How To Use This Skill In This Repo

When the user asks what to stage or commit after opening the project in Godot:

1. Identify new stable metadata files.
2. Separate them from cache and editor-local files.
3. Recommend a small `chore` commit.
4. Ask for confirmation before applying changes if the user wants review first.
