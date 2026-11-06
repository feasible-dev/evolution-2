// Asset packing only. Artwork is generated with built-in imagegen.
const sharp = require('../../.cache/sprite-tools/node_modules/sharp');
const fs = require('node:fs/promises');
const path = require('node:path');
const root = __dirname;
const width = 128, height = 192;
const sequence = [
  [0, 1100], [1, 140], [2, 140], [3, 220],
  [4, 200], [5, 140], [6, 200], [5, 140],
  [4, 200], [5, 140], [3, 160], [7, 180], [8, 650],
  [7, 180], [3, 140], [2, 160], [1, 140], [0, 1200]
];
async function main() {
  const input = path.join(root, 'nao-celebration-grid.png');
  const metadata = await sharp(input).metadata();
  if (metadata.width !== 965 || metadata.height !== 1629) throw Error('Unexpected source geometry');
  await fs.mkdir(path.join(root, 'frames'), {recursive:true});
  const frames = [], crops = [];
  // Align the cut thigh baseline without scaling individual heads or bodies.
  const baselines = [529, 1041, 1582];
  for (let i = 0; i < 12; i++) {
    const col = i % 4, row = Math.floor(i / 4);
    const left = Math.round(col * 965 / 4);
    const right = Math.round((col + 1) * 965 / 4);
    const crop = {left, top: baselines[row] - 480, width:right-left, height:427};
    crops.push(crop);
    const tile = await sharp(input).extract(crop)
      .resize(108, height, {kernel:'nearest', fit:'fill'}).png().toBuffer();
    const frame = await sharp({create:{width,height,channels:4,background:'#101827'}})
      .composite([{input:tile,left:10,top:0}]).png().toBuffer();
    frames.push(frame);
    await fs.writeFile(path.join(root, 'frames', `${String(i).padStart(2,'0')}.png`), frame);
  }
  const atlas = await sharp({create:{width:width*4,height:height*3,channels:4,background:'#101827'}})
    .composite(frames.map((input,i)=>({input,left:(i%4)*width,top:Math.floor(i/4)*height})))
    .png().toBuffer();
  await fs.writeFile(path.join(root,'nao-celebration-native.png'),atlas);
  // Bookend with the very same neutral frame; exclude the generated wave
  // poses, which still switch hands. No crossfade, morphing or invented limbs.
  const raw = [];
  for (const [i] of sequence) raw.push(await sharp(frames[i]).resize(width*2,height*2,{kernel:'nearest'}).ensureAlpha().raw().toBuffer());
  await sharp(Buffer.concat(raw),{raw:{width:width*2,height:height*2*sequence.length,channels:4,pageHeight:height*2}})
    .gif({delay:sequence.map(x=>x[1]),loop:0,dither:0,effort:7})
    .toFile(path.join(root,'nao-celebration-preview.gif'));
  await fs.writeFile(path.join(root,'animation.json'),JSON.stringify({
    status:'Animation study, not installed in the game',
    generator:'Built-in imagegen', source:'nao-celebration-grid.png',
    nativeFrame:{width,height},grid:{columns:4,rows:3},crops,
    scale:0.45, sequence:sequence.map(([frame,durationMs])=>({frame,durationMs})),
    notes:['Uniform scale and aligned thigh baseline.',
      'Generated wave poses 9 and 10 excluded because they switch hands.',
      'Opaque review background; a final transparent sprite pass is still needed.',
      'First and last playback frames use identical neutral pixels.']
  },null,2)+'\n');
  console.log(JSON.stringify({frames:frames.length,playbackFrames:sequence.length,durationMs:sequence.reduce((n,x)=>n+x[1],0),native:`${width}x${height}`}));
}
main().catch(e=>{console.error(e);process.exitCode=1});
