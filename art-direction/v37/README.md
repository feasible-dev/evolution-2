# v37 — Action cards and attention to the rink

## First-level cards

The playable deck is Turn, Thrust and Direction. All three are unconditional
actions. Direction is a new `AccelerateDirection` action, not a renamed target
seeker. Each agent inherits an angle in world space: 0 degrees points right,
90 degrees down. The angle is initially uniform over the circle, inherited by
offspring and locally mutated with circular wrapping. It never reads the
agent's position, a target point, or the survival region.

Both teams retain the same fixed physical profile, 44 u/s² direction push,
220 u/s² forward thrust and 1 rad/s turn strength. Energy and collisions still
affect actual motion. The ten allocation points remain draw weights, not force
sliders. Direction inspection displays the inherited angle. English and French
card faces, help text and diagnostics were updated, including earlier inaccurate
claims that thrust strength mutates. The opponent uses the same new gene pool.

Perception cards and perception/action combinations remain future work. The
engine's target-seeking action is preserved for later levels, but the first
level cannot generate it. The survival zone remains the existing rightmost 5%.

## Attention and reactions

`ArenaAttention` observes visible agents from both teams at 4 Hz. Overlapping
spatial windows estimate the largest local concentration and its centroid.
These windows span 30% of rink width and 38% of height; the displayed behavior
does not claim pixel-perfect co-location. Ordinary attention favors one shared
point. At 90% concentration, that shared point follows the cluster and each
commentator chooses it with 95% probability at his next attention change.
Otherwise the shared-choice probability is 82%. Brief individual excursions
track a real agent object until the next irregular change or until it disappears.

Each actor changes attention after a separately sampled 1.1–4.6 seconds,
with different smoothing rates and occasional small chin changes. Discrete
left/center/right and lower-rink poses express that continuous attention point.
Conversational partner glances remain asymmetric and occasionally unanswered.
This is coarse gaze animation, not continuously articulated pupils. These
observers never feed information into agent decisions or the evolution model.

Ordinary selection and 90% survival no longer trigger a large gesture. The
first 95% result per team, first perfect result, and first generation wipeout
per team produce appropriate commentary. Standing accompanies achievement;
the table reaction accompanies a wipeout. The reaction cue is emitted with
its spoken-text line. A 25-second cooldown suppresses excess gestures rather
than queuing a stale response. A generation wipeout rebuilds the population;
it does not end the match. Additional quiet lines describe concentration,
following an individual, or inherited directions without constant gesturing.

## Art and character direction

The user now approves the friendly simplified style. Keep Gerard warm, serene,
eccentric and communicative, with specific enthusiasms. Proposed lore: a divorced
multimillionaire looking for the next young prodigy of evolution. An autistic /
neurodivergent characterization was proposed; develop him through specific
interests and individual behavior, not a diagnostic caricature. Do not return
to the intimidating McAfee-inspired version. Detailed lore is still to come.
All earlier art is retained, including v34 and the v35/v36 sources.

New gaze illustrations were created with the built-in image generator. Exact
prompt: `broadcast/generation.json`. Local Sharp crops align the heads over
the existing seated bodies. Rebuild with
`node art-direction/v37/broadcast/build-runtime.mjs`. Runtime asset:
`prototype/assets/broadcast-v37/rink.png`. Captures: `captures/`.

Eight registered blink variants close the eyes independently without changing
the mouth or seated body. Their separate generation prompt is preserved in
`broadcast/blink-generation.json`; the preparation script transfers only the
glasses/eyelid band from that source.

## Verification

The first-level playtest checks the new deck; `DirectionAndAttentionChecks`
also verifies position/heading/target independence, fixed strength, uniform
initial angles, inherited and wrapped mutations, measured concentration,
following a moving cluster, and exact 90%/95%/wipeout dialogue/gesture fixtures.
The broadcast checks retain the delayed partner response and pixel continuity
checks. The duel audit verifies the same force and decision cadence on both
sides after twelve generations. No results are forced outside test fixtures.

Results are recorded in `verification.json`: 40-generation playtest passed,
29 direction/attention checks and 62 final scheduling/pixel checks passed.
The long playtest emitted an ObjectDB cleanup warning on exit; it is recorded
separately from the successful checks and remains uninvestigated.
