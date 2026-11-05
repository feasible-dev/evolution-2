# Gerard proposal 3 animation library

The user selected proposal 3 from v33. This replaces the bearded v31/v32 identity:
Gerard now has an elongated clean-shaven face, round wire glasses, wavy brown
hair, wrinkles and a gentle eccentric geek personality. The cropped selected
portrait is retained as gerard-reference.png.

Open index.html for five animated sequences with pause, replay, slow playback
and frame selection. Every scene also has a directly playable GIF.

| Scene | Intended gaze and action |
| --- | --- |
| idle | Follow the rink below, blink, track play left and right |
| talk | Speak with small hand movement while following the rink |
| stand | Gerard reacts down toward the rink and stands; Le Chevalier tracks his rising face up-left |
| glance | Both briefly look at each other for an aside, then return to rink |
| slap | Gerard keeps his focus down toward rink through the table impact; Le Chevalier reacts |

## Exports and scope

Each scene has six illustrated poses plus a repeat of the exact starting pose.
GIF previews are 768 by 256 at 2x scale; prepared scene sheets use 384 by 128
per frame. Pixelbolt exports synchronized 192 by 128 actor-half resources in
exports/. Each half contains a baked background and desk, not a transparent
cutout. Start both actors on the same clock. Looping is appropriate for idle
and speech previews, while stand, glance and slap should be event-triggered.

The sources were generated with the built-in image tool, then aligned,
palette-reduced and exported locally using Sharp 0.34.5 and Pixelbolt 0.11.0.
No external generation account or API key was used. Prompts are in generation.json.

These are revised animation assets for review, not a runtime replacement yet.
There is no phoneme-driven lip sync. Small changes in clothing detail, posture
and framing remain between independently generated poses and clips. The down-rink
intent is visible in the lowered head and gaze, though at game resolution the
pupil direction behind the glasses is subtle. The standing peak intentionally
crops the head; the gaze cannot be inspected once it is offscreen.

Rebuild from the repository root with the v30 pinned tools installed:

```powershell
node art-direction/v34/duo-scenes/build.mjs
```
