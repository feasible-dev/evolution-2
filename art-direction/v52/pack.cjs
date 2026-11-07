// Run from the repository root after generating the saved source images.
const s=require('sharp');
(async()=>{
await s('art-direction/v52/excited-source.png').resize(768,256,{kernel:'nearest'}).toFile('prototype/assets/studio-v52/gerard-excited.png');
await s('art-direction/v52/sit-source.png').resize(384,384,{kernel:'nearest'}).toFile('prototype/assets/studio-v52/gerard-sit.png');
const atlas=await s('art-direction/v52/nico-reactions-source.png').resize(512,768,{kernel:'nearest'}).toBuffer();
await s(atlas).extract({left:0,top:0,width:128,height:192}).toFile('prototype/assets/studio-v52/nico.png');
for(let reaction=0;reaction<2;reaction++){let a=[];for(let i=0;i<8;i++){const frame=reaction*8+i; a.push({input:await s(atlas).extract({left:frame%4*128,top:Math.floor(frame/4)*192,width:128,height:192}).toBuffer(),left:0,top:i*192});}await s({create:{width:128,height:1536,channels:3,background:'#08051a'}}).composite(a).png().toFile('prototype/assets/studio-v52/nico-'+(reaction?'lose':'win')+'.png');}
})();

