import fs from 'node:fs/promises';
import path from 'node:path';
import {fileURLToPath} from 'node:url';
import sharp from '../../.cache/sprite-tools/node_modules/sharp/lib/index.js';
const dir=path.dirname(fileURLToPath(import.meta.url));
const out=path.resolve(dir,'../../prototype/assets/broadcast-v38');
// Original five-column scoreboard alphabet. Each number is one five-bit row.
const rows={
 A:[14,17,17,31,17,17,17],B:[30,17,17,30,17,17,30],C:[14,17,16,16,16,17,14],D:[30,17,17,17,17,17,30],
 E:[31,16,16,30,16,16,31],F:[31,16,16,30,16,16,16],G:[14,17,16,23,17,17,15],H:[17,17,17,31,17,17,17],
 I:[14,4,4,4,4,4,14],J:[7,2,2,2,18,18,12],K:[17,18,20,24,20,18,17],L:[16,16,16,16,16,16,31],
 M:[17,27,21,21,17,17,17],N:[17,25,25,21,19,19,17],O:[14,17,17,17,17,17,14],P:[30,17,17,30,16,16,16],
 Q:[14,17,17,17,21,18,13],R:[30,17,17,30,20,18,17],S:[15,16,16,14,1,1,30],T:[31,4,4,4,4,4,4],
 U:[17,17,17,17,17,17,14],V:[17,17,17,17,17,10,4],W:[17,17,17,21,21,21,10],X:[17,17,10,4,10,17,17],
 Y:[17,17,10,4,4,4,4],Z:[31,1,2,4,8,16,31],
 '0':[14,17,19,21,25,17,14],'1':[4,12,4,4,4,4,14],'2':[14,17,1,2,4,8,31],'3':[30,1,1,14,1,1,30],
 '4':[2,6,10,18,31,2,2],'5':[31,16,16,30,1,1,30],'6':[14,16,16,30,17,17,14],'7':[31,1,2,4,8,8,8],
 '8':[14,17,17,14,17,17,14],'9':[14,17,17,15,1,1,14],
 ' ':[0,0,0,0,0,0,0],'.':[0,0,0,0,0,12,12],',':[0,0,0,0,0,4,8],':':[0,12,12,0,12,12,0],
 ';':[0,12,12,0,4,4,8],'!':[4,4,4,4,4,0,4],'?':[14,17,1,2,4,0,4],"'":[4,4,8,0,0,0,0],
 '"':[10,10,0,0,0,0,0],'-':[0,0,0,31,0,0,0],'/':[1,2,2,4,8,8,16],'%':[17,2,4,4,8,16,17],
 '+':[0,4,4,31,4,4,0],'=':[0,0,31,0,31,0,0],'(':[2,4,8,8,8,4,2],')':[8,4,2,2,2,4,8],
 '&':[12,18,20,8,21,18,13],'*':[0,21,14,31,14,21,0],'•':[0,0,4,14,4,0,0],'—':[0,0,0,31,0,0,0],
 '’':[4,4,8,0,0,0,0],'…':[0,0,0,0,0,0,21], '×':[0,0,17,10,4,10,17]
};
const glyphs=Object.fromEntries(Object.entries(rows).map(([c,r])=>[c,[0,0,...r,0,0]]));
for(const [accent,letters,mark] of [['acute','É',[2,4]],['grave','ÀÈÙ',[8,4]],['circumflex','ÂÊÎÔÛ',[4,10]],['diaeresis','ÄËÏÖÜ',[10,0]]])
 for(const c of letters)glyphs[c]=[...mark,...rows[c.normalize('NFD')[0]],0,0];
glyphs['Ç']=[0,0,...rows.C,4,8];glyphs['Œ']=[0,0,15,20,20,22,20,20,15,0,0];
const characters=Object.keys(glyphs).join(''),columns=16,cellWidth=8,cellHeight=12;
const width=columns*cellWidth,height=Math.ceil(characters.length/columns)*cellHeight;
const rects=[];
[...characters].forEach((c,i)=>glyphs[c].forEach((row,y)=>{for(let x=0;x<5;x++)if(row&(1<<(4-x)))rects.push(`<rect x="${i%columns*cellWidth+x+1}" y="${Math.floor(i/columns)*cellHeight+y}" width="1" height="1"/>`);}));
const svg=`<svg xmlns="http://www.w3.org/2000/svg" width="${width}" height="${height}" fill="white">${rects.join('')}</svg>`;
await fs.writeFile(path.join(out,'ticker-alphabet.svg'),svg);
await sharp(Buffer.from(svg)).png().toFile(path.join(out,'ticker-alphabet.png'));
const palette=['#35e6ff','#ff55bb','#ffe64b','#ff963c','#8bff76','#b497ff'];
await fs.writeFile(path.join(out,'ticker-alphabet.json'),JSON.stringify({name:'Evolution Arena Pixel',license:'Original project artwork',characters,columns,cellWidth,cellHeight,palette},null,2));
await fs.writeFile(path.join(dir,'alphabet-source.json'),JSON.stringify(glyphs,null,2));
await sharp(Buffer.from(svg)).resize(width*6,height*6,{kernel:'nearest'}).flatten({background:'#081522'}).png().toFile(path.join(dir,'alphabet-preview.png'));
console.log(`Exported ${characters.length} original glyphs, accents and six neon colors.`);
