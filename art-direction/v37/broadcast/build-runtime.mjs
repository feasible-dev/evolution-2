import fs from 'node:fs/promises';
import path from 'node:path';
import {fileURLToPath} from 'node:url';
import sharp from '../../../.cache/sprite-tools/node_modules/sharp/lib/index.js';
const dir=path.dirname(fileURLToPath(import.meta.url));
const out=path.resolve(dir,'../../../prototype/assets/broadcast-v37');
await fs.mkdir(out,{recursive:true});
const src=path.join(dir,'rink-gazes-source.png');
const m=await sharp(src).metadata(),w=m.width/2,h=m.height/4;
const frames=[];
for(let frame=0;frame<16;frame++){
 const i=frame%8;
 const crop={left:i%2*w+2,top:Math.floor(i/2)*h+2,width:w-4,height:h-4};
 let cell=await sharp(src).extract(crop).resize(384,128,{kernel:'nearest'}).png().toBuffer();
 if(frame>=8){
  const blink=await sharp(path.join(dir,'rink-blink-source.png')).extract(crop).resize(384,128,{kernel:'nearest'}).png().toBuffer();
  // Restrict generated blink changes to the glasses/eyelids; all other pixels stay registered.
  const bands=[{left:94,top:55,width:47,height:22},{left:250,top:55,width:49,height:22}];
  const overlays=[];
  for(const band of bands) overlays.push({input:await sharp(blink).extract(band).png().toBuffer(),left:band.left,top:band.top});
  cell=await sharp(cell).composite(overlays).png().toBuffer();
 }
 const actors=[];
 for(let actor=0;actor<2;actor++){
  const old=path.resolve(out,'../broadcast-v36/'+(actor===0?'gerard':'rene')+'-conversation.png');
  const base=await sharp(old).extract({left:0,top:0,width:192,height:128}).png().toBuffer();
  const head=actor===0?{left:60,top:3,width:100,height:91}:{left:192+40,top:3,width:100,height:91};
  const patch=await sharp(cell).extract(head).png().toBuffer();
  actors.push(await sharp(base).composite([{input:patch,left:actor===0?69:40,top:3}]).png().toBuffer());
 }
 frames.push(await sharp({create:{width:384,height:128,channels:4,background:'#081522'}}).composite(actors.map((input,j)=>({input,left:j*192,top:0}))).png().toBuffer());
}
await sharp({create:{width:384,height:frames.length*128,channels:4,background:'#081522'}}).composite(frames.map((input,i)=>({input,left:0,top:i*128}))).png({palette:true,colours:64,dither:0}).toFile(path.join(out,'rink.png'));
console.log('Exported eight rink gaze pairs and eight blink variants; existing bodies preserved.');
