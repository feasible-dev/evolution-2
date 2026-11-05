# Gerard table slap experiment

Open index.html locally to compare both variants with pause, replay and frame selection.

## Tools actually used

- Built-in image generation produced generated-sheet.png using
  prototype/assets/gerard-mccautry-v13b.png as the character reference.
- Sharp 0.34.5 sliced the 4 by 2 image using rounded cell boundaries, resized with
  nearest-neighbor filtering, thresholded alpha and created a shared palette.
- Pixelbolt 0.11.0 imported both sheets into editable recipes, generated GIF
  previews, and exported transparent PNG atlases, APNG animations and Godot
  SpriteFrames resources. Both exports validated with 8 frames and transparency.
- PixelLab and SpriteCook were NOT tested: plugin search returned no available
  matches and the browser host was unavailable. A service/account connection
  remains necessary. Do not interpret these images as output from those services.

## Generation specification

Preserve Gerard's front-facing identity, headphones, microphone, dark beard,
blue lemon-and-flower shirt and dark tee from the original atlas. Generate eight
aligned transparent poses, ordered in a 4-column, 2-row sheet: neutral, prepare,
lift the right open hand (viewer left), anticipation near the ear, palm impact on
the imaginary tabletop, hold, recover, neutral. Keep the left forearm stable,
camera fixed, waist anchor consistent and the desk outside the generated image.
Use clean pixel clusters and a coherent limited palette. No text or borders.

The generator delivered 1774 by 887 pixels rather than exact divisible cells.
The preprocessing uses rounded boundaries. The eighth generated pose is replaced
with the exact first pose to ensure a matching neutral endpoint.

## Assessment

The raised hand and open-palm impact read as a gesture. Shirt motifs, hair and
torso still vary between poses. The impact would benefit from a better shoulder
and torso accent. This is a tool and style experiment, not production animation.
The square composition differs from the existing 192 by 128 seated viewport;
adapting the framing is required before game integration.

The 168-pixel version has pixels about 14 percent larger than the 192-pixel
version when displayed at equal size. This comparison is between the two NEW
versions, not an exact before-and-after comparison with the current game.

Rebuild from repository root:

```powershell
npm.cmd install --prefix .cache/sprite-tools --ignore-scripts --no-audit --no-fund pixelbolt@0.11.0 sharp@0.34.5
node art-direction/v30/gerard-slap/build-preview.mjs
node .cache/sprite-tools/node_modules/pixelbolt/bin/pixelbolt.js render art-direction/v30/gerard-slap/recipe-192.json --out art-direction/v30/gerard-slap/export-192 --force
node .cache/sprite-tools/node_modules/pixelbolt/bin/pixelbolt.js render art-direction/v30/gerard-slap/recipe-168.json --out art-direction/v30/gerard-slap/export-168 --force
```
