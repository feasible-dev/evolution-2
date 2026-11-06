# v45 — Shared portrait scale and matrix performances

Nico's approved portrait and reactions already run in the game, not just the
art gallery. His square texture is 128x128; each presenter texture is 192x128.
All are displayed at one native pixel per logical UI pixel with nearest
filtering. Window scaling is shared by the whole canvas. Native pixels are
shown side by side in `captures/portrait-native-pixels.png`. The v44 GIFs used
a 3x review enlargement. Different portrait crops still produce different face
sizes; equal pixel pitch does not mean identical head dimensions.

The interest/encouragement and arena-facing Nico poses are deferred, as asked.

The matrix now supports four explicit motion routines: continuous leftward
scroll, still centered text, enter/stop/three flashes/resume, and a non-letter
burst followed by still text. Bold is the default; regular weight remains
available for comparison. Weight adds one neighboring lit column, preserving
the glyph cell spacing and the physical dot grid.

Short result/survivor/audience messages stop and flash. The clock can remain
still, while energy messages get a brief orange/blue burst. Long messages
always fall back to scrolling. Live statistic refresh preserves cue age.
The three pulses happen only within the strip, at 0.6-second intervals; no
additional full-screen flashing is introduced. Existing red result impacts
and cooldown remain in place.

`MarqueePerformance.cs` owns deterministic timing; `MarqueeProgram.cs` selects
routines and `TickerStrip.cs` passes the sampled state to the shader. The
shader changes bulb colors only, including the geometric burst rays.

`index.html` includes an animated excerpt made from actual Godot captures,
plus regular/bold comparison images. `build-preview.mjs` assembles the excerpt
using the existing Sharp/Pixelbolt art tooling. These tools are not required
to run the game.
