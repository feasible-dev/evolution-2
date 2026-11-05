gerard-glance — Pixelbolt export

gerard-glance.png: transparent RGBA sprite sheet
gerard-glance.atlas.json: TexturePacker-style frames, named animation sequences and Aseprite-style frame tags
gerard-glance.tres: Godot 4 SpriteFrames; it finds gerard-glance.png in its own folder, so keep the two together
gerard-glance.css: CSS classes with per-frame timing
frames/: individual PNGs
animations/: animated PNGs (APNG)
gerard-glance.pixel.json: editable source
player.js: dependency-free Canvas player
preview.html: open directly in a browser, or serve this folder

Canvas: import { SpritePlayer } from './player.js';
const player = await SpritePlayer.load(canvas, './gerard-glance.atlas.json');
player.play('glance');

Godot 4: copy gerard-glance.png and gerard-glance.tres into one folder of your project, give an AnimatedSprite2D the Sprite Frames gerard-glance.tres and play("glance"). Set the node's Texture Filter to Nearest for crisp pixels.

CSS: <link rel="stylesheet" href="gerard-glance.css">
<span class="pf-gerard-glance pf-gerard-glance-glance"></span>

Timing:
glance: 7 frames of 200 to 700 ms, 3750 ms in all, plays once
