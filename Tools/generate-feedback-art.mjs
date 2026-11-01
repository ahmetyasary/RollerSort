import sharp from 'sharp';import {writeFile} from 'node:fs/promises';
async function save(name,w,h,body){let svg=`<svg xmlns="http://www.w3.org/2000/svg" width="${w}" height="${h}" viewBox="0 0 ${w} ${h}">${body}</svg>`;await writeFile(`Assets/RollerSort/ArtSource/${name}.svg`,svg);await sharp(Buffer.from(svg)).resize(w*3,h*3).png().toFile(`Assets/Resources/Art/${name}.png`);}
// Difficulty badge icon: rounded skull with soft violet shading.
await save('skull',64,64,'<path d="M32 5 C16 5 7 16 7 29 C7 38 12 44 18 47 V55 Q18 59 22 59 H42 Q46 59 46 55 V47 C52 44 57 38 57 29 C57 16 48 5 32 5 Z" fill="#f4eefe" stroke="#3b2a63" stroke-width="3.5"/><ellipse cx="22" cy="31" rx="7.5" ry="8.5" fill="#3b2a63"/><ellipse cx="42" cy="31" rx="7.5" ry="8.5" fill="#3b2a63"/><path d="M32 39 L28 46 H36 Z" fill="#3b2a63"/><path d="M26 52 V58 M32 52 V58 M38 52 V58" stroke="#3b2a63" stroke-width="3" stroke-linecap="round"/><path d="M13 22 Q17 12 28 10" stroke="#fff" stroke-width="3" fill="none" stroke-linecap="round" opacity=".8"/>');
// Expanding ring shown when a completed tray pops above the belt.
await save('pop-ring',96,96,'<circle cx="48" cy="48" r="41" fill="none" stroke="#fff" stroke-width="7"/><circle cx="48" cy="48" r="41" fill="none" stroke="#dff4ff" stroke-width="2.5" opacity=".9"/>');
// Five-point star for completion sparkles.
await save('star-white',64,64,'<path d="M32 3 L40 22 L61 24 L45 38 L50 59 L32 48 L14 59 L19 38 L3 24 L24 22 Z" fill="#fff" stroke="#e7f2ff" stroke-width="2" stroke-linejoin="round"/>');
console.log('Feedback assets: skull badge, pop ring and white star.');
