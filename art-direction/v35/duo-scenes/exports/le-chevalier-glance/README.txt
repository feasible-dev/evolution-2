le-chevalier-glance — Pixelbolt export

le-chevalier-glance.png: transparent RGBA sprite sheet
le-chevalier-glance.atlas.json: TexturePacker-style frames, named animation sequences and Aseprite-style frame tags
le-chevalier-glance.tres: Godot 4 SpriteFrames; it finds le-chevalier-glance.png in its own folder, so keep the two together
le-chevalier-glance.css: CSS classes with per-frame timing
frames/: individual PNGs
animations/: animated PNGs (APNG)
le-chevalier-glance.pixel.json: editable source
player.js: dependency-free Canvas player
preview.html: open directly in a browser, or serve this folder

Canvas: import { SpritePlayer } from './player.js';
const player = await SpritePlayer.load(canvas, './le-chevalier-glance.atlas.json');
player.play('glance');

Godot 4: copy le-chevalier-glance.png and le-chevalier-glance.tres into one folder of your project, give an AnimatedSprite2D the Sprite Frames le-chevalier-glance.tres and play("glance"). Set the node's Texture Filter to Nearest for crisp pixels.

CSS: <link rel="stylesheet" href="le-chevalier-glance.css">
<span class="pf-le-chevalier-glance pf-le-chevalier-glance-glance"></span>

Timing:
glance: 9 frames of 180 to 900 ms, 4410 ms in all, plays once
