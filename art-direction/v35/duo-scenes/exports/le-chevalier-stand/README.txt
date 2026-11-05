le-chevalier-stand — Pixelbolt export

le-chevalier-stand.png: transparent RGBA sprite sheet
le-chevalier-stand.atlas.json: TexturePacker-style frames, named animation sequences and Aseprite-style frame tags
le-chevalier-stand.tres: Godot 4 SpriteFrames; it finds le-chevalier-stand.png in its own folder, so keep the two together
le-chevalier-stand.css: CSS classes with per-frame timing
frames/: individual PNGs
animations/: animated PNGs (APNG)
le-chevalier-stand.pixel.json: editable source
player.js: dependency-free Canvas player
preview.html: open directly in a browser, or serve this folder

Canvas: import { SpritePlayer } from './player.js';
const player = await SpritePlayer.load(canvas, './le-chevalier-stand.atlas.json');
player.play('stand');

Godot 4: copy le-chevalier-stand.png and le-chevalier-stand.tres into one folder of your project, give an AnimatedSprite2D the Sprite Frames le-chevalier-stand.tres and play("stand"). Set the node's Texture Filter to Nearest for crisp pixels.

CSS: <link rel="stylesheet" href="le-chevalier-stand.css">
<span class="pf-le-chevalier-stand pf-le-chevalier-stand-stand"></span>

Timing:
stand: 18 frames of 150 to 1000 ms, 6750 ms in all, plays once
