# Broadcast animation library

Open index.html for three independently controlled scenes, with pause, replay,
speed controls and frame scrubbing. GIF links can be opened without JavaScript.

- stand: Gerard stands and raises his arms outside the fixed broadcast frame;
  Le Chevalier turns toward him and raises his gaze as Gerard rises.
- glance: mutual eye contact, an aside, laughter and a restrained response.
- slap: anticipation, table impact, Le Chevalier's surprise and sideways reaction.

Gerard's approved v31 identity is used, with angular adult hands and hair on
the hands/forearms. The name is Gerard without an accent. Le Chevalier keeps his
existing silver hair, glasses, mustache, jacket and tie.

## Production and limitations

Each source contains six generated poses. These are discrete first animation
takes, not motion-captured or anatomically interpolated sequences. The standing
pose intentionally clips Gerard's head at the top. The source still shows a
little lower chin/neck; there is no attempt to shrink him back into frame.
The partner gaze follows the gesture's direction, but final pupil and hand
cleanup can still improve small-scale clarity. Clothing motifs and body scale
are not perfectly consistent across takes; these are not yet seamless clips
for arbitrary runtime transitions.

build.mjs detects the desk highlight at the outside edges, aligns it, replaces
the desk strip with one stationary line, resizes to 384 by 128 and quantizes a
shared palette per scene. This is the existing pair of 192 by 128 portrait sizes.
The final review frame repeats the first source pose exactly. Native poses are
retained. The GIF contains the real timed sequence, not a crossfade.

Pixelbolt exports each half as synchronized 192 by 128 SpriteFrames resources
in exports/. Both halves must start together and remain on the same clock.
They contain baked backgrounds and desk halves; they are not transparent actor
cutouts. The running game has not been switched to these assets.

## Rebuild

From the repository root, with the v30 pinned tools installed:

```powershell
node art-direction/v32/duo-scenes/build.mjs
```

Prompts and reference files are recorded in generation.json. Timings and
desk anchors are recorded in verification.json. No external generation account
or API key was used; image generation used the built-in service, with local
Sharp and Pixelbolt for preparation and export.
