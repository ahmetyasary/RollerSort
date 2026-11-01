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
// Play-field outline shared by every normal board. The ramps end in round noses
// at a throat one sphere wide, and the floor runs on below the art to meet the belt unbroken.
export const funnelOuter='M80 276 H370 Q425 276 426 330 L434 651 Q436 685 392 696 L246.5 728 A7 7 0 0 0 239.5 735 L239.5 800 H210.5 L210.5 735 A7 7 0 0 0 203.5 728 L58 696 Q14 685 16 651 L24 330 Q25 276 80 276 Z';
// The wall wraps each nose and returns outward under the ramp, leaving the throat
// itself without a stroke, so each ramp reads as a solid teardrop-tipped slab.
export const funnelOutline='M361 742 H246.5 A7 7 0 0 1 239.5 735 A7 7 0 0 1 246.5 728 L392 696 Q436 685 434 651 L426 330 Q425 276 370 276 H80 Q25 276 24 330 L16 651 Q14 685 58 696 L203.5 728 A7 7 0 0 1 210.5 735 A7 7 0 0 1 203.5 742 H89';
// Front faces of the ramp slabs, between each underside and the belt rim.
export const funnelFaces=fill=>`<path d="M239.5 735 A7 7 0 0 0 246.5 742 H361 V757 H239.5 Z M210.5 735 A7 7 0 0 1 203.5 742 H89 V757 H210.5 Z" fill="${fill}"/>`;
// Ramp fronts share the housing wall colour so they merge into its rim.
export const rampFace={'#4b689c':'#4c699d','#78468f':'#78468f'};
// Inner face running along every edge of a closed shape, so its walls read as thick.
export const innerBand=(d,id,color,width)=>`<clipPath id="${id}"><path d="${d}"/></clipPath><path d="${d}" fill="none" stroke="${color}" stroke-width="${width*2}" stroke-linejoin="round" clip-path="url(#${id})"/>`;
