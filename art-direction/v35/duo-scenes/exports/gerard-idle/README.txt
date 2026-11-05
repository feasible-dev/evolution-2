gerard-idle — Pixelbolt export

gerard-idle.png: transparent RGBA sprite sheet
gerard-idle.atlas.json: TexturePacker-style frames, named animation sequences and Aseprite-style frame tags
gerard-idle.tres: Godot 4 SpriteFrames; it finds gerard-idle.png in its own folder, so keep the two together
gerard-idle.css: CSS classes with per-frame timing
frames/: individual PNGs
animations/: animated PNGs (APNG)
gerard-idle.pixel.json: editable source
player.js: dependency-free Canvas player
preview.html: open directly in a browser, or serve this folder

Canvas: import { SpritePlayer } from './player.js';
const player = await SpritePlayer.load(canvas, './gerard-idle.atlas.json');
player.play('idle');

Godot 4: copy gerard-idle.png and gerard-idle.tres into one folder of your project, give an AnimatedSprite2D the Sprite Frames gerard-idle.tres and play("idle"). Set the node's Texture Filter to Nearest for crisp pixels.

CSS: <link rel="stylesheet" href="gerard-idle.css">
<span class="pf-gerard-idle pf-gerard-idle-idle"></span>

Timing:
idle: 6 frames of 110 to 1400 ms, 4810 ms in all
