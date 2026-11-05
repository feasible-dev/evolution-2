le-chevalier-idle — Pixelbolt export

le-chevalier-idle.png: transparent RGBA sprite sheet
le-chevalier-idle.atlas.json: TexturePacker-style frames, named animation sequences and Aseprite-style frame tags
le-chevalier-idle.tres: Godot 4 SpriteFrames; it finds le-chevalier-idle.png in its own folder, so keep the two together
le-chevalier-idle.css: CSS classes with per-frame timing
frames/: individual PNGs
animations/: animated PNGs (APNG)
le-chevalier-idle.pixel.json: editable source
player.js: dependency-free Canvas player
preview.html: open directly in a browser, or serve this folder

Canvas: import { SpritePlayer } from './player.js';
const player = await SpritePlayer.load(canvas, './le-chevalier-idle.atlas.json');
player.play('idle');

Godot 4: copy le-chevalier-idle.png and le-chevalier-idle.tres into one folder of your project, give an AnimatedSprite2D the Sprite Frames le-chevalier-idle.tres and play("idle"). Set the node's Texture Filter to Nearest for crisp pixels.

CSS: <link rel="stylesheet" href="le-chevalier-idle.css">
<span class="pf-le-chevalier-idle pf-le-chevalier-idle-idle"></span>

Timing:
idle: 6 frames of 110 to 1400 ms, 4810 ms in all
