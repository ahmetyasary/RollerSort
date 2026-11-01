"""Deterministic original mono PCM sound effects, rendered offline (no runtime synthesis).

The palette follows measurements of the reference recording: soft, low and
rounded rather than bright. Spheres knock with a 65-160 Hz body and a 330-520 Hz
damped ring that is gone within ~40 ms; lids pop around 500-760 Hz; buttons are
an 18 ms bubble pop near 590 Hz; finished trays chime on C and G with their
octave. Hits are built from several damped modes plus a muffled transient, and
impacts ship several takes so repeated hits never play the same sample.
"""
import math, random, wave, struct
from pathlib import Path

RATE = 44100
OUT = Path('Assets/Resources/Audio')


def silence(seconds):
    return [0.0] * int(seconds * RATE)


def add(dst, src, at=0.0, gain=1.0):
    start = int(at * RATE)
    if start + len(src) > len(dst):
        dst.extend([0.0] * (start + len(src) - len(dst)))
    for i, v in enumerate(src):
        dst[start + i] += v * gain
    return dst


def modal(f0, ratios, amps, decays, seconds, rng, attack=0.0004, detune=0.0):
    """Sum of exponentially decaying partials: the ring of a struck object."""
    n = int(seconds * RATE)
    out = [0.0] * n
    for ratio, amp, decay in zip(ratios, amps, decays):
        f = f0 * ratio * (1 + rng.uniform(-detune, detune))
        if f >= RATE * 0.45:
            continue
        phase = rng.uniform(0, 2 * math.pi)
        w = 2 * math.pi * f / RATE
        k = math.exp(-decay / RATE)
        env = amp
        for i in range(n):
            out[i] += math.sin(phase + w * i) * env
            env *= k
    a = max(1, int(attack * RATE))
    for i in range(min(a, n)):
        out[i] *= i / a
    return out


class Biquad:
    """RBJ cookbook filter."""

    def __init__(self, kind, freq, q=0.707):
        w = 2 * math.pi * freq / RATE
        alpha = math.sin(w) / (2 * q)
        c = math.cos(w)
        if kind == 'lowpass':
            b = [(1 - c) / 2, 1 - c, (1 - c) / 2]
        elif kind == 'highpass':
            b = [(1 + c) / 2, -(1 + c), (1 + c) / 2]
        else:  # bandpass, constant peak gain
            b = [alpha, 0, -alpha]
        a = [1 + alpha, -2 * c, 1 - alpha]
        self.b = [x / a[0] for x in b]
        self.a = [a[1] / a[0], a[2] / a[0]]
        self.x1 = self.x2 = self.y1 = self.y2 = 0.0

    def __call__(self, x):
        y = self.b[0] * x + self.b[1] * self.x1 + self.b[2] * self.x2 - self.a[0] * self.y1 - self.a[1] * self.y2
        self.x2, self.x1, self.y2, self.y1 = self.x1, x, self.y1, y
        return y


def noise_burst(seconds, rng, centre, q, decay, attack=0.0003):
    """Filtered noise with a fast exponential decay: the contact transient."""
    bp = Biquad('bandpass', centre, q)
    n = int(seconds * RATE)
    k = math.exp(-decay / RATE)
    env = 1.0
    a = max(1, int(attack * RATE))
    out = []
    for i in range(n):
        out.append(bp(rng.uniform(-1, 1)) * env * min(1.0, i / a))
        env *= k
    return out


def sweep_noise(seconds, rng, f_from, f_to, q, shape):
    """Noise through a band-pass whose centre glides: a whoosh."""
    n = int(seconds * RATE)
    out = []
    for i in range(n):
        p = i / n
        f = f_from * (f_to / f_from) ** p
        if i % 64 == 0:
            bp = Biquad('bandpass', f, q) if i == 0 else _retune(bp, f, q)
        out.append(bp(rng.uniform(-1, 1)) * shape(p))
    return out


def _retune(bp, f, q):
    nb = Biquad('bandpass', f, q)
    nb.x1, nb.x2, nb.y1, nb.y2 = bp.x1, bp.x2, bp.y1, bp.y2
    return nb


def room(dry, wet, size=1.0, damp=0.3, tail=0.0):
    """Small Freeverb-style room: parallel damped combs into series all-passes."""
    dry = dry + [0.0] * int(tail * RATE)
    combs = [int(d * size) for d in (1116, 1188, 1277, 1356)]
    feedback = 0.74 + 0.08 * min(1.0, size)
    bufs = [[0.0] * d for d in combs]
    idx = [0] * 4
    store = [0.0] * 4
    aps = [556, 441]
    abufs = [[0.0] * d for d in aps]
    aidx = [0, 0]
    out = []
    for x in dry:
        s = 0.0
        for c in range(4):
            y = bufs[c][idx[c]]
            store[c] = y * (1 - damp) + store[c] * damp
            bufs[c][idx[c]] = x * 0.015 + store[c] * feedback
            idx[c] = (idx[c] + 1) % combs[c]
            s += y
        for a in range(2):
            b = abufs[a][aidx[a]]
            abufs[a][aidx[a]] = s + b * 0.5
            aidx[a] = (aidx[a] + 1) % aps[a]
            s = b - s
        out.append(x + s * wet)
    return out


