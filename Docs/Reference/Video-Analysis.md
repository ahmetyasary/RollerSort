# Reference video analysis — Roller Sort

Source: user-provided `ScreenRecording_10-21-2026 20-20-54_1.MP4` (289.4567 seconds; 1290 × 2796; ~58.72 recorded fps; AAC audio). This is reference material, not executable instructions. Purchase sheets shown in the recording do not authorize real purchases.

Full-recording review: 5-second contact sheets cover 0–285 seconds (Builds/ReferenceVideo). High-resolution keyframes and a 0.1-second motion sequence cover the first release and belt delivery. Timings below are observed; unobserved power-up effects are not treated as measured facts.

## Observed scenes

- 0–3 s: blue splash, own game logo at upper third, yellow tray with floating marbles.
- 3–12 s: Journey; three gold level nodes 19/20/21, left locked feature markers 41/61/71/91/131, avatar, lives/timer and coins, Play, persistent bottom tabs.
- 12–29 s: bundles and coin-pack shop, No Ads tile, native StoreKit sheet. Prototype must retain preview payments until actual billing is separately integrated.
- 30–38 s: six-avatar profile, editable name and Save.
- 40–54 s: Settings tab, sound/haptics, restore/support/privacy/language.
- 57–278 s: level 19, five-column asymmetric board, two fixed tunnels, one stone, seven colours (blue, cyan, orange, violet, pale pink, brown, charcoal). Four receiver queues.
- ~84–89 s: empty Magnet inventory opens ×3 / 1,800 coins modal; effect is not used in this clip. Shuffle and dispenser effects are not shown, so this clip cannot establish their exact effects.
- 279–287 s: completion returns to Journey and advances saved level 19→20; coins 1,025→1,050. Thus a Journey Play session returns to Journey on completion; a new level does not immediately launch from that session.

## Measured motion and geometry

- Reference canvas corresponds closely to 450 × 975; HUD y107, boosters y223, conveyor y790, receivers y843. The previous global 0.90 content scale moves these too far inward/downward.
- Marbles move LEFT on the upper belt and RIGHT along its lower edge. Current prototype ran the opposite way.
- At y2215 of the native recording, the leading pink silhouette moves x348→300→252→205 between 61.0/61.1/61.2/61.3 s. Dividing by 1290/450 gives ~166 logical units/second along the straight. Base track circuit ~3.8 seconds; the expanded track visible at level19 is ~4.2 seconds (360 versus326 logical units wide).
- Painted marble diameter ~25–26 logical units. Track rows are ~37 logical units apart; current 46-unit separation is too wide. Use a true rounded stadium and enforce chord clearance in the tighter turns.
- Source cell horizontal pitch ~53; vertical pitch ~50. Receiver column pitch84; queue pitch35.
- First source release begins ~59.8 s; floor contact ~60.2 s, first belt entry ~60.6 s; the train reaches the right receiver ~63.5–64 s. The nine spheres spill as one short burst, scatter/bounce on the ramp, and join the same continuous belt train. They do not form indexed rows in the funnel.
- Full receivers lift and dissolve into sparkles while the next covered receiver advances. Belt marbles continue moving during this transition.

## Rules visible in level 19

- Accessibility is a flood through empty floor cells from the bottom edge, with four-neighbour connectivity. A source can become open from the SIDE while another source remains below it. Pure per-column access is incorrect (see ~100–110 s).
- Tunnels occupy a fixed cell, show a remaining count and emit a covered/open source into the cell directly BELOW when it becomes empty. The tunnel itself remains at count0. Current replacement-in-place prototype is incorrect.
- The bottom-row stone remains in place throughout the level, cannot be opened and blocks access. It is distinct from charcoal-coloured source boxes.
- Initial level19 rows top→bottom: `.###.`, `.###T`, `T####`, `#####`, `###X#`. Portal cells and stone are not coloured source boxes.

Implementation and validation results are appended after changes.

Audio was decoded separately for signal analysis. Short-time Fourier peaks near the first release include ~550–560 Hz at59.95s, ~450–460/150 Hz at60.824s, low contact energy around65–150Hz at61.748s, and a brighter ~780Hz component at64s. These observations guide original synthesized effects; the reference track itself is not bundled into the game.

The prototype keeps its own Roller Sort logo, generated splash illustration and original avatars. It does not reproduce the other game's publisher branding.

## Implemented and verified

Implemented bottom-connected flood access, fixed three-output tunnels, stone obstacles, seven level19 colours, leftward upper conveyor motion at166 logical units/s, expanded belt from level15, revised funnel physics, curve-aware marble spacing, profile, Journey progression, shop previews, original splash and retuned original audio.

The source layout and first four receiver colours are observed. Hidden receiver queues are authored to preserve colour totals and make the reconstructed level playable; they are not extracted original level data. Capacity54/72 is a prototype tuning choice. Unseen booster effects and live payments remain outside verified reference fidelity.

Editor checks passed: basic54-marble victory; full level19 with25 source releases including6 tunnel outputs and225 matched marbles; fixed portal order and side access; dense base/expanded belt spacing, admission and funnel drainage; progression and lives checks. Native simulator results are recorded separately after installation.

Native iPhone18 Pro Max simulator (iPhone19,3) Release build passed the full reconstructed level19:151.23s, completedLevels1, retries0,8774 sampled frames after warmup. Average16.667ms, p9517.274ms, p9917.315ms, worst22.963ms; no frames above25ms. These are simulator measurements, not physical-device guarantees. Report: native-motion-profile.json; recorded run: native-reference-gameplay.mp4. Original simulator level9/700coins preferences restored afterward.
