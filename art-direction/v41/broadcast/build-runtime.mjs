import fs from 'node:fs/promises';
import path from 'node:path';
import {fileURLToPath} from 'node:url';
import assert from 'node:assert/strict';
import sharp from '../../../.cache/sprite-tools/node_modules/sharp/lib/index.js';
const dir=path.dirname(fileURLToPath(import.meta.url));
const root=path.resolve(dir,'../../..'),out=path.join(root,'prototype/assets/broadcast-v41');
await fs.mkdir(out,{recursive:true});
const source=path.join(dir,'excited-source.png'),meta=await sharp(source).metadata();
const cells=[];
for(let i=0;i<8;i++)cells.push(await sharp(source).extract({left:i%2*meta.width/2+2,top:Math.floor(i/2)*meta.height/4+2,width:meta.width/2-4,height:meta.height/4-4}).resize(384,128,{kernel:'nearest'}).png().toBuffer());
// Shared source palette for all replacement head variants.
const paletteSheet=await sharp({create:{width:384,height:1024,channels:4,background:'#081522'}}).composite(cells.map((input,i)=>({input,left:0,top:i*128}))).png({palette:true,colours:128,dither:0}).toBuffer();
const revised=[];const head={left:48,top:0,width:108,height:92};
for(let i=0;i<8;i++){
 const old=await sharp(path.join(root,'prototype/assets/broadcast-v39/excited.png')).extract({left:0,top:(i-i%2)*128,width:384,height:128}).png().toBuffer();
 let generated=await sharp(paletteSheet).extract({left:0,top:(i-i%2)*128,width:384,height:128}).png().toBuffer();
 if(i%2){
  // Syllables replace the lips only, never the chin, jaw contour or neck.
  const mouth=[{left:96,top:54,width:19,height:6},{left:87,top:62,width:19,height:6},
   {left:111,top:61,width:19,height:6},{left:112,top:60,width:19,height:6}][Math.floor(i/2)];
  const open=await sharp(paletteSheet).extract({left:mouth.left,top:i*128+mouth.top,width:mouth.width,height:mouth.height}).png().toBuffer();
  // The generated partner syllables sit lower than the closed lips; register
  // them to the closed pose rather than carrying that jaw displacement over.
  generated=await sharp(generated).composite([{input:open,left:mouth.left,top:i>=4?54:mouth.top}]).png().toBuffer();
 }
 const patch=await sharp(generated).extract(head).png().toBuffer();
 const frame=await sharp(old).composite([{input:patch,left:head.left,top:head.top}]).png().toBuffer();
 // Keep the original hands, torso and entire other commentator byte-identical.
 for(const region of [{left:0,top:92,width:384,height:36},{left:192,top:0,width:192,height:128}]){
  const original=await sharp(old).extract(region).ensureAlpha().raw().toBuffer();
  const updated=await sharp(frame).extract(region).ensureAlpha().raw().toBuffer();
  assert.ok(original.equals(updated));
 }
 revised.push(frame);
}
await sharp({create:{width:384,height:1024,channels:4,background:'#081522'}}).composite(revised.map((input,i)=>({input,left:0,top:i*128}))).removeAlpha().png().toFile(path.join(out,'excited.png'));
const comparison=[];
for(let pair=0;pair<4;pair++){
 const old=await sharp(path.join(root,'prototype/assets/broadcast-v39/excited.png')).extract({left:0,top:pair*256,width:192,height:128}).png().toBuffer();
 const fixed=await sharp(revised[pair*2]).extract({left:0,top:0,width:192,height:128}).png().toBuffer();
 comparison.push({input:old,left:0,top:pair*128},{input:fixed,left:192,top:pair*128});
}
const comparisonSheet=await sharp({create:{width:384,height:512,channels:4,background:'#081522'}}).composite(comparison).png().toBuffer();
await sharp(comparisonSheet).resize(768,1024,{kernel:'nearest'}).png().toFile(path.join(dir,'before-after.png'));
console.log('Head corrections exported; body and Rene continuity verified for all eight frames.');

