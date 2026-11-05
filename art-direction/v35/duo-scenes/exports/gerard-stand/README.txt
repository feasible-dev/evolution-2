gerard-stand — Pixelbolt export

gerard-stand.png: transparent RGBA sprite sheet
gerard-stand.atlas.json: TexturePacker-style frames, named animation sequences and Aseprite-style frame tags
gerard-stand.tres: Godot 4 SpriteFrames; it finds gerard-stand.png in its own folder, so keep the two together
gerard-stand.css: CSS classes with per-frame timing
frames/: individual PNGs
animations/: animated PNGs (APNG)
gerard-stand.pixel.json: editable source
player.js: dependency-free Canvas player
preview.html: open directly in a browser, or serve this folder

Canvas: import { SpritePlayer } from './player.js';
const player = await SpritePlayer.load(canvas, './gerard-stand.atlas.json');
player.play('stand');

Godot 4: copy gerard-stand.png and gerard-stand.tres into one folder of your project, give an AnimatedSprite2D the Sprite Frames gerard-stand.tres and play("stand"). Set the node's Texture Filter to Nearest for crisp pixels.

CSS: <link rel="stylesheet" href="gerard-stand.css">
<span class="pf-gerard-stand pf-gerard-stand-stand"></span>

Timing:
stand: 18 frames of 150 to 1000 ms, 6750 ms in all, plays once
