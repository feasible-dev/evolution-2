// Pack the generated artwork without redrawing or morphing the character.
const sharp = require('../../.cache/sprite-tools/node_modules/sharp');
const fs = require('node:fs/promises');
const path = require('node:path');
const root = __dirname;
const width = 128, height = 192;
const sequence = [
  [0,850],[1,150],[2,180],
  [3,190],[4,140],[3,140],[4,160],[5,160],
  [6,300],[5,140],[6,250],[5,140],
  [7,150],[8,180],[9,200],[8,180],[9,200],
  [10,180],[1,150],[0,1000]
];
const poseNames = ['Ready','Anticipation','Paws up','Left cheek','Left paw pulse',
  'Center','Right cheek','Hip sway','Left paw forward','Right paw forward','Settle','Ready'];
async function main(){
  const input=path.join(root,'nao-paws-grid.png');
  const meta=await sharp(input).metadata();
  if(meta.width!==965 || meta.height!==1630)throw Error('Unexpected atlas geometry');
  await fs.mkdir(path.join(root,'frames'),{recursive:true});
  const frames=[],crops=[];
  const baselines=[529,1041,1548];
  for(let i=0;i<12;i++){
    const row=Math.floor(i/4),col=i%4;
    const left=Math.round(col*965/4),right=Math.round((col+1)*965/4);
    const crop={left,top:baselines[row]-480,width:right-left,height:427};
    crops.push(crop);
    const tile=await sharp(input).extract(crop).resize(108,height,{kernel:'nearest',fit:'fill'}).png().toBuffer();
    let frame=await sharp({create:{width,height,channels:4,background:'#101827'}})
      .composite([{input:tile,left:10,top:0}]).png().toBuffer();
    // Keep the approved rest pose exactly, including its face pixels.
    if(i===0 || i===11)frame=await fs.readFile(path.join(root,'../level-2-v03/frames/00.png'));
    frames.push(frame);
    await fs.writeFile(path.join(root,'frames',`${String(i).padStart(2,'0')}.png`),frame);
  }
  await sharp({create:{width:512,height:576,channels:4,background:'#101827'}})
    .composite(frames.map((input,i)=>({input,left:i%4*width,top:Math.floor(i/4)*height})))
    .png().toFile(path.join(root,'nao-paws-native.png'));
  const animated=[];
  for(const [i] of sequence)animated.push(await sharp(frames[i]).resize(256,384,{kernel:'nearest'}).ensureAlpha().raw().toBuffer());
  await sharp(Buffer.concat(animated),{raw:{width:256,height:384*sequence.length,channels:4,pageHeight:384}})
    .gif({delay:sequence.map(s=>s[1]),loop:0,dither:0,effort:7})
    .toFile(path.join(root,'nao-paws-preview.gif'));
  const manifest={status:'Choreography preview; not installed in game',generator:'Built-in imagegen',
    approvedStyle:'../level-2-v03/nao-celebration-grid.png',source:'nao-paws-grid.png',
    nativeFrame:{width,height},grid:{columns:4,rows:3},crops,scale:0.45,
    poseNames,sequence:sequence.map(([frame,durationMs])=>({frame,durationMs})),
    notes:['Same approved palette, clothing and identity.',
      'Human cat-paw hand gestures, cheek-to-cheek movement, hip shifts and counter-rotating shoulders.',
      'First and last playback frames use the exact approved v03 neutral image.',
      'Opaque review background; no gameplay code changed.']};
  await fs.writeFile(path.join(root,'animation.json'),JSON.stringify(manifest,null,2)+'\n');
  let html=await fs.readFile(path.join(root,'../level-2-v03/index.html'),'utf8');
  html=html.replaceAll('nao-celebration-native.png','nao-paws-native.png')
    .replaceAll('nao-celebration-preview.gif','nao-paws-preview.gif')
    .replace('Nao motion study','Nao cat-paw dance')
    .replace('Nao — victory dance','Nao — cat-paw dance')
    .replace('Motion study · fictional adult character · simple stadium outfit','Approved style and colors · new cat-paw choreography')
    .replace('max="17"','max="19"')
    .replace(/const sequence=\[.*?\];/,`const sequence=${JSON.stringify(sequence)};`)
    .replace(/const names=\[.*?\];/,`const names=${JSON.stringify(poseNames)};`)
    .replace('A short anticipation, raised fists, side-to-side weight shifts, one arm overhead, then a settled return. The first and last playback frames use the same pixels. The generated hand-wave poses are excluded because their arm continuity is inconsistent.',
      'Bent wrists and softly curled fingers, paws beside the left cheek then the right, hip sways with opposing shoulder movement, alternating paw reaches, then a settled return. Same approved neutral pixels at both ends.')
    .replace('Twelve generated key poses, including unused wave poses','Twelve cat-paw key poses')
    .replace('<h2>Native pixel comparison</h2>', '<h2>Previous dance</h2><p>The approved style is unchanged. Compare the first celebration with the new choreography above.</p><img src="../level-2-v03/nao-celebration-preview.gif" width="128" height="192" style="image-rendering:pixelated" alt="Previous celebration"><h2>Native pixel comparison</h2>')
    .replace('Initial imagegen prompt','Imagegen prompt')
    .replace(' · <a href="prompt-refinement.txt">Refinement prompt</a>','');
  await fs.writeFile(path.join(root,'index.html'),html);
  const result=await sharp(path.join(root,'nao-paws-preview.gif'),{animated:true}).metadata();
  if(result.pages!==sequence.length || result.pageHeight!==384)throw Error('Invalid animated export');
  console.log(JSON.stringify({frames:frames.length,playbackFrames:result.pages,durationMs:sequence.reduce((t,s)=>t+s[1],0),native:`${width}x${height}`}));
}
main().catch(e=>{console.error(e);process.exitCode=1});
