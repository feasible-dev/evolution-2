le-chevalier-slap — Pixelbolt export

le-chevalier-slap.png: transparent RGBA sprite sheet
le-chevalier-slap.atlas.json: TexturePacker-style frames, named animation sequences and Aseprite-style frame tags
le-chevalier-slap.tres: Godot 4 SpriteFrames; it finds le-chevalier-slap.png in its own folder, so keep the two together
le-chevalier-slap.css: CSS classes with per-frame timing
frames/: individual PNGs
animations/: animated PNGs (APNG)
le-chevalier-slap.pixel.json: editable source
player.js: dependency-free Canvas player
preview.html: open directly in a browser, or serve this folder

Canvas: import { SpritePlayer } from './player.js';
const player = await SpritePlayer.load(canvas, './le-chevalier-slap.atlas.json');
player.play('slap');

Godot 4: copy le-chevalier-slap.png and le-chevalier-slap.tres into one folder of your project, give an AnimatedSprite2D the Sprite Frames le-chevalier-slap.tres and play("slap"). Set the node's Texture Filter to Nearest for crisp pixels.

CSS: <link rel="stylesheet" href="le-chevalier-slap.css">
<span class="pf-le-chevalier-slap pf-le-chevalier-slap-slap"></span>

Timing:
slap: 10 frames of 90 to 950 ms, 3600 ms in all, plays once
