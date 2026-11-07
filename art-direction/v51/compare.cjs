const sharp=require('sharp');
(async()=>{
 const entries=[['broadcast-v41/gerard-conversation.png',0,0,128],['broadcast-v41/gerard-conversation.png',0,6*128,128],['broadcast-v41/rink.png',0,0,128],['broadcast-v41/excited.png',0,3*128,128],['studio-v51/gerard-standing.png',2*192,0,192],['studio-v51/gerard-standing.png',3*192,0,192],['studio-v51/gerard-standing.png',2*192,192,192]];
 const items=[];
 for(let i=0;i<entries.length;i++){
  const [p,x,y,h]=entries[i];
  const data=await sharp('../../prototype/assets/'+p).extract({left:x,top:y,width:192,height:h}).toBuffer();
  items.push({input:data,left:i*192,top:192-h});
 }
 await sharp({create:{width:192*7,height:192,channels:3,background:'#04132d'}}).composite(items).png().toFile('head-scale-review.png');
})();