// Import restrained mouth drawings into the existing pose masters. Generated
// open-mouth drawings are not allowed to replace the whole lower face.
const speechSource=path.join(dir,'speech-source.png');
const speechMeta=await sharp(speechSource).metadata();
const speech=[];
for(let i=0;i<12;i++){
 const left=Math.round(i%2*speechMeta.width/2),right=Math.round((i%2+1)*speechMeta.width/2);
 const top=Math.round(Math.floor(i/2)*speechMeta.height/6),bottom=Math.round((Math.floor(i/2)+1)*speechMeta.height/6);
 speech.push(await sharp(speechSource).extract({left:left+2,top:top+2,width:right-left-4,height:bottom-top-4})
  .resize(384,128,{kernel:'nearest'}).png().toBuffer());
}
async function readFrames(name,width,count){
 const sheet=path.join(root,'prototype/assets/broadcast-v39',name+'.png');
 return Promise.all(Array.from({length:count},(_,i)=>sharp(sheet).extract({left:0,top:i*128,width,height:128}).png().toBuffer()));
}
async function lips(base,source,rect,destinationTop=rect.top){
 const patch=await sharp(source).extract({...rect,height:9}).resize(rect.width,rect.height,{kernel:'nearest'}).png().toBuffer();
 return sharp(base).composite([{input:patch,left:rect.left,top:destinationTop}]).png().toBuffer();
}
async function writeFrames(name,width,frames){
 const sheet=await sharp({create:{width,height:frames.length*128,channels:4,background:'#081522'}})
  .composite(frames.map((input,i)=>({input,left:0,top:i*128}))).png().toBuffer();
 await sharp(sheet).removeAlpha().png().toFile(path.join(out,name+'.png'));
}
const conversation=await readFrames('gerard-conversation',192,11);
const front={left:102,top:61,width:19,height:6},partner={left:112,top:61,width:19,height:6};
for(const [dest,base,source,rect] of [[1,0,1,front],[2,0,2,front],[6,5,5,partner],[7,5,6,partner],[9,0,1,front]])
 conversation[dest]=await lips(conversation[base],speech[source],rect,base===5?58:rect.top);
await writeFrames('gerard-conversation',192,conversation);
const rink=await readFrames('rink',384,16);
for(const blink of [0,8]){
 for(const [dest,base,source,rect] of [[1,0,9,{left:102,top:64,width:19,height:6}],
  [3,2,11,{left:108,top:62,width:19,height:6}],[5,4,1,front]]){
  const oldRene=await sharp(rink[dest+blink]).extract({left:192,top:0,width:192,height:128}).png().toBuffer();
  rink[dest+blink]=await sharp(await lips(rink[base+blink],speech[source],rect))
   .composite([{input:oldRene,left:192,top:0}]).png().toBuffer();
 }
}
await writeFrames('rink',384,rink);
console.log('Speech rebuilt from closed-mouth masters; lip motion stays within six native pixels.');
const speechComparison=[];
for(const [name,pairs] of [['gerard-conversation',[[0,1],[0,2],[5,6],[5,7]]],['excited',[[0,1],[2,3],[4,5],[6,7]]]]){
 for(const pair of pairs) for(let column=0;column<2;column++){
  const input=await sharp(path.join(out,name+'.png')).extract({left:64,top:pair[column]*128+24,width:84,height:64}).png().toBuffer();
  speechComparison.push({input,left:column*84,top:Math.floor(speechComparison.length/2)*64});
 }
}
const speechReview=await sharp({create:{width:168,height:512,channels:4,background:'#081522'}}).composite(speechComparison).png().toBuffer();
await sharp(speechReview).resize(672,2048,{kernel:'nearest'}).png().toFile(path.join(dir,'speech-pairs.png'));
