import sharp from 'sharp';
const files=['splash','gameplay','rolling','victory','settings','shop'];
for(const file of files){const input=`Docs/${file}.png`;const m=await sharp(input).metadata();const width=Math.round(m.height*450/975);await sharp(input).extract({left:Math.round((m.width-width)/2),top:0,width,height:m.height}).resize(450,975).png().toFile(`Docs/mobile-${file}.png`);}
for(const [name,shots] of [['preview',['splash','gameplay','victory']],['ui-preview',['settings','shop']]]){await sharp({create:{width:shots.length*450+(shots.length-1)*20,height:975,channels:4,background:'#242452'}}).composite(shots.map((file,i)=>({input:`Docs/mobile-${file}.png`,left:i*470,top:0}))).png().toFile(`Docs/${name}.png`);}
console.log('Mobile previews exported.');
