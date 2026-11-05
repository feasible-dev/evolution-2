# Broadcast animation studies v35

This revision follows the selected Gerard proposal 3. It reduces facial noise:
larger skin and hair color clusters, fewer wrinkle marks, narrow eyes, and
less exposed sclera. Age is conveyed through the face shape and a few folds.
The simplified treatment is deliberately more graphic than v34.

Open `index.html` locally for five playable sequences. Pause, replay, slow down
or scrub individual frames. Each sequence also has a GIF.

## Timing and continuity

- Standing: 18 timed composite frames from eight drawn duo poses. Gerard begins
  rising first. Le Chevalier holds the identical original pose for 490 ms,
  notices with his eyes, turns in surprise, then raises his chin. Gerard holds
  standing for 3.74 seconds, including two small forearm gestures, before sitting.
  His head intentionally passes above the frame. Le Chevalier also follows the
  return to sitting with a delay.
- Table slap: ten composite frames from eight drawn poses. The active gesture
  has a rounded astonished mouth, not a grin. Le Chevalier reacts 180 ms after
  contact. Incorrectly placed generated impact ticks were removed.
- Basic watching and speech share one eight-pose source. A fixed face contour,
  glasses, hair and torso prevent texture crawling. Only the eye or mouth area
  changes for small movements; the explanatory hand gesture changes the lower
  body region. The two close eye-tracking poses remain subtle at native size.
- The partner glance retains a short asymmetric response before eye contact.

Eight illustrated poses do not mean eighteen anatomical inbetweens. Independent
actor holds and pose combinations create the additional timed standing frames.
Small costume and posture shifts remain in the larger generated gestures; this
is not a skeletal animation rig or phoneme-based lip sync.

## Files and reproducibility

Sources are four 1536 x 1024 sheets, each a 2-column, 4-row grid. Prepared frames
are 384 x 128, with a shared stationary desk and a 64-color, undithered palette.
GIFs display at 768 x 256. `scenes.json` records every actor pose and duration.

`exports/` contains ten actor resources, including Godot SpriteFrames `.tres`
and PNG atlases, generated locally with Pixelbolt 0.11.0. Each actor half is
192 x 128 with an opaque background and baked desk. Start the matching actor
animations on the same clock: the reaction delays are already encoded. Idle
and speech loop; the event resources are one-shot. Gallery GIFs loop for review.

Images were made with the built-in image generator; prompts are in
`generation.json`. Sharp 0.34.5 performs deterministic crops, stable facial
patches, desk alignment and palette preparation. No external generation
account or API key was used. Older libraries are preserved.

From the repository root, with the existing pinned local tools installed:

```powershell
node art-direction/v35/duo-scenes/build.mjs
```

The build verifies GIF dimensions, frame counts and all durations, standing
timings, and byte-identical held actors. `verification.json` records the checks.
The gallery script was syntax-checked and prepared sheets visually inspected.
Browser playback has not been verified in this session.

These are review assets. The running game still uses its existing portraits;
this pass does not replace its portrait shader or event orchestration.
