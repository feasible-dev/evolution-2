import fs from 'node:fs/promises';
import path from 'node:path';
import {fileURLToPath} from 'node:url';
import sharp from 'sharp';
const dir=path.dirname(fileURLToPath(import.meta.url));
const out=path.resolve(dir,'../../../prototype/assets/broadcast-v39');
await fs.mkdir(out,{recursive:true});
const desk=Buffer.from('<svg width="384" height="8"><rect width="384" height="8" fill="#102b46"/><rect width="384" height="2" fill="#285680"/></svg>');
async function grid(name,rows){
 const source=path.join(dir,name+'-source.png'), m=await sharp(source).metadata();
 const frames=[];
 for(let i=0;i<rows*2;i++){
  const left=Math.round(i%2*m.width/2),top=Math.round(Math.floor(i/2)*m.height/rows);
  const right=Math.round((i%2+1)*m.width/2),bottom=Math.round((Math.floor(i/2)+1)*m.height/rows);
  frames.push(await sharp(source).extract({left:left+2,top:top+2,width:right-left-4,height:bottom-top-4})
   .resize(384,128,{kernel:'nearest'}).composite([{input:desk,left:0,top:120}]).png().toBuffer());
 }
 return frames;
}
const seated=await grid('seated',6), blink=await grid('blink',6), stand=await grid('stand',4), slap=await grid('slap',4);
await fs.writeFile(path.join(dir,'neutral-preview.png'),await sharp(seated[0]).resize(1152,384,{kernel:'nearest'}).png().toBuffer());
await fs.writeFile(path.join(dir,'seated-native.png'),await sharp({create:{width:384,height:1536,channels:4,background:'#081522'}}).composite(seated.map((input,i)=>({input,left:0,top:i*128}))).png().toBuffer());
// The neutral body, event entry/exit and delayed listener all use one master drawing.
const half=async(p,a)=>sharp(p).extract({left:a*192,top:0,width:192,height:128}).png().toBuffer();
async function patch(base,source,rect){return sharp(base).composite([{input:await sharp(source).extract(rect).png().toBuffer(),left:rect.left,top:rect.top}]).png().toBuffer();}
const banks=[];
for(let a=0;a<2;a++){
 const s=await Promise.all(seated.map(p=>half(p,a))),b=await Promise.all(blink.map(p=>half(p,a))),base=s[0];
 const mouth=a===0?{left:96,top:55,width:36,height:22}:{left:66,top:58,width:37,height:20};
 const eyes=a===0?{left:93,top:37,width:53,height:23}:{left:56,top:38,width:55,height:23};
 const head=a===0?{left:59,top:0,width:101,height:94}:{left:43,top:0,width:90,height:94};
 const pose=async(i)=>patch(base,s[i],head);
 const partner=await pose(4), turn=await pose(7);
 const conversation=[base,await patch(base,s[1],mouth),await patch(base,s[2],mouth),await patch(base,b[0],eyes),turn,partner,
  await patch(partner,s[5],mouth),await patch(partner,s[6],mouth),partner,await patch(base,s[1],mouth),base];
 const left=await pose(8),right=await pose(10);
 const rink=[left,await patch(left,s[9],mouth),right,await patch(right,s[11],mouth),base,conversation[1],left,right];
 // Brief closed lids use the matching master eye band, never a whole sleepy face.
 const matching=[8,9,10,11,0,1,8,10];
 for(let i=0;i<8;i++)rink.push(await patch(rink[i],b[matching[i]],eyes));
 banks.push({conversation,rink});
}
const oldScenes=JSON.parse(await fs.readFile(path.resolve(dir,'../../v35/duo-scenes/scenes.json'),'utf8'));
const events={};
for(const id of ['stand','slap']){
 const src=id==='stand'?stand:slap,steps=oldScenes.find(s=>s.id===id).steps;
 events[id]=[];
 for(const step of steps){
  const g=step.g===0||step.g===7?await half(seated[0],0):await half(src[step.g],0);
  const r=step.r===0||step.r===7?await half(seated[0],1):await half(src[step.r],1);
  events[id].push(await sharp(g).extend({right:192,background:'#081522'}).composite([{input:r,left:192,top:0}]).png().toBuffer());
 }
}
const sheets=[];
const excitedSource=await grid('excited',4);
const excited=[];
for(let i=0;i<8;i++){
 const base=excitedSource[i-i%2];
 const mouth={left:89+(i>=4?13:0),top:54+(i>=2?5:0),width:41,height:24};
 excited.push(i%2?await patch(base,excitedSource[i],mouth):base);
}
sheets.push({name:'excited',width:384,frames:excited});
for(let a=0;a<2;a++) sheets.push({name:(a===0?'gerard':'rene')+'-conversation',width:192,frames:banks[a].conversation});
const rink=[];
for(let i=0;i<16;i++)rink.push(await sharp(banks[0].rink[i]).extend({right:192,background:'#081522'}).composite([{input:banks[1].rink[i],left:192,top:0}]).png().toBuffer());
sheets.push({name:'rink',width:384,frames:rink},...Object.entries(events).map(([name,frames])=>({name,width:384,frames})));
// Quantize once for the whole library: identical pixels never shift between clip palettes.
const all=[];let row=0;
for(const sheet of sheets){sheet.row=row; for(const input of sheet.frames){all.push({input,left:0,top:row*128});row++;}}
const atlas=await sharp({create:{width:384,height:row*128,channels:4,background:'#081522'}}).composite(all).png({palette:true,colours:128,dither:0}).toBuffer();
for(const sheet of sheets) await sharp(atlas).extract({left:0,top:sheet.row*128,width:sheet.width,height:sheet.frames.length*128}).png().toFile(path.join(out,sheet.name+'.png'));
const manifest={version:39,frameWidth:192,frameHeight:128,conversationFrames:11,rinkFrames:16,excitedFrames:8,events:{}};
for(const id of ['stand','slap'])manifest.events[id]={durationsMs:oldScenes.find(s=>s.id===id).steps.map(s=>s.ms)};
await fs.writeFile(path.join(out,'manifest.json'),JSON.stringify(manifest,null,2));
console.log('Unified library exported with shared palette, body and event endpoints.');
