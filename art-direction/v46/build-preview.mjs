import fs from 'node:fs/promises';
import path from 'node:path';
import {fileURLToPath} from 'node:url';
import sharp from '../../.cache/sprite-tools/node_modules/sharp/lib/index.js';
import {encodeGIF} from '../../.cache/sprite-tools/node_modules/pixelbolt/src/index.js';

const directory=path.dirname(fileURLToPath(import.meta.url));
const frameDirectory=path.join(directory,'captures/sign-sequence');
const names=(await fs.readdir(frameDirectory)).filter(n=>n.endsWith('.png')).sort();
if(names.length!==102)throw new Error('Expected 102 actual shader captures.');
const meta=await sharp(path.join(frameDirectory,names[0])).metadata();
const stripHeight=meta.height,stripWidth=meta.width;
const inputs=await Promise.all(names.map(n=>fs.readFile(path.join(frameDirectory,n))));
const atlas=await sharp({create:{width:stripWidth,height:stripHeight*names.length,channels:3,background:'black'}})
 .composite(inputs.map((input,i)=>({input,left:0,top:i*stripHeight})))
 .png({palette:true,colours:256,dither:0}).toBuffer();
const frames=[];
for(let i=0;i<names.length;i++){
 const data=await sharp(atlas).extract({left:0,top:i*stripHeight,width:stripWidth,height:stripHeight}).ensureAlpha().raw().toBuffer();
 frames.push({data,duration:100});
}
await fs.writeFile(path.join(directory,'matrix-performance.gif'),encodeGIF(frames,stripWidth,stripHeight,{loop:true}).data);
console.log(`Exported ${frames.length} actual shader frames at ${stripWidth}x${stripHeight}.`);
