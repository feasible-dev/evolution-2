import fs from 'node:fs/promises';
import path from 'node:path';
import {fileURLToPath} from 'node:url';
import assert from 'node:assert/strict';
import sharp from '../../../.cache/sprite-tools/node_modules/sharp/lib/index.js';
import {importPNG, encodeGIF, createBundle, writeBundle} from '../../../.cache/sprite-tools/node_modules/pixelbolt/src/index.js';

const dir = path.dirname(fileURLToPath(import.meta.url));
const width = 384, height = 128, half = 192;
const step = (g, r, ms, label) => ({g, r, ms, label});
// Independent actor pose choices make reaction delays explicit, rather than
// relying on both actors moving together in a generated illustration.
const scenes = [
 {id:'stand', source:'stand', title:'The delayed double take', description:'Gerard rises first. Le Chevalier notices 490 ms later, turns, then lifts his chin. Gerard stays up for 3.74 seconds before sitting.', steps:[
  step(0,0,1000,'Watching the rink'), step(1,0,160,'Gerard pushes up / partner still watching'),
  step(2,0,180,'Rising / partner has not noticed'), step(3,0,150,'Standing / partner still looking down'),
  step(3,2,180,'Partner notices: eyes first'), step(3,3,200,'Surprised head turn'),
  step(3,4,350,'Chin rises toward Gerard'), step(3,5,450,'Standing hold / partner looks up'),
  step(4,5,240,'Small forearm lift'), step(5,5,380,'Forearms settle'),
  step(3,5,550,'Standing hold'), step(4,5,240,'Second small gesture'),
  step(5,5,450,'Forearms settle'), step(3,5,550,'Standing hold'),
  step(6,5,220,'Gerard sits / partner still looks up'), step(7,6,250,'Seated / partner catches up'),
  step(0,3,200,'Partner lowers his head'), step(0,0,1000,'Back to the rink')
 ]},
 {id:'slap', source:'slap', title:'A stunned table slap', description:'Surprise and passionate disbelief, with a downward gaze at impact. Le Chevalier reacts after the hand lands.', steps:[
  step(0,0,950,'Watching'), step(1,0,180,'Notice the play'), step(2,0,120,'Hand lifts'),
  step(3,0,140,'Anticipation'), step(4,0,90,'Impact / eyes down'),
  step(5,0,90,'Rebound / partner has not reacted'), step(5,5,180,'Partner startles after impact'),
  step(6,6,650,'Astonished exhale'), step(7,6,300,'Gerard settles first'), step(0,0,900,'Watching again')
 ]},
 {id:'idle', source:'basic', title:'Following the rink', description:'A stable face and body, small eye movements and a short blink. No cycling wrinkle pixels.', steps:[
  step(0,0,1400,'Watch'), step(1,0,650,'Small eye movement'), step(2,0,110,'Blink'),
  step(0,0,850,'Watch'), step(3,0,700,'Small eye movement'), step(0,0,1100,'Watch')
 ]},
 {id:'talk', source:'basic', title:'Commentating the play', description:'Stable hair, glasses and face contours. Only the mouth changes for syllables, followed by a small hand gesture.', steps:[
  step(0,0,400,'Listen'), step(4,0,130,'Begin'), step(5,0,140,'Rounded syllable'),
  step(4,0,110,'Syllable'), step(0,0,90,'Closed mouth'), step(6,0,220,'Small hand gesture'),
  step(7,0,180,'Hand settles'), step(0,0,500,'Listen')
 ]},
 {id:'glance', source:'glance', title:'A quiet aside', description:'Gerard looks over first. Le Chevalier answers the glance, then they return to the rink.', steps:[
  step(0,0,900,'Watching'), step(1,0,180,'Eyes first'), step(2,0,230,'Gerard turns'),
  step(3,3,300,'Partner notices'), step(4,4,650,'The aside'), step(5,5,700,'Dry amusement'),
  step(6,5,250,'Gerard returns to rink'), step(7,7,800,'Both watching'), step(0,0,400,'Rest')
 ]}
];

const sources = new Map();
const desk = Buffer.from('<svg width="384" height="8"><rect width="384" height="8" fill="#102b46"/><rect width="384" height="2" fill="#285680"/></svg>');
for (const id of ['stand','slap','basic','glance']) {
 const source = path.join(dir, id + '-source.png');
 const meta = await sharp(source).metadata();
 assert.equal(meta.width % 2, 0); assert.equal(meta.height % 4, 0);
 const cellWidth = meta.width / 2, cellHeight = meta.height / 4;
 const poses = [];
 for (let i = 0; i < 8; i++) {
  // Remove the generated panel dividers; lock the desk to one shared strip.
  const overlays = [{input:desk,left:0,top:120}];
  // The generated impact marks were beside the stationary hand. Remove them;
  // the moving palm and its rebound carry the impact without a misleading cue.
  if (id === 'slap' && i === 4) overlays.push({input:Buffer.from('<svg width="24" height="21"><rect width="24" height="21" fill="#081522"/></svg>'),left:181,top:99});
  const pose = await sharp(source).extract({left:(i%2)*cellWidth+2, top:Math.floor(i/2)*cellHeight+2, width:cellWidth-4, height:cellHeight-4})
   .resize(width,height,{kernel:'nearest',fit:'fill'}).composite(overlays).png().toBuffer();
  poses.push(pose);
 }
 sources.set(id, poses);
}