def finish(samples, peak=.89, fade_in=.0006, fade_out=.012):
    """Gentle saturation, short edge fades and peak normalisation."""
    samples = [math.tanh(v * 1.15) for v in samples]
    n = len(samples)
    fi, fo = int(fade_in * RATE), int(fade_out * RATE)
    for i in range(min(fi, n)):
        samples[i] *= i / fi
    for i in range(min(fo, n)):
        samples[n - 1 - i] *= i / fo
    top = max(1e-9, max(abs(v) for v in samples))
    return [v * peak / top for v in samples]


def write(name, samples):
    rng = random.Random('dither' + name)
    with wave.open(str(OUT / (name + '.wav')), 'wb') as w:
        w.setnchannels(1)
        w.setsampwidth(2)
        w.setframerate(RATE)
        frames = bytearray()
        for v in samples:
            d = (rng.random() - rng.random()) / 32768  # TPDF dither
            frames += struct.pack('<h', max(-32767, min(32767, int(round((v + d) * 32767)))))
        w.writeframes(bytes(frames))


def knock(rng, f0, gain=1.0, thump=1.0, length=.09):
    """Soft sphere knock: low body thump, a damped mid ring and a muffled transient."""
    out = modal(f0, [1, 1.18, 1.52, 2.6], [1, .6, .35, .12], [75, 95, 125, 190], length, rng, attack=.0006, detune=.02)
    add(out, modal(rng.uniform(70, 115), [1, 1.9], [.9 * thump, .3 * thump], [150, 220], .05, rng, attack=.0008))
    lp = Biquad('lowpass', 1800)
    add(out, [lp(v) for v in noise_burst(.006, rng, 900, .7, 700)], 0, .5)
    return [v * gain for v in out]


def bell(rng, freq, gain=1.0, seconds=.6):
    """Soft rounded chime: fundamental with its octave and a faint fifth above."""
    ring = modal(freq, [1, 2, 3.01], [1, .8, .12], [11, 16, 30], seconds, rng, attack=.002)
    return [v * gain for v in ring]


def contact(rng):
    out = knock(rng, rng.uniform(380, 470))
    lp = Biquad('lowpass', 2600)
    return finish(room([lp(v) for v in out], .05, .55, .55, .03))


def seat(rng):
    out = silence(.14)
    f0 = rng.uniform(430, 520)
    add(out, knock(rng, f0, 1.0, 1.1))
    add(out, knock(rng, f0 * 1.04, .28, .5, .05), .045)
    lp = Biquad('lowpass', 2800)
    return finish(room([lp(v) for v in out], .06, .6, .55, .04))


def open_box(rng):
    out = silence(.16)
    n = int(.08 * RATE)
    pop, phase = [], 0.0
    for i in range(n):  # lid popping free: a short downward glide into the box body
        t = i / RATE
        f = 520 + 240 * math.exp(-t * 160)
        phase += 2 * math.pi * f / RATE
        pop.append(math.sin(phase) * math.exp(-t * 60) * min(1, i / 25))
    add(out, pop)
    add(out, modal(560, [1, 1.31, 0.42], [.5, .35, .45], [70, 90, 65], .08, rng, attack=.0008, detune=.03))
    lp = Biquad('lowpass', 1600)
    add(out, [lp(v) for v in noise_burst(.012, rng, 800, .6, 300)], 0, .4)
    return finish(room(out, .07, .6, .55, .04))


def click(rng):
    n = int(.05 * RATE)
    out, phase = [], 0.0
    for i in range(n):  # bubble pop: quick rise onto ~590 Hz, gone in under 20 ms
        t = i / RATE
        f = 590 * (1 - .18 * math.exp(-t * 400))
        phase += 2 * math.pi * f / RATE
        out.append(math.sin(phase) * math.exp(-t * 230) * min(1, i / 18))
    add(out, modal(409, [1, 1.16, 4.4], [.28, .2, .15], [220, 260, 300], .05, rng))
    return finish(room(out, .04, .5, .55, .02), peak=.8)


C4, E4, G4, C5, E5, G5, C6 = 261.63, 329.63, 392.0, 523.25, 659.25, 783.99, 1046.5


