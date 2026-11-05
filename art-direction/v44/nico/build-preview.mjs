import fs from 'node:fs/promises';
import path from 'node:path';
import {fileURLToPath} from 'node:url';
import sharp from '../../../.cache/sprite-tools/node_modules/sharp/lib/index.js';
import {encodeGIF} from '../../../.cache/sprite-tools/node_modules/pixelbolt/src/index.js';

const directory=path.dirname(fileURLToPath(import.meta.url));
const root=path.resolve(directory,'../../..');
const timing=JSON.parse(await fs.readFile(path.join(directory,'timing.json'),'utf8'));
const neutral=await sharp(path.join(directory,'../captures/nico-neutral.png')).ensureAlpha().raw().toBuffer();
// GIF has a 256-color limit. Quantization is only for this review export;
// the game still displays the original neutral texture without conversion.
const neutralGifPng=await sharp(neutral,{raw:{width:128,height:128,channels:4}}).png({palette:true,colours:256,dither:0}).toBuffer();
const neutralGif=await sharp(neutralGifPng).ensureAlpha().raw().toBuffer();
for(const clip of ['win','lose']){
 const frames=[];
 for(const beat of timing[clip]){
  const data=beat.frame===0||beat.frame===7?neutralGif:await sharp(path.join(root,'prototype/assets/opponents/nico-v44',clip+'.png'))
   .extract({left:0,top:beat.frame*128,width:128,height:128}).ensureAlpha().raw().toBuffer();
  frames.push({data,duration:Math.round(beat.seconds*1000)});
 }
 frames.push({data:neutralGif,duration:1800});
 await fs.writeFile(path.join(directory,clip+'.gif'),encodeGIF(frames,128,128,{loop:true,scale:3}).data);
}
const comparisons=[];
for(const [i,clip] of ['win','lose'].entries()){
 const atlas=path.join(root,'prototype/assets/opponents/nico-v44',clip+'.png');
 for(const [column,frame] of [0,1,3,4,6,7].entries()){
  const input=frame===0||frame===7?await sharp(neutral,{raw:{width:128,height:128,channels:4}}).png().toBuffer()
   :await sharp(atlas).extract({left:0,top:frame*128,width:128,height:128}).png().toBuffer();
  comparisons.push({input,left:column*128,top:i*128});
 }
}
await sharp({create:{width:768,height:256,channels:3,background:'#080f1c'}}).composite(comparisons).png().toFile(path.join(directory,'contact-sheet.png'));
console.log('GIF previews exported with exact runtime timing and neutral endpoints.');
