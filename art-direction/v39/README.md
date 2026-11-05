# v39 — Upright Gerard, seated excitement and fixed LED dots

Gerard's neutral, speech and rink-gaze source poses now have a comfortable
upright neck and level eyes. Small pupil/chin changes carry rink attention;
the diagonal head tilt is no longer his default. His refined v38 identity,
warm expression and clothing style remain the reference. Earlier art is kept.

Three new sources (seated, matching blink, excited aftermath) were generated
with built-in ImageGen. Prompts and reused source paths are preserved in
`broadcast/generation.json`. The existing stand/slap choreography is reused,
with new upright master images at entry and exit. The library is assembled
and quantized together by `broadcast/build-runtime.mjs`.

## Reaction sequence

First 95% and perfect-survival commentary now holds the initial line for at
least 7.2 seconds so the standing action can finish. A new localized Gerard
line starts an eight-second seated aftermath: settle back, shift toward the
left and right rink, gesture toward René, then relax. Mouth variants follow
the actual active speaker and stop moving when his text performance ends.
René answers the invitation 550 ms after Gerard turns toward him, remaining
independent. His analytical reply follows the eight-second aftermath.

This applies to real milestone commentary, including an opponent's perfect
result, and continues during the perfect-result pause. Ordinary rounds do not
acquire random standing or excited loops. Restart clears the aftermath state.
The existing 25-second limit on major gestures is retained. The seated
aftermath is a small performance, not a second standing event.

Dialogue is displayed text with animated mouths; no new recorded voice or
speech synthesis is added. Large motions remain discrete illustrated poses.

## LED message strips

Both scrolling message strips are now black fixed dot matrices. A single
canvas shader anchors circular bulbs at a 2.5 logical-pixel pitch. Dim inactive
bulbs remain visible. A cached text raster supplies their RGB values; scrolling
advances only the sampled text column, never bulb positions or bulb size.
Reading direction remains right-to-left at 28 logical pixels/second. A brief
brightness/color crest passes across the fixed bulbs. The central score is
still a separate steady display.

The v38 original alphabet and six-color palette are reused. The raster changes
only when the message changes; the shader renders the lattice in one draw per
strip, rather than creating one node or draw call per lamp. Message textures
are disposed when replaced. No simulation or card parameters changed.

## Review and rebuilding

`index.html` has a runtime-asset GIF of standing plus the seated aftermath and
a fixed-grid LED design preview. Actual Godot screenshots are in `captures/`.
The HTML preview is illustrative; Godot is the authoritative implementation.

```powershell
node art-direction/v39/broadcast/build-runtime.mjs
node art-direction/v39/build-preview.mjs
dotnet build prototype/EvolutionTwo.csproj
```

Validation: build succeeded with zero warnings/errors; 32 direction/attention
checks and 79 broadcast checks passed. The new checks cover the milestone
dialogue queue during pause, seated excitement timing, delayed partner response,
settling, reset and whole-column LED scroll. Fourteen graphical captures cover
the neutral posture, reactions and fixed-dot message strips. No new full
40-generation physics run was needed for this presentation-only change.

Branch: `feat/gerard-reactions-led-v39`. All earlier uncommitted work is preserved;
this pass does not commit or push.
