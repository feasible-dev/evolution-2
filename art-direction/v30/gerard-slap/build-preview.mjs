// Run from the repository root after installing the pinned tools in .cache/sprite-tools.
import fs from 'node:fs/promises';
import path from 'node:path';
import sharp from '../../../.cache/sprite-tools/node_modules/sharp/lib/index.js';
import { importPNG, renderProject, animationGIF } from '../../../.cache/sprite-tools/node_modules/pixelbolt/src/index.js';

const dir = path.dirname(new URL(import.meta.url).pathname.replace(/^\/(\w:)/, '$1'));
const source = path.join(dir, 'generated-sheet.png');
const meta = await sharp(source).metadata();
const names = ['rest', 'prepare', 'lift', 'anticipate', 'impact', 'hold', 'recover', 'rest-end'];
const times = [900, 120, 100, 180, 70, 180, 140, 650];
const report = {source: {width:meta.width,height:meta.height,alpha:meta.hasAlpha}, tools:{pixelbolt:'0.11.0',sharp:'0.34.5'}, variants:[]};
for (const size of [192,168]) {
  const tiles = [];
  for (let i=0;i<8;i++) {
    // Reuse the exact neutral image at the end to avoid an identity-changing loop seam.
    const cell = i === 7 ? 0 : i;
    const left=Math.round(cell%4*meta.width/4), right=Math.round((cell%4+1)*meta.width/4);
    const top=Math.round(Math.floor(cell/4)*meta.height/2), bottom=Math.round((Math.floor(cell/4)+1)*meta.height/2);
    const input = await sharp(source).extract({left,top,width:right-left,height:bottom-top})
      .resize(size,size,{kernel:'nearest'}).png().toBuffer();
    tiles.push({input,left:(i%4)*size,top:Math.floor(i/4)*size});
  }
  // One palette for the whole animation, no temporal dithering.
  const {data,info} = await sharp({create:{width:size*4,height:size*2,channels:4,background:'#00000000'}})
    .composite(tiles).raw().toBuffer({resolveWithObject:true});
  // Generated edge alpha needs a binary mask for crisp sprites and reliable GIF transparency.
  for(let p=0;p<data.length;p+=4) {
    data[p+3]=data[p+3]>=128 ? 255 : 0;
    if(!data[p+3]) data[p]=data[p+1]=data[p+2]=0;
  }
  const sheet = await sharp(data,{raw:info}).png({palette:true,colours:96,quality:100,effort:10,dither:0}).toBuffer();
  await fs.writeFile(path.join(dir,`sheet-${size}.png`),sheet);
  const atlas = {frames:Object.fromEntries(names.map((name,i)=>[name,{frame:{x:i%4*size,y:Math.floor(i/4)*size,w:size,h:size},duration:times[i]}])),animations:{slap:{frames:names,loop:false},review:{frames:names,loop:true}}};
  const {recipe,provenance} = importPNG(sheet,{name:`gerard-slap-${size}`,atlas});
  recipe.sheet={columns:4,padding:1,scale:1};
  await fs.writeFile(path.join(dir,`recipe-${size}.json`),JSON.stringify(recipe));
  const gif = animationGIF(renderProject(recipe),'review',{scale:2,background:'#081522'});
  await fs.writeFile(path.join(dir,`preview-${size}.gif`),gif.data);
  report.variants.push({size,paletteSize:Object.keys(recipe.palette??{}).length,provenance,gifNotes:gif.notes});
}
await fs.writeFile(path.join(dir,'verification.json'),JSON.stringify(report,null,2));
console.log(JSON.stringify(report,null,2));
