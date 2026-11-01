import sharp from 'sharp';
import {writeFile} from 'node:fs/promises';
async function save(name,w,h,body){const svg=`<svg xmlns="http://www.w3.org/2000/svg" width="${w}" height="${h}" viewBox="0 0 ${w} ${h}">${body}</svg>`;await writeFile(`Assets/RollerSort/ArtSource/${name}.svg`,svg);await sharp(Buffer.from(svg)).resize(w*3,h*3).png().toFile(`Assets/Resources/Art/${name}.png`);}
// Match the runtime stadium exactly: straight half-length 120, radius 23.
for(const w of [326,360])await save(w===326?'belt':'belt-expanded',w,69,`<rect x="2" y="1" width="${w-4}" height="67" rx="33.5" fill="#656773" stroke="#fff" stroke-width="4"/><rect x="7" y="5" width="${w-14}" height="59" rx="29.5" fill="#90919b" stroke="#5e606c" stroke-width="2"/><rect x="38" y="29.5" width="${w-76}" height="10" rx="5" fill="#fff" stroke="#c5c6cf" stroke-width="2"/>`);
await save('belt-socket',24,24,'<circle cx="12" cy="12" r="10" fill="#737783" stroke="#a6a7af" stroke-width="1.5"/><path d="M5 14 A7 7 0 0 1 14 5" fill="none" stroke="#60646f" stroke-width="1.7" stroke-linecap="round"/>');
function rounded(points,r=6){let d='';for(let i=0;i<points.length;i++){const p=points[i],a=points[(i+points.length-1)%points.length],b=points[(i+1)%points.length];const la=Math.hypot(a[0]-p[0],a[1]-p[1]),lb=Math.hypot(b[0]-p[0],b[1]-p[1]);const t=Math.min(r,la/2,lb/2);const u=[p[0]+(a[0]-p[0])*t/la,p[1]+(a[1]-p[1])*t/la],v=[p[0]+(b[0]-p[0])*t/lb,p[1]+(b[1]-p[1])*t/lb];d+=`${i?'L':'M'}${u} Q${p} ${v} `;}return d+'Z';}
for(const cols of [2,3,4]){
 const heights=cols===4?[4,4,2,2]:Array(cols).fill(3),step=53,start=225-cols*step/2-7,bottom=647;
 // Include the 14-unit pocket allowance at an internal step as well as the outside edge.
 const points=[[26,644],[31,332],[32,310],[48,292],[81,289],[369,289],[402,292],[418,310],[419,332],[426,644],[start+cols*step+14,644],[start+cols*step+14,bottom-heights[cols-1]*50-7]];
 for(let i=cols-1;i>=0;i--){if(i>0&&heights[i]===heights[i-1])continue;const x=i===0?start:start+i*step+14;points.push([x,bottom-heights[i]*50-7]);if(i>0)points.push([x,bottom-heights[i-1]*50-7]);}
 points.push([start,644]);let wall=rounded(points);
 // Preserve the original smooth upper bowl rather than approximating its large curves.
 wall='M26 638 L31 332 Q31 289 81 289 H369 Q418 289 419 332 L426 638 '+wall.slice(wall.indexOf('Q426,644')).replace(/Z$/, 'L32 644 Q26 644 26 638 Z');
 const outer='M80 276 H370 Q425 276 426 330 L434 651 Q436 685 392 696 L268 730 Q248 735 253 751 L253 768 H197 V751 Q203 735 182 730 L58 696 Q14 685 16 651 L24 330 Q25 276 80 276 Z';
 await save(`board-${cols}`,450,790,`<defs><linearGradient id="blue" x2="0" y2="1"><stop stop-color="#9cbccb"/><stop offset="1" stop-color="#82a3bb"/></linearGradient></defs><path d="${outer}" fill="#d0d9ef" stroke="#4b689c" stroke-width="8"/><path d="${wall}" transform="translate(0,4)" fill="#6c829f" opacity=".2"/><path d="${wall}" fill="url(#blue)" stroke="#577e8e" stroke-width="4" stroke-linejoin="round"/><path d="M33 332 Q34 301 81 301 H369 Q414 301 416 332" fill="none" stroke="#acc8d1" stroke-width="13" opacity=".4"/>`);
}