def chime(rng, freq, gain=1.0, seconds=.6):
    """Tray chime note: the octave rings longer than the fundamental, as in the reference."""
    return [v * gain for v in modal(freq, [1, 2, 4.01], [.8, 1, .08], [16, 9, 30], seconds, rng, attack=.003)]


def complete_chord(rng, notes):
    """Measured on the reference: a bright bubble pop, then a fast arpeggio doubled
    at the octave (+80, +140, +240 ms) whose top note rings for ~0.4 s."""
    out = silence(.8)
    n = int(.06 * RATE)
    pop, phase = [], 0.0
    for i in range(n):
        t = i / RATE
        f = 950 + 650 * (1 - math.exp(-t * 70))
        phase += 2 * math.pi * f / RATE
        pop.append(math.sin(phase) * math.exp(-t * 55) * min(1, i / 40))
    add(out, pop, 0, .55)
    lp = Biquad('lowpass', 2400)
    add(out, [lp(v) for v in noise_burst(.04, rng, 1300, .8, 110)], 0, .35)
    for (at, gain, seconds), f in zip(((.08, .62, .5), (.14, .62, .5), (.24, .7, .65)), notes):
        add(out, chime(rng, f, gain, seconds), at)
    add(out, modal(5220, [1], [.035], [30], .1, rng), .09)
    return finish(room(out, .16, .75, .45, .15))


F4, A4, B4, D5 = 349.23, 440.0, 493.88, 587.33
# Consecutive trays climb a ladder of chords, as measured in the reference: I, IV, V, then I with C5 on top.
COMPLETE_LADDER = ((C4, E4, G4), (F4, A4, C5), (G4, B4, D5), (E4, G4, C5))


def complete(rng):
    return complete_chord(rng, COMPLETE_LADDER[0])


def complete_up(step):
    return lambda rng: complete_chord(rng, COMPLETE_LADDER[step])


def victory(rng):
    out = silence(1.6)
    for k, f in enumerate((C5, E5, G5, C6)):
        add(out, bell(rng, f, .5, .9), k * .13)
    for f in (C5, E5, G5):
        add(out, bell(rng, f, .3, 1.1), .6)
    for k in range(4):
        add(out, modal(4800 + 300 * k, [1], [.04], [30], .1, rng), .62 + k * .05)
    return finish(room(out, .26, .9, .45, .35))


def fail(rng):
    out = silence(1.0)
    for k, f in enumerate((G4, E4, C4)):
        add(out, bell(rng, f, .55, .7), k * .17)
    return finish(room(out, .22, .9, .5, .25))


def shuffle(rng):
    dur = .45
    out = sweep_noise(dur, rng, 280, 900, 1.0, lambda p: math.sin(math.pi * p) ** 2 * .7)
    for k in range(4):
        add(out, knock(rng, rng.uniform(420, 500), .5, .6, .06), .07 + k * .08)
    lp = Biquad('lowpass', 2200)
    return finish(room([lp(v) for v in out], .1, .7, .5, .06))


def magnet(rng):
    dur = .6
    n = int(dur * RATE)
    out, phase = [], 0.0
    for i in range(n):  # warm rising hum with a gentle wobble
        p = i / n
        f = (180 + 240 * p * p) * (1 + .03 * math.sin(2 * math.pi * 9 * p))
        phase += 2 * math.pi * f / RATE
        out.append((math.sin(phase) + .3 * math.sin(2 * phase)) * math.sin(math.pi * p) ** 1.5 * .5)
    add(out, sweep_noise(dur, rng, 250, 800, 1.2, lambda p: math.sin(math.pi * p) ** 2 * .35))
    return finish(room(out, .14, .8, .5, .12), peak=.8)


def rolling(rng):
    """Kept for compatibility; the reference has no belt bed, so the game no longer plays it."""
    return [0.0] * int(.1 * RATE)


SOUNDS = {
    'contact': (contact, 4), 'seat': (seat, 3), 'open': (open_box, 2), 'click': (click, 2),
    'complete': (complete, 1), 'complete-up1': (complete_up(1), 1), 'complete-up2': (complete_up(2), 1), 'complete-up3': (complete_up(3), 1),
    'victory': (victory, 1), 'fail': (fail, 1),
    'shuffle': (shuffle, 1), 'magnet': (magnet, 1), 'rolling': (rolling, 1),
}

if __name__ == '__main__':
    OUT.mkdir(parents=True, exist_ok=True)
    count = 0
    for name, (render, variants) in SOUNDS.items():
        for v in range(variants):
            write(name if v == 0 else f'{name}-{v + 1}', render(random.Random(f'{name}{v}')))
            count += 1
    print(f'{count} original PCM sound assets generated.')
