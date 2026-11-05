gerard-slap-168 — Pixelbolt export

gerard-slap-168.png: transparent RGBA sprite sheet
gerard-slap-168.atlas.json: TexturePacker-style frames, named animation sequences and Aseprite-style frame tags
gerard-slap-168.tres: Godot 4 SpriteFrames; it finds gerard-slap-168.png in its own folder, so keep the two together
gerard-slap-168.css: CSS classes with per-frame timing
frames/: individual PNGs
animations/: animated PNGs (APNG)
gerard-slap-168.pixel.json: editable source
player.js: dependency-free Canvas player
preview.html: open directly in a browser, or serve this folder

Canvas: import { SpritePlayer } from './player.js';
const player = await SpritePlayer.load(canvas, './gerard-slap-168.atlas.json');
player.play('slap');

Godot 4: copy gerard-slap-168.png and gerard-slap-168.tres into one folder of your project, give an AnimatedSprite2D the Sprite Frames gerard-slap-168.tres and play("slap"). Set the node's Texture Filter to Nearest for crisp pixels.

CSS: <link rel="stylesheet" href="gerard-slap-168.css">
<span class="pf-gerard-slap-168 pf-gerard-slap-168-slap"></span>

Timing:
slap: 8 frames of 70 to 900 ms, 2340 ms in all, plays once
review: 8 frames of 70 to 900 ms, 2340 ms in all
