import fs from 'node:fs/promises';
import path from 'node:path';
import {fileURLToPath} from 'node:url';
import sharp from '../../../.cache/sprite-tools/node_modules/sharp/lib/index.js';
import {importPNG,encodeGIF,renderProject,createBundle,writeBundle} from '../../../.cache/sprite-tools/node_modules/pixelbolt/src/index.js';

const dir=path.dirname(fileURLToPath(import.meta.url));
const scenes=[
 {id:'idle',title:'Following the rink',durations:[1500,650,110,130,700,900,800],labels:['Rink center','Track left','Blink','Open eyes','Track right','Rink center','Rest']},
 {id:'talk',title:'Commentating the play',durations:[300,130,160,130,150,250,300],labels:['Listen','Begin speaking','Explain the play','Syllable','Finish','Close mouth','Rest']},
 {id:'stand',title:'Too enthusiastic',durations:[850,180,220,1000,250,650,500],labels:['Watching','Hands on desk','Rising','Standing — gaze up-left','Sitting down','Sideways reaction','Rest']},
 {id:'glance',title:'A knowing look',durations:[700,200,450,700,600,600,500],labels:['Watching','Turn toward partner','Eye contact','The joke','Shared amusement','Return','Rest']},
 {id:'slap',title:'The table slap',durations:[800,160,180,90,500,650,500],labels:['Watching','Prepare','Anticipation','Impact','Reaction','Settle','Rest']}
];
const report=[];
for(const scene of scenes){
 const src=path.join(dir,scene.id+'-source.png');
 const {data,info}=await sharp(src).ensureAlpha().raw().toBuffer({resolveWithObject:true});
 const h=Math.round(info.height/6), deskHeight=16, targetY=h-deskHeight;
 const crops=[],anchors=[];
 for(let i=0;i<6;i++){
   // Detect the desk's blue highlight using the outer edges, away from the actors.
   const expected=(i+1)*info.height/6-deskHeight;
   let best=-Infinity,anchor=Math.round(expected);
   for(let y=Math.max(0,Math.floor(expected-12));y<Math.min(info.height,Math.ceil(expected+12));y++){
     let score=0;
     for(let x=0;x<20;x++)for(const xx of [x,info.width-1-x]){const p=(y*info.width+xx)*4;score+=data[p+2]+data[p+1]-2*data[p];}
     if(score>best){best=score;anchor=y;}
   }
   anchors.push(anchor);
   const top=anchor-targetY,from=Math.max(0,top),height=Math.min(info.height,top+h)-from;
   const clip=await sharp(src).extract({left:0,top:from,width:info.width,height}).png().toBuffer();
   // Draw one stationary desk over the six generated desk strips.
   const desk=Buffer.from(`<svg width="${info.width}" height="${deskHeight}"><rect width="100%" height="100%" fill="#102b46"/><rect width="100%" height="3" fill="#285680"/></svg>`);
   const aligned=await sharp({create:{width:info.width,height:h,channels:4,background:'#081522'}})
      .composite([{input:clip,left:0,top:from-top},{input:desk,left:0,top:targetY}]).png().toBuffer();
   crops.push(aligned);
 }
 // Exact same rest at the end of each standalone playback.
 crops.push(crops[0]);
 const width=384,height=128;
 const small=await Promise.all(crops.map(input=>sharp(input).resize(width,height,{kernel:'nearest',fit:'fill'}).png().toBuffer()));
 const sheet=await sharp({create:{width,height:height*7,channels:4,background:'#081522'}})
   .composite(small.map((input,i)=>({input,left:0,top:i*height})))
   .png({palette:true,colours:128,quality:100,effort:10,dither:0}).toBuffer();
 await fs.writeFile(path.join(dir,scene.id+'-sheet.png'),sheet);
 const frames=[];
 for(let i=0;i<7;i++){
   const raw=await sharp(sheet).extract({left:0,top:i*height,width,height}).ensureAlpha().raw().toBuffer();
   frames.push({data:raw,duration:scene.durations[i]});
 }
 const gif=encodeGIF(frames,width,height,{loop:true,scale:2});
 await fs.writeFile(path.join(dir,scene.id+'.gif'),gif.data);
 // Separate synchronized actor resources respect Pixelbolt's 256px frame limit.
 for(const [actor,offset] of [['gerard',0],['le-chevalier',192]]){
   const actorSheet=await sharp(sheet).extract({left:offset,top:0,width:192,height:height*7}).png().toBuffer();
   const names=frames.map((_,i)=>'pose-'+i);
   const atlas={frames:Object.fromEntries(names.map((n,i)=>[n,{frame:{x:0,y:i*height,w:192,h:height},duration:scene.durations[i]}])),animations:{[scene.id]:{frames:names,loop:false}}};
   const {recipe}=importPNG(actorSheet,{name:`${actor}-${scene.id}`,atlas});
   recipe.sheet={columns:4,padding:1,scale:1};
   const recipePath=path.join(dir,`${actor}-${scene.id}.json`);
   await fs.writeFile(recipePath,JSON.stringify(recipe));
   const bundle=await createBundle(recipe);
   await writeBundle(bundle,path.join(dir,'exports',`${actor}-${scene.id}`),{force:true});
 }
 report.push({id:scene.id,width,height,frames:7,originalPoses:6,deskAnchors:anchors,durations:scene.durations});
}
await fs.writeFile(path.join(dir,'scenes.json'),JSON.stringify(scenes,null,2));
await fs.writeFile(path.join(dir,'verification.json'),JSON.stringify(report,null,2));
console.log(JSON.stringify(report,null,2));
