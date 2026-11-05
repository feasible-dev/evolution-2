gerard-talk — Pixelbolt export

gerard-talk.png: transparent RGBA sprite sheet
gerard-talk.atlas.json: TexturePacker-style frames, named animation sequences and Aseprite-style frame tags
gerard-talk.tres: Godot 4 SpriteFrames; it finds gerard-talk.png in its own folder, so keep the two together
gerard-talk.css: CSS classes with per-frame timing
frames/: individual PNGs
animations/: animated PNGs (APNG)
gerard-talk.pixel.json: editable source
player.js: dependency-free Canvas player
preview.html: open directly in a browser, or serve this folder

Canvas: import { SpritePlayer } from './player.js';
const player = await SpritePlayer.load(canvas, './gerard-talk.atlas.json');
player.play('talk');

Godot 4: copy gerard-talk.png and gerard-talk.tres into one folder of your project, give an AnimatedSprite2D the Sprite Frames gerard-talk.tres and play("talk"). Set the node's Texture Filter to Nearest for crisp pixels.

CSS: <link rel="stylesheet" href="gerard-talk.css">
<span class="pf-gerard-talk pf-gerard-talk-talk"></span>

Timing:
talk: 7 frames of 130 to 300 ms, 1420 ms in all, plays once
