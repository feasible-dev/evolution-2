const sharp=require('sharp');
const fs=require('node:fs/promises'),path=require('node:path'),vm=require('node:vm');
const root=__dirname;
const left=[[0,230],[1,200],[2,230],[3,200]],right=[[4,230],[5,200],[6,230],[7,200]],front=[[8,200],[9,200],[10,200],[11,200]];
const dance=[...left,...left,[12,160],[14,140],...right,...right,[14,200],...front,...front,...front,...front,[14,140]];
const finale=[[16,180],[17,160],[18,160],[19,170],[20,200],[21,350],[22,550],[23,180],[24,180],[25,180],[26,180],[27,350],[28,750],[29,120],[30,650],[31,1600]];
const sequence=[...dance,...finale];
const names=['Left: shoulder','Left: descending','Left: hip','Left: rising','Right: shoulder','Right: descending','Right: hip','Right: rising','Cross','Opening','Open','Closing','Turn through front','Prepare right','Gather in front','Return left','Gather','Peace signs rising','Peace signs by cheeks','Fan opening','Fan spread','Peace pose','Playful hold','Hands lowering','Hands at waist','Hands beside hips','Hands move behind back','Hands clasped behind back','Watching rink','Blink','Attentive smile','Watching down-right'];
async function gif(frames,steps,name){
 const raw=[];for(const [i] of steps)raw.push(await sharp(frames[i]).resize(384,384,{kernel:'nearest'}).ensureAlpha().raw().toBuffer());
 const out=path.join(root,name);
 await sharp(Buffer.concat(raw),{raw:{width:384,height:384*steps.length,channels:4,pageHeight:384}}).gif({delay:steps.map(s=>s[1]),loop:0,dither:0,effort:7}).toFile(out);
 const meta=await sharp(out,{animated:true}).metadata();
 if(meta.pageHeight!==384||meta.delay.reduce((n,v)=>n+v,0)!==steps.reduce((n,s)=>n+s[1],0))throw Error('GIF size/timing mismatch');
}
async function main(){
 const frames=[],source=path.join(root,'nao-finale-sheet.png'),meta=await sharp(source).metadata(),crops=[];
 await fs.mkdir(path.join(root,'frames'),{recursive:true});
 for(let i=0;i<16;i++)frames.push(await sharp(path.join(root,'nao-dance-base.png')).extract({left:i%4*192,top:Math.floor(i/4)*192,width:192,height:192}).png().toBuffer());
 for(let i=0;i<16;i++){
  const col=i%4,row=Math.floor(i/4),left=Math.round(col*meta.width/4),top=Math.round(row*meta.height/4);
  const crop={left,top,width:Math.round((col+1)*meta.width/4)-left,height:Math.round((row+1)*meta.height/4)-top};crops.push(crop);
  const tile=await sharp(source).extract(crop).resize({height:192,kernel:'nearest'}).png().toBuffer(),m=await sharp(tile).metadata();
  if(m.width>192)throw Error('Source cell too wide');
  frames.push(await sharp({create:{width:192,height:192,channels:4,background:'#101827'}}).composite([{input:tile,left:Math.floor((192-m.width)/2),top:0}]).png().toBuffer());
 }
 for(let i=0;i<frames.length;i++)await fs.writeFile(path.join(root,'frames',`${String(i).padStart(2,'0')}.png`),frames[i]);
 await sharp({create:{width:768,height:1536,channels:4,background:'#101827'}}).composite(frames.map((input,i)=>({input,left:i%4*192,top:Math.floor(i/4)*192}))).png().toFile(path.join(root,'nao-pikki-native.png'));
 await gif(frames,sequence,'nao-pikki-preview.gif');await gif(frames,finale,'nao-finale-preview.gif');
 const durationMs=sequence.reduce((n,s)=>n+s[1],0),finaleDurationMs=finale.reduce((n,s)=>n+s[1],0);
 await fs.writeFile(path.join(root,'animation.json'),JSON.stringify({character:'Nao Morita',age:20,city:'Fukuoka, Japan',status:'Animation preview; not integrated into game',generator:'Built-in imagegen finale revision; existing v13 dance frames preserved',sources:['nao-dance-base.png','nao-finale-sheet.png'],nativeFrame:{width:192,height:192},atlas:{columns:4,rows:8},crops,sequence:sequence.map(([frame,durationMs])=>({frame,durationMs})),durationMs,finaleDurationMs,finaleRange:{start: dance.length,end:dance.length+finale.length},notes:['Preserves the v13 dance pixels and choreography.','Double peace transitions to a simple relaxed smile; hands join behind the back while torso, head and eyes turn screen-right and slightly downward toward the rink.','No mouth-covering gesture or laughter. Interactive preview plays once and holds the final stance; GIF repeats for review.','Finale uses timed key poses with no programmatic body deformation.']},null,2));
 const template=await fs.readFile(path.join(root,'preview-template.html'),'utf8');
 const html=template.replace('/* TIMING */',`const sequence=${JSON.stringify(sequence)},names=${JSON.stringify(names)},finaleStart=${dance.length},finaleEnd=${dance.length+finale.length};`);
 new vm.Script(html.match(/<script>([\s\S]*?)<\/script>/)[1]);
 await fs.writeFile(path.join(root,'index.html'),html);console.log(JSON.stringify({frames:frames.length,steps:sequence.length,durationMs,finaleDurationMs}));
}
main().catch(e=>{console.error(e);process.exitCode=1});
