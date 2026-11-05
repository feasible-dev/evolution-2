# v40 — One continuous stadium marquee

The two LED strips and central score block are replaced by one full-width
matrix. Detailed survival charts and rates remain in the side panels. The
marquee cycles through six routines: latest generation survival, current
generation clock/state, both teams' energy, survivor counts, a short commentator
excerpt and the audience prompt. Statistical routines refresh once per second
without resetting horizontal scroll. The complete message gets time to pass
before the next routine; English and French strings are provided.

## Limited palette and effects

- Orange: `#ff941f`
- Blue: `#369dff`
- Red: `#f52824`, reserved for strong result alerts

Ordinary cues use orange alone, blue alone or alternating orange/blue words.
The earlier magenta, lime, violet and yellow ticker palette is no longer used.
The existing original pixel alphabet is reused.

First perfect survival per team and a generation with zero survivors issue a
red headline. A single red-screen envelope rises over 0.3 seconds, holds until
1.2 seconds, and fades completely by 3.8 seconds. A 20-second cooldown prevents
repeated full-screen bursts. The headline remains readable for 4.2 seconds,
then the ordinary program resumes. A 95% cue uses orange, not a red screen.
No result is invented for the game; the review command triggers a clearly
test-only preview after its normal captures.

Red headlines are centered, boldened and enlarged up to 38%, fitted to width.
These effects resample the letter mask across the stationary dots. Dot centers,
spacing and physical diameter do not move or grow. The matrix still scrolls
right-to-left at 28 logical pixels per second. A subtle light sweep provides
ordinary variation; no repeating full-screen strobe is used.

`MarqueeProgram.cs` owns routines, live refresh, outcome detection and cooldown.
`TickerStrip.cs` caches the current message raster. The LED shader renders the
fixed matrix in one draw. `BroadcastImpactOverlay.cs` applies a temporary
full-screen red color treatment and ignores all input.

## First visual cleanup

Removed the central caption/score dividers, ticker edge rules, the horizontal
rule beneath the commentators, bubble borders and the two tall side separators.
Plain background blocks now organize that region. Arena boundaries and chart
axes are retained because they describe the simulation/data. Other older blue
control outlines remain for a later pass, as requested gradually.

Portraits and motion remain v39. Simulation, cards, survival rules and speed
are unchanged. Previous assets and review galleries remain preserved.

## Validation and review

Build and Godot graphical review were run. Captures in `captures/` include the
continuous normal band, `14-red-impact.png`, and `15-settled.png`. The automated
checks cover six routines, no ordinary red floods, live refresh without scroll
restart, impact timing/cooldown/reset, absence of the central labels, and a
real simulated wipeout choosing the alert. Earlier animation and direction
checks also run. Detailed counts are in `verification.json`.

Branch: `feat/broadcast-marquee-v40`. No commit or push was made.
