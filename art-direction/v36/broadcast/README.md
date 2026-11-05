# Playable broadcast integration v36

The first level now uses the v35 simplified animation treatment by default.
The v34 textured study and all v35 sources remain unchanged. The user considers
this a small-size playtest, not a final art-style choice. A future middle ground
should restore a little facial texture while retaining stable color clusters.

## Performance

The two commentators have separate conversational look tracks. Four patterns
rotate between utterances: an unanswered speaker glance, a delayed reply to the
speaker, an unanswered listener glance, and a listener-led exchange. They work
in both directions. The initiator returns to the rink before the responding
partner. Short intermediate poses, independently timed blinks, closed-mouth
beats and a small Gerard hand gesture break up the basic speech cycle.

Le Chevalier has new speaking poses facing the rink and facing Gerard. The
source was generated with the built-in image tool; see `generation.json` for
the exact prompt. Local Sharp preparation locks the body and facial contours
between syllables. These are stylized mouth cycles, not audio-driven lip sync.

The first selection triggers the table reaction. The first result at or above
90% survival for either side queues standing; perfect survival also queues a
celebration. One pending reaction is retained and gestures cannot interrupt
each other. A minimum 18-second real-time gap prevents repeated reactions at
high simulation speed. Animation continues during a simulation pause. Restart
clears both pending and current performance state.

Standing retains the v35 490 ms delayed reaction and 3.74-second standing hold.
Large gestures still have small pose/costume differences. Runtime rendering
uses the prepared palette directly at 192 x 128 per actor, with nearest
filtering; it does not use the old 16-color recoloring shader.

## Run and reproduce

From the repository root:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\Lancer-Evolution2.ps1
```

The normal English game opens in card preparation. French remains available
through `-Language fr`. The unchanged v34 comparison gallery is at
`../../v34/duo-scenes/index.html`. The Godot-only `--classic-portraits` switch
selects the much older v13/v14 runtime assets, not the v34 art study.

Rebuild the runtime image banks with:

```powershell
node art-direction/v36/broadcast/build-runtime.mjs
```

The output lives in `prototype/assets/broadcast-v36/`, including a manifest
with event frame durations. The launcher now imports nested asset folders.

`BroadcastAnimationDirector.cs` owns performance timing independently of the
simulation. `BroadcastAnimationAssets.cs` loads frames. The previous renderer
remains available for its existing portrait regression test.

## Verification

`--broadcast-review` checks timing and actual mouth/body pixels, then captures
seven in-game poses when run with graphics. `--smoke-test` includes these checks
as part of the complete first-level playtest. Captures are in `../captures/`.

Two older verification problems were repaired while running the suites: the
root project accidentally compiled the separate prototype's copied engine,
and the selection fixture used the old fixed world height instead of the
resized arena. No simulation tuning or team physics changed in this pass.