async function stabilizedBasic(pose, id) {
 const source = sources.get('basic');
 if (pose === 0) return source[0];
 const region = id === 'idle' ? {left:103,top:44,width:39,height:22} : {left:113,top:69,width:24,height:18};
 const overlays = [{input:await sharp(source[pose]).extract(region).png().toBuffer(),left:region.left,top:region.top}];
 // The large gesture can change the body, while the same head silhouette remains.
 if (id === 'talk' && pose >= 6) overlays.push({input:await sharp(source[pose]).extract({left:0,top:94,width:half,height:26}).png().toBuffer(),left:0,top:94});
 return sharp(source[0]).composite(overlays).png().toBuffer();
}

const report = [];
for (const scene of scenes) {
 const pairs = [];
 for (const s of scene.steps) {
  const g = scene.source === 'basic' ? await stabilizedBasic(s.g, scene.id) : sources.get(scene.source)[s.g];
  const r = sources.get(scene.source)[s.r];
  const pair = await sharp(g).composite([{input:await sharp(r).extract({left:half,top:0,width:half,height}).png().toBuffer(),left:half,top:0}]).png().toBuffer();
  pairs.push(pair);
 }
 const count = pairs.length;
 const sheet = await sharp({create:{width,height:height*count,channels:4,background:'#081522'}})
  .composite(pairs.map((input,i)=>({input,left:0,top:i*height})))
  .png({palette:true,colours:64,quality:100,dither:0,effort:10}).toBuffer();
 await fs.writeFile(path.join(dir,scene.id+'-sheet.png'),sheet);
 const frames = [];
 for (let i=0; i<count; i++) frames.push({data:await sharp(sheet).extract({left:0,top:i*height,width,height}).ensureAlpha().raw().toBuffer(),duration:scene.steps[i].ms});
 await fs.writeFile(path.join(dir,scene.id+'.gif'),encodeGIF(frames,width,height,{loop:true,scale:2}).data);
 for (const [actor,offset] of [['gerard',0],['le-chevalier',half]]) {
  const actorSheet = await sharp(sheet).extract({left:offset,top:0,width:half,height:height*count}).png().toBuffer();
  const names = frames.map((_,i)=>'pose-'+i);
  const atlas = {frames:Object.fromEntries(names.map((name,i)=>[name,{frame:{x:0,y:i*height,w:half,h:height},duration:scene.steps[i].ms}])),animations:{[scene.id]:{frames:names,loop:scene.id==='idle'||scene.id==='talk'}}};
  const {recipe} = importPNG(actorSheet,{name:actor+'-'+scene.id,atlas});
  recipe.sheet = {columns:4,padding:1,scale:1};
  await fs.writeFile(path.join(dir,actor+'-'+scene.id+'.json'),JSON.stringify(recipe));
  await writeBundle(await createBundle(recipe),path.join(dir,'exports',actor+'-'+scene.id),{force:true});
 }
 const gifMeta = await sharp(path.join(dir,scene.id+'.gif'),{animated:true}).metadata();
 assert.equal(gifMeta.pages,count); assert.equal(gifMeta.width,768); assert.equal(gifMeta.pageHeight,256);
 assert.deepEqual(gifMeta.delay,scene.steps.map(s=>s.ms));
 // A held actor is byte-identical, not a new noisy drawing every beat.
 const raw = frames.map(f=>f.data);
 function actorEqual(a,b,offset) {
  for(let y=0;y<height;y++) {
   const start=(y*width+offset)*4;
   assert.ok(raw[a].subarray(start,start+half*4).equals(raw[b].subarray(start,start+half*4)));
  }
 }
 if(scene.id==='stand') {
  actorEqual(0,1,half); actorEqual(0,2,half); actorEqual(0,3,half);
  actorEqual(3,4,0); actorEqual(3,5,0); actorEqual(3,6,0); actorEqual(3,7,0);
 }
 report.push({id:scene.id,frames:count,totalMs:scene.steps.reduce((n,s)=>n+s.ms,0),gifWidth:gifMeta.width,gifFrameHeight:gifMeta.pageHeight,delaysVerified:true});
 console.log(`${scene.id}: ${count} frames exported and checked`);
}
const stand = scenes.find(s=>s.id==='stand');
const timing = {reactionDelayMs:stand.steps.slice(1,4).reduce((n,s)=>n+s.ms,0),standingHoldMs:stand.steps.slice(3,14).reduce((n,s)=>n+s.ms,0)};
assert.equal(timing.reactionDelayMs,490); assert.equal(timing.standingHoldMs,3740);
await fs.writeFile(path.join(dir,'scenes.json'),JSON.stringify(scenes,null,2));
await fs.writeFile(path.join(dir,'verification.json'),JSON.stringify({scenes:report,standing:timing,heldActorsByteIdentical:true},null,2));
const template = await fs.readFile(path.join(dir,'gallery-template.html'),'utf8');
await fs.writeFile(path.join(dir,'index.html'),template.replace('/* SCENE_DATA */',JSON.stringify(scenes)));
