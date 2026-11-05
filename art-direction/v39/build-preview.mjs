import fs from 'node:fs/promises';
import path from 'node:path';
import {fileURLToPath} from 'node:url';
import sharp from '../../.cache/sprite-tools/node_modules/sharp/lib/index.js';
import {encodeGIF} from '../../.cache/sprite-tools/node_modules/pixelbolt/src/index.js';
const dir=path.dirname(fileURLToPath(import.meta.url)),assets=path.resolve(dir,'../../prototype/assets/broadcast-v39');
const manifest=JSON.parse(await fs.readFile(path.join(assets,'manifest.json'),'utf8'));
const crop=async(name,i,left=0,width=384)=>sharp(path.join(assets,name+'.png')).extract({left,top:i*128,width,height:128}).png().toBuffer();
const frames=[];
for(let i=0;i<manifest.events.stand.durationsMs.length;i++)frames.push({data:await sharp(await crop('stand',i)).ensureAlpha().raw().toBuffer(),duration:manifest.events.stand.durationsMs[i]});
for(let i=0;i<64;i++){
 const age=i/8,pose=age<1.2?0:age<2.1?2:age<3?4:age<5.1?6:age<6.2?4:0;
 const mouth=age<4.6&&i%3!==0?1:0;
 const g=await crop('excited',pose+mouth,0,192),r=await crop('rene-conversation',age>=3.55&&age<6.55?5:0,0,192);
 const pair=await sharp(g).extend({right:192,background:'#081522'}).composite([{input:r,left:192,top:0}]).ensureAlpha().raw().toBuffer();
 frames.push({data:pair,duration:125});
}
const g=await crop('gerard-conversation',0,0,192),r=await crop('rene-conversation',0,0,192);
frames.push({data:await sharp(g).extend({right:192,background:'#081522'}).composite([{input:r,left:192,top:0}]).ensureAlpha().raw().toBuffer(),duration:1500});
await fs.writeFile(path.join(dir,'standing-aftermath.gif'),encodeGIF(frames,384,128,{loop:true,scale:2}).data);
const glyphs=JSON.parse(await fs.readFile(path.resolve(dir,'../v38/alphabet-source.json'),'utf8'));
const html=`<!doctype html><meta charset="utf-8"><title>Evolution Wars / v39</title><style>body{background:#06101c;color:#e0edff;max-width:1000px;margin:40px auto;font:17px system-ui}h1{color:#35e6ff}img{image-rendering:pixelated;max-width:100%}section{padding:22px;border:1px solid #285680;margin:25px 0}canvas{width:100%;background:#000}button{padding:10px;background:#173253;color:white;border:1px solid #35e6ff}</style><h1>Gerard / a comfortable neck and a lasting reaction</h1><section><img src="standing-aftermath.gif" alt="Standing, settling back, excited speech and a delayed glance from Rene"><p>Stand → settle back → watch both sides → invite René → calm down.</p></section><section><h2>Stationary LED matrix</h2><canvas width="960" height="84"></canvas><button id="pause">Pause</button><p>Every bulb stays at the same position. The message advances by changing colors on the lattice.</p></section><script>
const glyphs=${JSON.stringify(glyphs)},text='RENÉ ! TU AS VU ÇA ? • EVERY LAST ONE ! • 100%   ',colors=['#35e6ff','#ff55bb','#ffe64b','#ff963c','#8bff76','#b497ff'];
const c=document.querySelector('canvas'),ctx=c.getContext('2d');let time=0,last=0,paused=false;
document.querySelector('button').onclick=e=>{paused=!paused;e.target.textContent=paused?'Resume':'Pause'};
const raster=[];let word=0;for(let i=0;i<text.length;i++){if(i&&text[i-1]==' '&&text[i]!=' ')word++;for(let x=0;x<8;x++)raster.push(Array.from({length:12},(_,y)=>x>=1&&x<=5&&((glyphs[text[i]]||glyphs['?'])[y]||0)&(1<<(5-x))?colors[word%6]:null));}
function draw(now){if(!paused)time+=(now-last)/1000;last=now;ctx.fillStyle='#000';ctx.fillRect(0,0,c.width,c.height);const offset=Math.floor(time*14),pitch=4;
for(let x=0;x<c.width/pitch;x++)for(let y=0;y<c.height/pitch;y++){const row=y-4,color=raster[(x+offset)%raster.length][row];ctx.fillStyle=color||'#12171c';ctx.beginPath();ctx.arc(x*pitch+pitch/2,y*pitch+pitch/2,1.35,0,Math.PI*2);ctx.fill();}requestAnimationFrame(draw)}requestAnimationFrame(n=>{last=n;draw(n)});
</script>`;
await fs.writeFile(path.join(dir,'index.html'),html);
console.log('Standing + seated aftermath GIF and fixed-LED preview exported.');
