import fs from 'node:fs/promises';
import path from 'node:path';
import {fileURLToPath} from 'node:url';
import sharp from '../../../.cache/sprite-tools/node_modules/sharp/lib/index.js';

const directory=path.dirname(fileURLToPath(import.meta.url));
const root=path.resolve(directory,'../../..');
const output=path.join(root,'prototype/assets/opponents/nico-v44');
await fs.mkdir(output,{recursive:true});
const frames=[];
for(const clip of ['win','lose']){
 const source=path.join(directory,clip+'-source.png');
 const metadata=await sharp(source).metadata();
 for(let i=0;i<8;i++){
  const left=Math.round(i%4*metadata.width/4),right=Math.round((i%4+1)*metadata.width/4);
  const top=Math.round(Math.floor(i/4)*metadata.height/2),bottom=Math.round((Math.floor(i/4)+1)*metadata.height/2);
  frames.push(await sharp(source).extract({left:left+1,top:top+1,width:right-left-2,height:bottom-top-2})
   .resize(128,128,{kernel:'lanczos3'}).removeAlpha().png().toBuffer());
 }
}
// One palette for both reactions; full-resolution sources stay untouched.
const atlas=await sharp({create:{width:128,height:2048,channels:3,background:'#080f1c'}})
 .composite(frames.map((input,i)=>({input,left:0,top:i*128})))
 .png({palette:true,colours:128,dither:0}).toBuffer();
for(const [i,clip] of ['win','lose'].entries()){
 await sharp(atlas).extract({left:0,top:i*1024,width:128,height:1024}).removeAlpha().png()
  .toFile(path.join(output,clip+'.png'));
}
// Read timing from the actual director so the browser preview matches the game.
const code=await fs.readFile(path.join(root,'prototype/scripts/presentation/OpponentReactionDirector.cs'),'utf8');
const timing={};
for(const clip of ['Win','Lose']){
 const beats=code.match(new RegExp(clip+'Beats\\s*=\\s*\\[([^\\]]+)\\]'))[1];
 timing[clip.toLowerCase()]=[...beats.matchAll(/new\((\d+),\s*([\d.]+)\)/g)].map(m=>({frame:Number(m[1]),seconds:Number(m[2])}));
}
await fs.writeFile(path.join(directory,'timing.json'),JSON.stringify(timing,null,2));
console.log('Exported two 128px reaction atlases and matching preview timing.');
