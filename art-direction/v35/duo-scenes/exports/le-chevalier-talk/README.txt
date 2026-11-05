le-chevalier-talk — Pixelbolt export

le-chevalier-talk.png: transparent RGBA sprite sheet
le-chevalier-talk.atlas.json: TexturePacker-style frames, named animation sequences and Aseprite-style frame tags
le-chevalier-talk.tres: Godot 4 SpriteFrames; it finds le-chevalier-talk.png in its own folder, so keep the two together
le-chevalier-talk.css: CSS classes with per-frame timing
frames/: individual PNGs
animations/: animated PNGs (APNG)
le-chevalier-talk.pixel.json: editable source
player.js: dependency-free Canvas player
preview.html: open directly in a browser, or serve this folder

Canvas: import { SpritePlayer } from './player.js';
const player = await SpritePlayer.load(canvas, './le-chevalier-talk.atlas.json');
player.play('talk');

Godot 4: copy le-chevalier-talk.png and le-chevalier-talk.tres into one folder of your project, give an AnimatedSprite2D the Sprite Frames le-chevalier-talk.tres and play("talk"). Set the node's Texture Filter to Nearest for crisp pixels.

CSS: <link rel="stylesheet" href="le-chevalier-talk.css">
<span class="pf-le-chevalier-talk pf-le-chevalier-talk-talk"></span>

Timing:
talk: 8 frames of 90 to 500 ms, 1770 ms in all
