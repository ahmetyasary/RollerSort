// Shared depth layers for the play-field art: a soft drop shadow, two-tone
// walls and blurred inner shadows, so boards read as raised tubs with
// recessed floors rather than flat outlines.
export const depthDefs=`<filter id="soft" x="-20%" y="-20%" width="140%" height="140%"><feGaussianBlur stdDeviation="4"/></filter><filter id="softer" x="-20%" y="-20%" width="140%" height="140%"><feGaussianBlur stdDeviation="2.2"/></filter>`;
export const dropShadow=d=>`<path d="${d}" transform="translate(0,7)" fill="#16213d" opacity=".24" filter="url(#soft)"/>`;
// Thick dark wall with a lighter bevel on its upper side.
export const wall=(d,dark,light,fill)=>`<path d="${d}" fill="${fill}" stroke="${dark}" stroke-width="7" stroke-linejoin="round" stroke-linecap="round"/><path d="${d}" fill="none" stroke="${light}" stroke-width="2.4" stroke-linejoin="round" stroke-linecap="round" transform="translate(0,-1.6)" opacity=".85"/>`;
// Shadow cast inward by the walls of a closed shape, strongest below top edges.
export const innerShadow=(d,id,{color='#22345a',opacity=.2,width=24,dy=8,filter='soft'}={})=>`<clipPath id="${id}"><path d="${d}"/></clipPath><g clip-path="url(#${id})"><path d="${d}" fill="none" stroke="${color}" stroke-width="${width}" opacity="${opacity}" transform="translate(0,${dy})" filter="url(#${filter})"/></g>`;
// Shadow a raised shape casts on the floor beside it; draw before the shape itself.
export const castShadow=(body,{color='#22345a',opacity=.3,dy=6}={})=>`<g transform="translate(0,${dy})" opacity="${opacity}" filter="url(#soft)">${body.replaceAll('FILL',color)}</g>`;
export const lighten={'#4b689c':'#7f9ccc','#78468f':'#a679bd','#4c699d':'#7f9ccc'};
// Perspective inner face: the band between a shape's top edge and the same shape
// lowered by `depth`. It is widest along top edges and tapers away at the corners.
export const innerFace=(d,id,face,fill,depth,extra='')=>`<clipPath id="${id}"><path d="${d}"/></clipPath><g ${extra}><path d="${d}" fill="${face}"/><g clip-path="url(#${id})"><path d="${d}" transform="translate(0,${depth})" fill="${fill}"/></g></g>`;
export const faceOf={'#4b689c':'#45608f','#78468f':'#6a3d80'};
// Play-field outline shared by every normal board. The funnel ramps end in rounded
// noses at the throat; the throat runs on below the art so it meets the belt unbroken.
export const funnelOuter='M80 276 H370 Q425 276 426 330 L434 651 Q436 685 392 696 L266 729 C254 732 253 735 253 739 L253 800 H197 L197 739 C197 735 196 732 184 729 L58 696 Q14 685 16 651 L24 330 Q25 276 80 276 Z';
