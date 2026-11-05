gerard-slap — Pixelbolt export

gerard-slap.png: transparent RGBA sprite sheet
gerard-slap.atlas.json: TexturePacker-style frames, named animation sequences and Aseprite-style frame tags
gerard-slap.tres: Godot 4 SpriteFrames; it finds gerard-slap.png in its own folder, so keep the two together
gerard-slap.css: CSS classes with per-frame timing
frames/: individual PNGs
animations/: animated PNGs (APNG)
gerard-slap.pixel.json: editable source
player.js: dependency-free Canvas player
preview.html: open directly in a browser, or serve this folder

Canvas: import { SpritePlayer } from './player.js';
const player = await SpritePlayer.load(canvas, './gerard-slap.atlas.json');
player.play('slap');

Godot 4: copy gerard-slap.png and gerard-slap.tres into one folder of your project, give an AnimatedSprite2D the Sprite Frames gerard-slap.tres and play("slap"). Set the node's Texture Filter to Nearest for crisp pixels.

CSS: <link rel="stylesheet" href="gerard-slap.css">
<span class="pf-gerard-slap pf-gerard-slap-slap"></span>

Timing:
slap: 10 frames of 90 to 950 ms, 3600 ms in all, plays once
