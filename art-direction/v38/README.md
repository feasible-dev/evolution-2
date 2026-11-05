# v38 — Unified broadcast art and arena lettering

The refined v34 Gerard reference and duo portraits are the visual anchor for
this pass. The user's phrase “style de Dia” has no named asset in the repository;
the working interpretation is the detailed earlier portraits, rather than the
flat v35–v37 cartoon treatment. Earlier libraries remain untouched.

## One runtime family

All default conversation, rink gaze, blink, standing and desk-reaction images
now load from `prototype/assets/broadcast-v38`. Four new source sheets were
created with the built-in image generator, without external accounts or API
keys. Exact prompts and reference paths are in `broadcast/generation.json`.

The character models retain the friendly mature Gerard and composed older
René. Thin glasses, warm skin shading, restrained facial texture and smaller
eye whites replace the simplified flat outlines. Open eyes are the normal
watching state; closed lids appear only in the short existing blink schedule.
No changes were made to match thresholds, gaze timing, physics or cards.

The preparation script uses one seated master body. Speech changes a mouth
patch; gaze changes the head; blinks use matching eyelid patches. All clips are
quantized together to one 128-color palette. Standing and table reactions use
the identical seated master at entry and exit. René remains byte-identical
while his delayed reaction is pending. The original event timings, including
the 490 ms delay and long standing hold, are preserved.

This removes the mixed illustration styles and neutral-pose discontinuities.
Large gestures still use discrete illustrated poses; small shirt-fold and
silhouette changes between those drawings are not claimed to be fully solved.
The detailed v34/v35/v36/v37 source libraries are all preserved.

Rebuild:

```powershell
node art-direction/v38/broadcast/build-runtime.mjs
node art-direction/v38/build-alphabet.mjs
node art-direction/v38/build-preview.mjs
```

## Evolution Arena Pixel

An original five-column bitmap alphabet is authored in `build-alphabet.mjs`:
75 uppercase letters, digits, French accented letters and punctuation. There
is no external font dependency. Editable row masks, SVG atlas, PNG atlas and
character metadata are saved with the assets. Lowercase input is displayed
as uppercase; unsupported characters use a question mark.

Palette: cyan `#35e6ff`, magenta `#ff55bb`, yellow `#ffe64b`, orange `#ff963c`,
lime `#8bff76`, violet `#b497ff`. Each word keeps a stable color. A brief wave
brightens and enlarges letters up to 14% once every eight seconds, then settles.
The two strips have different effect phases. Fixed character advance avoids
text reflow; only visible glyphs draw, from a cached atlas. Messages retain
their 28 logical pixels/second right-to-left movement. The central score remains
steady. No full-screen flashing is introduced.

`index.html` is a local review gallery with actual runtime standing/slap GIFs,
conversation poses, alphabet and a ticker illustration. The Godot implementation
is authoritative; the gallery is a design preview. Actual game captures are in
`captures/`.

## Verification

- Prototype build: zero errors and warnings.
- Godot import completed.
- Direction/attention regression checks: 29 passed.
- Broadcast scheduling/pixel checks: 72 passed, including exact event entry,
  exit and shared neutral-pose equality for both actors.
- Eleven Godot captures produced; neutral, impact and ticker reviewed visually.
- Gameplay was not changed, so the earlier 40-generation run was not repeated.

The work is on `feat/broadcast-style-v38`; pre-existing uncommitted work was
preserved. Nothing was committed or pushed by this pass.
