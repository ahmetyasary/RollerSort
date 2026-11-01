"""Deterministic original mono PCM assets, rendered offline (no runtime synthesis)."""
import math, random, wave, struct
from pathlib import Path
out=Path('Assets/Resources/Audio');out.mkdir(parents=True,exist_ok=True)
rate=44100
for name,duration in [('contact',.09),('seat',.15),('open',.23),('complete',.52),('victory',1.25),('fail',.65),('shuffle',.48),('magnet',.8),('click',.065),('rolling',1.0)]:
 rng=random.Random(name);samples=[];noise=0
 for i in range(int(duration*rate)):
  t=i/rate;p=t/duration;n=rng.uniform(-1,1);noise=noise*.85+n*.15
  if name in ('contact','seat','click'):
   freq={'contact':460,'seat':780,'click':850}[name];decay={'contact':72,'seat':38,'click':95}[name]
   v=(math.sin(2*math.pi*freq*t)*.25*math.exp(-decay*t)+math.sin(2*math.pi*freq*2.71*t)*.09*math.exp(-decay*1.8*t)+math.sin(2*math.pi*140*t)*(.13 if name=='contact' else .03)*math.exp(-t*40)+noise*.27*math.exp(-t*130))*min(1,t*4000)
  elif name=='open':v=(noise*.65+math.sin(2*math.pi*(540+80*p)*t)*.15)*math.exp(-p*5)*min(1,p*60)
  elif name in ('complete','victory','fail'):
   notes=([523.25,659.25,783.99] if name=='complete' else [523.25,659.25,783.99,1046.5] if name=='victory' else [392,329.63,261.63]);spacing=.10 if name=='complete' else .18;v=0
   for k,f in enumerate(notes):
    q=t-k*spacing
    if q>=0:v+=(math.sin(2*math.pi*f*q)+.22*math.sin(2*math.pi*f*2.01*q))*.17*math.exp(-q*7)*min(1,q*140)
  elif name=='shuffle':v=(noise*.6+math.sin(2*math.pi*180*t)*.08)*math.sin(math.pi*p)**2
  elif name=='magnet':v=(math.sin(2*math.pi*(280*t+260*t*t))*.16+noise*.12)*math.sin(math.pi*p)**2
  else:v=(noise*.10+math.sin(2*math.pi*96*t)*.015)*(0.65+.35*math.sin(2*math.pi*7*t)**2)
  samples.append(max(-.95,min(.95,v)))
 with wave.open(str(out/(name+'.wav')),'wb') as w:w.setnchannels(1);w.setsampwidth(2);w.setframerate(rate);w.writeframes(b''.join(struct.pack('<h',int(v*32767)) for v in samples))
print('10 original PCM sound assets generated.')
