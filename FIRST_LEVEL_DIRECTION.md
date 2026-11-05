# First level direction

Working design notes, November 4 2026. Preserve the wider card discussion in
MUTATION_CARD_DESIGN.md while focusing on the first playable level. These notes
do not change gameplay or approve all proposed content for implementation.

## Requested direction

- Improve movement in the opening level; clarify agent motion versus character
  animation before choosing the next implementation pass.
- Eventually give Gerard a standing, raised-arms reaction.
- Prepare more opportunities for humorous remarks and serious match analysis.
- Preferred first opponent: a software developer from Shenzhen,
  now unemployed and entering the game. Lin is the earlier working name, not a
  confirmed final identity.
- The user selected Evolution Wars as the game title on November 4 2026.

## Broadcast events to explore

The current LevelOneSession has commentary for introduction, start, selection
reveal, ten-generation milestones and first perfect survival. Extend coverage
using measured events rather than periodic generic remarks.

| Event | Serious commentary purpose | Character reaction proposal |
| --- | --- | --- |
| First selection | Explain the observed survival rule | Gerard surprise, Rene explanation |
| First survivor after extinction | Recognize recovery | Small relieved gesture |
| New best survival | Describe improvement | Lean forward; reserve celebration for substantial gains |
| Sustained lead change | Compare completed-generation results | Gerard turns to Rene |
| Large improvement over the previous result | Describe the actual gain | Brief excited gesture |
| Sustained plateau | Explain that progress has stalled | Dry exchange; no claim about cause without evidence |
| Several agents arrive and then leave the refuge | Explain overshooting | Requires per-agent region-entry tracking |
| Near-perfect result | Identify the remaining gap | Anticipation, without declaring victory |
| First perfect survival | Celebrate the measured achievement | Gerard stands and raises both arms, then settles |
| Both teams reach perfection together | Recognize a tie | Shared reaction; avoid announcing a winner |

Thresholds are not fixed yet. Compare both teams at the same completed generation.
Do not confuse temporary region occupancy with final survival. If the deck can
produce identical populations, acknowledge equal results honestly.

Use one speaker at a time, real-time reading durations, event priority,
per-event cooldowns and varied lines. Expire stale commentary when newer results
make it misleading. Lead changes need a margin or persistence criterion to avoid
repeated announcements around a tie. Important reactions should interrupt only
at a readable boundary, with a partner response when useful.

## Character animation

Standing and raised arms require coherent full-body poses, not an enlarged facial
patch or a shifted elbow. Plan anticipation, rising, held celebration and a return
to the seated idle. Keep desk, scale, character identity and camera consistent.
Keep the broadcast frame fixed: when Gerard stands, show his torso and outfit
while his head and eventually his neck leave the top of the frame. This clipping
is intentional comic staging, not a reason to shrink the character or arena.
Additional requested gesture ideas include table slaps and agitated reactions.
Slightly stronger pixelation is an option to evaluate, not an approved art change.
No new sprite asset is generated or approved by this note.

November 4 follow-up: the user prefers the existing built-in image generator
and local sprite software, without external generation accounts or API keys.
Pixelbolt was tested locally in art-direction/v30/gerard-slap; it assembles
sprites and exports animation, but does not generate poses. Built-in generation
created the eight poses in one sheet, not anatomically computed interpolation.

Gerard's next style should show a lived-in 45-to-50-year-old man: expressive
creases, weary eyelids, asymmetry and mischievous charm. The user referenced a
reckless, weathered temperament, explicitly not a copy of a real person's face.
Remove blue from eye whites and use neutral brown/gray facial hair. Hands and
arm proportions remain quality concerns. The style study in art-direction/v31
explores skepticism, joking and laughter; it is not an approved final design
or a sequence of animation in-betweens.

The user approved the v31 Gerard face and requested the spelling Gerard without
an accent. Hands should be angular and adult-sized, with defined finger joints
and subtle hair. Requested animation stock now includes standing with raised
arms while Le Chevalier tracks his face upward, mutual glances and other partner
reactions. The first three timed studies are in art-direction/v32/duo-scenes.
They are review assets, not replacements for the runtime portrait system yet.

Latest art feedback supersedes the earlier approval of the v31 face: avoid
crooked/exaggerated teeth, threatening or leering expressions and the earlier
McAfee-inspired temperament. Gerard should remain around 45–50, with lived-in
features and wrinkles, but read as a warm, eccentric, expressive geek with a
good-natured smile. Do not treat neurodivergence as a visual diagnostic or
caricature. Five unselected character directions are saved in art-direction/v33.
Existing animation studies are retained; no new face is applied to them yet.

The user then selected proposal 3 from v33: clean-shaven, round glasses, wavy
brown hair, long expressive face and warm eccentric geek personality. The new
v34 library regenerates standing, table slap and partner glances with this face,
and adds quiet watching/blinking and talking. During table slap and rising,
Gerard watches the rink BELOW the commentators rather than performing toward
the camera. Le Chevalier follows Gerard's face upward during standing. Mutual
sideways eye contact is reserved for their deliberate exchanges. v34 is the
current review library; v32 is superseded but retained.

## First opponent proposal

Retain Lin as a placeholder. Her requested background is a software developer in
Shenzhen who lost her job and enters the competition. Proposed personality:
practical, observant, quietly competitive, with dry humor about prototypes,
deadlines and corporate promises. Her full name and personal stakes
remain open. Avoid making her nationality or job loss the recurring punchline.

Her cards and tactics must obey the same costs, physical limits and information
rules as the player. No hidden speed advantage to manufacture personality.

## Game title

Evolution Wars is the selected title. No availability, trademark or commercial
clearance research has been performed.

## Animation tools to evaluate before producing the opponent

