const el=id=>document.getElementById(id);
const players=[];
let playing=false,last=performance.now();
function draw(p,frame){
 const w=p.variant.width,h=p.variant.height;
 p.context.clearRect(0,0,w,h);
 p.context.drawImage(p.image,(frame%4)*w,Math.floor(frame/4)*h,w,h,0,0,w,h);
}
function resize(){
 const scale=+el('scale').value;
 for(const p of players){p.canvas.style.width=(p.variant.width*scale)+'px';p.canvas.style.height=(p.variant.height*scale)+'px';p.stage.style.height=(192*scale)+'px';}
}
function showFinal(){playing=false;for(const p of players)draw(p,p.variant.finalFrame);el('status').textContent='Final poses — rink on the left, photographer on the right.';}
function replay(){for(const p of players){p.step=0;p.elapsed=0;p.done=false;draw(p,p.steps[0].frame);}playing=true;last=performance.now();el('status').textContent='Playing; each version will hold its final pose.';}
function tick(now){
 if(playing){const delta=Math.min(100,now-last)*+el('speed').value;
  for(const p of players){if(p.done)continue;p.elapsed+=delta;while(p.elapsed>=p.steps[p.step].durationMs){p.elapsed-=p.steps[p.step].durationMs;if(p.step+1===p.steps.length){p.done=true;draw(p,p.variant.finalFrame);break;}p.step++;draw(p,p.steps[p.step].frame);}}
  if(players.every(p=>p.done)){playing=false;el('status').textContent='Both endings held for comparison.';}
 }
 last=now;requestAnimationFrame(tick);
}
async function init(){
 const response=await fetch('variants.json');if(!response.ok)throw Error('Variant registry unavailable');const registry=await response.json();
 for(const v of registry.variants){
  const response=await fetch(v.directory+'/animation.json');if(!response.ok)throw Error('Animation unavailable: '+v.character);const metadata=await response.json();
  const image=new Image();image.src=v.directory+'/'+v.atlas;await image.decode();
  const article=document.createElement('article'),heading=document.createElement('h2'),label=document.createElement('p'),stage=document.createElement('div'),canvas=document.createElement('canvas'),link=document.createElement('a');
  heading.textContent=v.character+' · Level '+v.level;label.textContent=v.label;label.className='label';stage.className='stage';canvas.width=v.width;canvas.height=v.height;canvas.setAttribute('role','img');canvas.setAttribute('aria-label',v.character+' — '+v.label);link.textContent='Individual preview';link.href=v.directory+'/index.html';link.className='foot';
  stage.append(canvas);article.append(heading,label,stage,link);el('grid').append(article);
  const context=canvas.getContext('2d');context.imageSmoothingEnabled=false;
  const steps=metadata.finaleRange?metadata.sequence.slice(metadata.finaleRange.start,metadata.finaleRange.end):metadata.sequence;
  players.push({variant:v,canvas,stage,context,image,steps,step:0,elapsed:0,done:true});
 }
 resize();showFinal();el('poses').onclick=showFinal;el('replay').onclick=replay;el('scale').onchange=resize;requestAnimationFrame(tick);
}
init().catch(error=>{el('status').textContent='Could not load preview: '+error.message;console.error(error);});
