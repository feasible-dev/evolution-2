// Mechanical sprite extraction, framing and animation export; no pose morphing.
const sharp=require('../../.cache/sprite-tools/node_modules/sharp');
const fs=require('node:fs/promises');
const path=require('node:path');
const root=__dirname;
const width=192,height=192;
const paws=[
  [0,850],[1,150],[2,180],[3,190],[4,140],[3,140],[4,160],[5,160],
  [6,300],[5,140],[6,250],[5,140],[7,150],[8,180],[9,200],[8,180],[9,200]
];
const opening=[
  [12,180],[13,160],[14,180],[15,350],[16,160],[15,180],[16,160],
  [17,170],[18,180],[19,180],[20,350],[21,180],[20,180],[21,160],[22,180],[23,180]
];
const settle=[[10,180],[1,150],[0,1000]];
const clips={full:[...paws,...opening,...settle],paws:[...paws,...settle],opening:[[12,800],...opening.slice(1),[12,800]]};
const names=['Ready','Anticipation','Paws up','Left cheek','Left paw pulse','Center','Right cheek','Hip sway','Left paw forward','Right paw forward','Settle','Ready',
  'Paws to opening','Left preparation','Left rise','Left opening','Left rebound','Return through center','Gather','Right rise','Right opening','Right rebound','Recover','Paws again'];
async function main(){
  const source=path.join(root,'nao-opening-grid.png');
  const meta=await sharp(source).metadata();
  if(meta.width!==1254||meta.height!==1254)throw Error('Unexpected source geometry');
  await fs.mkdir(path.join(root,'frames'),{recursive:true});
  const frames=[],crops=[];
  for(let i=0;i<12;i++){
    const old=await fs.readFile(path.join(root,'../level-2-v04/frames',`${String(i).padStart(2,'0')}.png`));
    frames.push(await sharp({create:{width,height,channels:4,background:'#101827'}})
      .composite([{input:old,left:32,top:0}]).png().toBuffer());
  }
  const rowTops=[0,412,822];
  for(let i=0;i<12;i++){
    const col=i%4,row=Math.floor(i/4),left=Math.round(col*1254/4),right=Math.round((col+1)*1254/4);
    const crop={left,top:rowTops[row],width:right-left,height:427};crops.push(crop);
    // Same ~0.45 source scale as v04. Widen the canvas, never shrink the head
    // to fit outstretched arms. Shift the new atlas down to align the face.
    const tile=await sharp(source).extract(crop).resize(141,192,{kernel:'nearest',fit:'fill'})
      .png().toBuffer();
    const visible=await sharp(tile).extract({left:0,top:0,width:141,height:172}).png().toBuffer();
    let frame=await sharp({create:{width,height,channels:4,background:'#101827'}})
      .composite([{input:visible,left:25,top:20}]).png().toBuffer();
    if(i===0||i===11)frame=frames[5];
    frames.push(frame);
  }
  for(let i=0;i<frames.length;i++)await fs.writeFile(path.join(root,'frames',`${String(i).padStart(2,'0')}.png`),frames[i]);
  await sharp({create:{width:768,height:1152,channels:4,background:'#101827'}})
    .composite(frames.map((input,i)=>({input,left:i%4*width,top:Math.floor(i/4)*height})))
    .png().toFile(path.join(root,'nao-dance-native.png'));
  for(const [name,sequence]of Object.entries(clips)){
    const raw=[];
    for(const [i]of sequence)raw.push(await sharp(frames[i]).resize(384,384,{kernel:'nearest'}).ensureAlpha().raw().toBuffer());
    await sharp(Buffer.concat(raw),{raw:{width:384,height:384*sequence.length,channels:4,pageHeight:384}})
      .gif({delay:sequence.map(s=>s[1]),loop:0,dither:0,effort:7}).toFile(path.join(root,`nao-${name}-preview.gif`));
    const check=await sharp(path.join(root,`nao-${name}-preview.gif`),{animated:true}).metadata();
    // The GIF encoder may merge adjacent identical holds; duration must survive.
    const expectedDuration=sequence.reduce((sum,s)=>sum+s[1],0);
    const actualDuration=(check.delay||[]).reduce((sum,ms)=>sum+ms,0);
    if(!check.pages||check.pages>sequence.length||check.pageHeight!==384||actualDuration!==expectedDuration)
      throw Error(`Invalid ${name} export`);
  }
  const manifest={status:'Animation preview, not installed in game',generator:'Built-in imagegen',
    source:'nao-opening-grid.png',previousFrames:'../level-2-v04/frames',nativeFrame:{width,height},
    grid:{columns:4,rows:6},sourceScale:0.45,crops,poseNames:names,
    clips:Object.fromEntries(Object.entries(clips).map(([k,seq])=>[k,seq.map(([frame,durationMs])=>({frame,durationMs}))])),
    notes:['Approved character style and colors retained.',
      'Wider 192px canvas provides room for arms at the existing head scale.',
      'Opening bridge uses the exact centered-paws pose from v04.',
      'Full sequence begins and ends with the exact approved neutral pixels.',
      'Opaque review background; no runtime or gameplay changes.']};
  await fs.writeFile(path.join(root,'animation.json'),JSON.stringify(manifest,null,2)+'\n');
  const html=await fs.readFile(path.join(root,'preview-template.html'),'utf8');
  await fs.writeFile(path.join(root,'index.html'),html.replace('/* CLIPS */',`const clips=${JSON.stringify(clips)};const names=${JSON.stringify(names)};`));
  console.log(JSON.stringify({uniqueFrames:frames.length,clips:Object.fromEntries(Object.entries(clips).map(([k,v])=>[k,{steps:v.length,durationMs:v.reduce((t,s)=>t+s[1],0)}])),native:`${width}x${height}`}));
}
main().catch(e=>{console.error(e);process.exitCode=1});
