# Current state — 2026-11-07

## Playable baseline

`prototype/project.godot` is the game to continue. Its main scene is `Main.tscn`;
presentation and level-one orchestration are in `prototype/scripts/presentation/`.
The root `project.godot` remains a separate older sandbox; do not confuse them.

The current v52 slice has Nico, two animated commentators, timed preparation,
Turn / Thrust / Direction allocation, two teams under shared physics, survival
charts, the fixed-dot LED marquee, reaction animations and compact active cards.
Source assets and gameplay are preserved by the cleanup. No new balancing or
art integration was performed as part of that cleanup.

## Approved visual direction

Gerard is a warm, eccentric, divorced tech multimillionaire hosting Evolution Wars
in a practical broadcast hangar on a Pacific island. Contestants heard his online
invitation and found their way there. He is looking for an evolution prodigy.
His name is spelled **Gerard**, without an accent. Rene / Le Chevalier is his
more reserved colleague. Keep faces, head size and visible pixel scale consistent.

Keep the compact composition: dominant arena, player columns, overhead press
gallery, sprites in front of a continuous dark hangar, and a restrained vertical
fascia masking the base of their hands. Do not stretch rivets or circular corners.
Speech bubbles fit their text, have no speaker name and disappear after reading.

The latest choice is **lighting option 02**:

- Flush, closely joined wooden sign boards painted matte black, tiny seams.
- Standard upright stencil **EVOLUTION** painted off-white.
- **WARS** formed by physical red-orange neon tubes, with restrained local spill.
- Thin black casing around the fixed-dot ticker, plausible beams and worklights.
- Avoid rustic board gaps, arbitrary ropes, decorative rivets, cinematic movie
  typography and a bright nightclub wash.

Canonical review: `art-direction/studio-approved/neon-context.png`.
Background and black-sign source images and the selected generation prompt are
beside it. The context image is a generated mockup, not a game screenshot.
The small neon detail is a reference crop, not a finished transparent sprite.

## Opponents and entrances

**Level 1: Nico Valdes, Havana, Cuba.** Odd, friendly, relaxed, thinning hair,
glasses, moustache, mustard pocket tee. Keep his approachable awkwardness.
Runtime has his portrait and win/loss reactions. The approved introduction is a
glasses adjustment and a shy open-hand wave, not a peace sign.

**Level 2: Nao Morita, age 20, Fukuoka, Japan.** Software developer and part-time
stadium dancer. Approved look: simple brown hair, navy cropped tee, white shorts.
Natural proportions and restrained movement. The selected entrance is **double
peace signs with a slight head tilt, then hands behind her back and a simple happy
smile**. No cat paws, mouth-covering, giggle sequence or exaggerated hip motion.

Both opponents retain two endings: looking right/down toward the rink, or looking
at the camera with a friendly expression. No global orientation has been chosen.
Review all four at `art-direction/opponent-endings-v01/index.html`.
These entrances and level two are **not integrated into runtime**. Older dance
cells remain in Nao's retained source atlas for provenance, but the comparison
gallery plays only the approved finale. Do not use the old full dance by default.

## Next useful work

1. Integrate the selected hangar/sign using proper layers and native proportions;
   preserve runtime characters and arena, rather than using the mockup as a UI.
2. Integrate opponent entrances once their placement and orientation are decided.
3. Continue the first-level movement/card design; see FIRST_LEVEL_DIRECTION.md.
4. Define level two's gameplay before treating its character art as a playable level.

## Cleanup policy

Rejected logo studies, superseded choreography, duplicate frame exports, old
runtime screenshots and launch logs were removed. Current runtime assets, source
code, tests, localization, font licenses, useful production sources and both
approved opponent endings remain. Earlier committed experiments are recoverable
from Git history at `cf310fd`; rejected untracked drafts are not in that history.
No Git history rewrite was performed, so this reduces the working tree, not the
size of historical Git objects or existing clones.

Use the small current documents instead of reconstructing decisions from old
version-numbered galleries. Historical generation metadata is provenance, not
an instruction to restore superseded art.
