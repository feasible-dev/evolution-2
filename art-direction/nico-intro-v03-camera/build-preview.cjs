const sharp=require('sharp');
const fs=require('node:fs/promises'),path=require('node:path'),vm=require('node:vm');
const root=__dirname;
const delays=[450,200,200,140,160,180,140,140,200,230,200,150,150,150,350,800];
const names=['Rest','Notices camera','Small smile','Hand rising','Touches glasses','Adjusts glasses','Releases glasses','Greeting begins','Open-hand greeting','Small wrist wave','Wrist returns','Hand lowering','Settles','Settles toward camera','Friendly camera glance','Camera-facing rest'];
async function main(){
 const source=path.join(root,'nico-intro-sheet.png'),meta=await sharp(source).metadata(),frames=[],crops=[];
 await fs.mkdir(path.join(root,'frames'),{recursive:true});
 for(let i=0;i<16;i++){
  const col=i%4,row=Math.floor(i/4),left=Math.round(col*meta.width/4),top=Math.round(row*meta.height/4);
  const crop={left,top,width:Math.round((col+1)*meta.width/4)-left,height:Math.round((row+1)*meta.height/4)-top};crops.push(crop);
  const tile=await sharp(source).extract(crop).resize({height:192,kernel:'nearest'}).png().toBuffer(),m=await sharp(tile).metadata();
  if(m.width>128)throw Error('Unexpected cell aspect ratio');
  const frame=i<13 ? await sharp(path.join(root,'nico-rink-base.png')).extract({left:i%4*128,top:Math.floor(i/4)*192,width:128,height:192}).png().toBuffer() : await sharp({create:{width:128,height:192,channels:4,background:'#08051a'}}).composite([{input:tile,left:Math.floor((128-m.width)/2),top:0}]).png().toBuffer();
  frames.push(frame);await fs.writeFile(path.join(root,'frames',`${String(i).padStart(2,'0')}.png`),frame);
 }
 await sharp({create:{width:512,height:768,channels:4,background:'#08051a'}}).composite(frames.map((input,i)=>({input,left:i%4*128,top:Math.floor(i/4)*192}))).png().toFile(path.join(root,'nico-intro-native.png'));
 const raw=[];for(const frame of frames)raw.push(await sharp(frame).resize(256,384,{kernel:'nearest'}).ensureAlpha().raw().toBuffer());
 const gif=path.join(root,'nico-intro-preview.gif');
 await sharp(Buffer.concat(raw),{raw:{width:256,height:384*16,channels:4,pageHeight:384}}).gif({delay:delays,loop:0,dither:0,effort:7}).toFile(gif);
 const output=await sharp(gif,{animated:true}).metadata(),durationMs=delays.reduce((a,b)=>a+b,0);
 if(output.pageHeight!==384||output.delay.reduce((a,b)=>a+b,0)!==durationMs)throw Error('GIF duration or dimensions mismatch');
 await fs.writeFile(path.join(root,'animation.json'),JSON.stringify({character:'Nico Valdes',level:1,status:'Entrance animation preview; not integrated into game',generator:'Built-in imagegen',source:'nico-intro-sheet.png',reference:'current-game-portrait.png',nativeFrame:{width:128,height:192},crops,durationMs,sequence:delays.map((durationMs,frame)=>({frame,durationMs,label:names[frame]})),notes:['Notices camera, adjusts glasses, gives a small shy open-hand wave, then settles facing the viewer. The rink-facing version is preserved separately in nico-intro-v02.','Same logical portrait size as runtime Nico; no programmatic body deformation.','Preview repeats for review; proposed entrance would play once.']},null,2));
 const template=await fs.readFile(path.join(root,'preview-template.html'),'utf8');
 const html=template.replace('/* TIMING */',`const delays=${JSON.stringify(delays)},names=${JSON.stringify(names)};`);
 new vm.Script(html.match(/<script>([\s\S]*?)<\/script>/)[1]);await fs.writeFile(path.join(root,'index.html'),html);
 console.log(JSON.stringify({frames:frames.length,durationMs,native:'128x192'}));
}
main().catch(e=>{console.error(e);process.exitCode=1});