Documentation reviewed November 4 2026; none of these services has been tested
on Gerard or connected to the project yet.

- PixelLab: https://www.pixellab.ai/mcp — agent tools for image generation,
  editing and animation, including animation from an existing image.
- SpriteCook: https://www.spritecook.ai/docs/guide-animate-sprite — custom
  motion from a pixel sprite; documented source limit is 256 by 256 pixels.
  Agent connection: https://www.spritecook.ai/blog/ai-agent-generate-sprites
- Sprite Fusion: https://www.spritefusion.com/docs/pixel-art-generator/api/mcp
  — agent tools for generation, editing, style references and animation.
- Aseprite automation: https://github.com/aigengame/aseprite-automation —
  editable sprite workflows through CLI or MCP; requires Aseprite separately.

Proposed evaluation: use the same Gerard reference and one short table-slap
gesture to compare identity preservation, stable torso/outfit, hand contact,
background transparency and timing. Then test the harder sit-to-stand gesture.
Keep the desk separate and fixed, and retain the same neutral start/end pose.
Evaluate modest pixel-density changes at actual game display size before
reworking every animation. Tool support is not proof of motion quality.

## v35 animation refinement — November 4, 2026

The user asked for less facial pixel noise, restrained eye whites, a delayed
reaction from Le Chevalier during Gerard's rise, a longer standing hold with
small arm movements, and astonishment rather than a grin during the table slap.

`art-direction/v35/duo-scenes/index.html` is the current review gallery, replacing
v34 for these studies. Four new eight-pose source sheets use broad color clusters.
Basic speech locks the facial contours and changes only the mouth area; watching
uses a small eye region. Independent actor pose tracks produce a 490 ms delayed
response during standing and a 3.74-second standing hold. The event uses 18 timed
composite frames, not 18 separately drawn anatomical inbetweens. The table slap
uses ten timed frames and a delayed partner reaction. Assets, exact prompts,
timelines, GIFs and separate Godot SpriteFrames are saved in the v35 folder.

This remains an asset review pass, not a runtime integration. Minor costume and
posture shifts remain in large gestures; keep these visible limitations in mind
when choosing the final animation pipeline. Earlier versions remain preserved.

## v36 playable trial and art direction still open — November 4, 2026

The user wants to judge the simplified v35 animation inside the first level.
It is now the default runtime treatment, with additional Le Chevalier speaking
poses and independent, asymmetric conversational glances in both directions.
A speaker can glance without a response; the listener can initiate; a delayed
reply can outlast the initiating glance. Match results trigger the table slap
and standing gestures, with a real-time cooldown and no forced changes to match
results. Restart clears the performance state.

Do not treat the simplified face as final approval. The user finds v35 very
simplified and may prefer a middle ground with v34, restoring a little facial
texture after viewing it small in the game. Preserve v34 and v35 sources and
galleries. Evaluate any texture return at actual display size and in motion;
keep stable color clusters and avoid scattered animated wrinkle pixels.

## v37 — Approved style, active attention and Direction

The latest user feedback approves the friendly simplified Gerard style. This
supersedes the v36 uncertainty: keep this style for now, while preserving older
assets. Gerard is a serene, eccentric, warm divorced multimillionaire seeking
the next young evolution prodigy. The user proposed autistic/neurodivergent
traits with good communication; detailed lore remains future work. Avoid the
earlier threatening McAfee direction.

The first level now offers only action cards: Turn, Thrust, Direction. Direction
is an inherited absolute angle with circular mutation and no target or region
perception. Both teams use the same strengths and gene pool. Observation/action
combinations will be introduced later rather than hiding them inside this card.

Presentation attention now observes actual concentrations and sometimes follows
one agent, with independent irregular timing and mostly shared attention.
Reserve large gestures for first 95%, first 100%, and generation wipeouts; emit
the reaction with its commentary and suppress stale or excessive gestures.
Ordinary rounds may remain seated throughout. See `art-direction/v37/README.md`.

## v38 — Refined unified broadcast style

The user's latest preference supersedes the v37 simplified-style approval:
use the finer earlier portrait treatment consistently across static and moving
poses. Keep the same warm, approachable character, without returning to the
McAfee direction. v38 regenerates conversation, gaze, blink, standing and slap
sources from the refined v34 reference and uses a shared palette/master pose.
Eyes normally remain open under their lids; brief blinks are distinct states.
The arena ticker now has an original colorful pixel alphabet with French
accents, right-to-left movement and occasional letter-size/highlight waves.
See `art-direction/v38/README.md`; preserve all earlier source versions.

## v39 — Comfortable posture and sustained excitement

Gerard's resting neck should be upright, not perpetually tilted sideways.
After a rare standing reaction he gets comfortable again but remains animated:
shifting toward the rink on either side, speaking excitedly and inviting René
into the moment. The colleague responds after a delay, then Gerard calms down.
Use real milestone dialogue, not random celebrations.

The scrolling banner is a physical-looking fixed grid of colored light dots
on black. Letters move by switching bulbs on/off and changing their colors;
bulbs themselves must never translate or grow with a glyph. This supersedes
the v38 moving glyph rectangles. See `art-direction/v39/README.md`.

## v40 — One stadium band, restricted colors

Use one uninterrupted LED matrix instead of two messages separated by a score.
Keep detailed survival on the sides; rotate current statistics and broadcast
messages through the band. Orange and blue dominate, sometimes monochrome.
Use little red: real exceptional results may briefly turn the entire screen
red and produce bold, enlarged headlines on the same stationary bulbs.
Build separate routines for those broadcast moments. Gradually remove the
decorative fine blue rules and framed-panel aesthetic, retaining meaningful
arena boundaries and graph axes. See `art-direction/v40/README.md`.
