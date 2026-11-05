import fs from 'node:fs/promises';
import path from 'node:path';
import {fileURLToPath} from 'node:url';
import sharp from '../../../.cache/sprite-tools/node_modules/sharp/lib/index.js';

const dir=path.dirname(fileURLToPath(import.meta.url));
const previous=path.resolve(dir,'../../v35/duo-scenes');
const output=path.resolve(dir,'../../../prototype/assets/broadcast-v36');
await fs.mkdir(output,{recursive:true});
const desk=Buffer.from('<svg width="192" height="8"><rect width="192" height="8" fill="#102b46"/><rect width="192" height="2" fill="#285680"/></svg>');
async function cropGrid(file,index,actorOnly=false){
 const m=await sharp(file).metadata(),w=m.width/2,h=m.height/4;
 const img=await sharp(file).extract({left:(index%2)*w+2,top:Math.floor(index/2)*h+2,width:w-4,height:h-4})
  .resize(actorOnly?192:384,128,{kernel:'nearest',fit:'fill'}).png().toBuffer();
 return actorOnly?img:sharp(img).extract({left:0,top:0,width:192,height:128}).png().toBuffer();
}
async function patch(base,source,rect){
 const input=await sharp(source).extract(rect).png().toBuffer();
 return sharp(base).composite([{input,left:rect.left,top:rect.top}]).png().toBuffer();
}
async function saveBank(actor,poses){
 const stabilized=await Promise.all(poses.map(async p=>{
  // Register the newly drawn analyst with his existing event poses.
  if(actor==='rene') p=await sharp({create:{width:192,height:128,channels:4,background:'#081522'}})
   .composite([{input:await sharp(p).extract({left:14,top:0,width:178,height:128}).png().toBuffer(),left:0,top:0}]).png().toBuffer();
  return sharp(p).composite([{input:desk,left:0,top:120}]).png().toBuffer();
 }));
 const sheet=await sharp({create:{width:192,height:128*poses.length,channels:4,background:'#081522'}})
  .composite(stabilized.map((input,i)=>({input,left:0,top:i*128}))).png({palette:true,colours:64,dither:0,quality:100}).toBuffer();
 await fs.writeFile(path.join(output,actor+'-conversation.png'),sheet);
 await fs.writeFile(path.join(dir,actor+'-conversation.png'),sheet);
}
const basic=await Promise.all(Array.from({length:8},(_,i)=>cropGrid(path.join(previous,'basic-source.png'),i)));
const aside=await Promise.all(Array.from({length:8},(_,i)=>cropGrid(path.join(previous,'glance-source.png'),i)));
const mouth={left:113,top:69,width:24,height:18};
// Preserve the same body and face contour through ordinary syllables.
const g0=basic[0],g1=await patch(g0,basic[4],mouth),g2=await patch(g0,basic[5],mouth);
const gBlink=await patch(g0,basic[2],{left:103,top:44,width:39,height:22});
const gPartner=await patch(g0,aside[2],{left:64,top:4,width:100,height:90});
const gPartnerTalk=await patch(gPartner,aside[4],{left:123,top:70,width:25,height:18});
const gTurn=await patch(g0,aside[1],{left:103,top:44,width:39,height:22});
const gExplain=await patch(g1,basic[6],{left:0,top:94,width:192,height:26});
const gSettle=await patch(g0,basic[7],{left:0,top:94,width:192,height:26});
await saveBank('gerard',[g0,g1,g2,gBlink,gTurn,gPartner,gPartnerTalk,gPartnerTalk,gPartner,gExplain,gSettle]);

const r=await Promise.all(Array.from({length:8},(_,i)=>cropGrid(path.join(dir,'rene-speaking-source.png'),i,true)));
const r0=r[0],r1=await patch(r0,r[1],{left:80,top:66,width:34,height:17}),r2=await patch(r0,r[2],{left:80,top:66,width:34,height:17});
const rBlink=await patch(r0,r[3],{left:76,top:49,width:44,height:18});
// Keep the same body under the head turn, not the independently generated hands.
const rPartner=await patch(r0,r[4],{left:50,top:4,width:90,height:92});
const rPartnerA=await patch(rPartner,r[5],{left:72,top:72,width:34,height:19});
const rPartnerO=await patch(rPartner,r[6],{left:72,top:72,width:34,height:19});
const rTurn=await patch(r0,r[7],{left:50,top:4,width:90,height:92});
await saveBank('rene',[r0,r1,r2,rBlink,rTurn,rPartner,rPartnerA,rPartnerO,rPartner,r0,r0]);

const scenes=JSON.parse(await fs.readFile(path.join(previous,'scenes.json'),'utf8'));
const manifest={version:36,frameWidth:192,frameHeight:128,conversationFrames:11,events:{}};
for(const id of ['stand','slap']){
 const scene=scenes.find(s=>s.id===id);
 await fs.copyFile(path.join(previous,id+'-sheet.png'),path.join(output,id+'.png'));
 manifest.events[id]={durationsMs:scene.steps.map(s=>s.ms)};
}
await fs.writeFile(path.join(output,'manifest.json'),JSON.stringify(manifest,null,2));
console.log('Runtime banks and timed events exported to '+output);
