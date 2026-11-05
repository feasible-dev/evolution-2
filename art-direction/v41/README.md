# v41 — Card clarity and stable speaking faces

The Direction card now has one diagonal arrow shaft. The extra parallel stroke
that made it look like two overlapping arrows is removed.

Gerard's smaller excited heads have been redrawn at a more consistent scale.
Only his head area changes: the existing bodies, gestures, hands and Rene are
preserved. Normal seated faces and earlier artwork remain available.

Speech no longer replaces a 36–41 by 22–24 pixel rectangle containing the jaw.
New restrained mouth drawings are registered to the closed-mouth pose and
confined to a 19 by 6 pixel lip area. Front, colleague-facing, rink-facing and
excited speech keep the chin, neck and head silhouette stable. Generated
colleague-facing mouths needed vertical registration before compositing.

## Review and source preservation

- Open `index.html` to compare animated old/new speech and face proportions.
- `broadcast/speech-pairs.png` shows closed/open poses side by side.
- `broadcast/before-after.png` compares the four excited head proportions.
- `broadcast/generation.json` records built-in image-generation prompts.
- Both generated source sheets and the reproducible build script are preserved.
- Runtime changes are isolated in `prototype/assets/broadcast-v41/`.
- The previous v30–v40 art studies, animation sources and implementation notes
  are included in the same checkpoint commit, not overwritten or discarded.

Run `node art-direction/v41/broadcast/build-runtime.mjs` after installing Sharp
under `.cache/sprite-tools` (the existing art tooling location). The game itself
does not need Node or Sharp; it loads the committed PNG/SVG assets.

## Validation

Both .NET projects compile without warnings or errors. The console simulation
suite passes. The Godot broadcast review additionally checks the actual imported
pixel resources: 15 Gerard speech pairs must show visible lip motion within a
20 by 6 pixel limit and preserve the lower jaw, neck and torso exactly. Existing
event endpoints, delayed reactions, direction logic and marquee checks remain
enabled. Final counts and inspected captures are recorded in `verification.json`.

Branch: `feat/vertical-slice-v41`, remote: `feasible-dev/evolution-2`.
