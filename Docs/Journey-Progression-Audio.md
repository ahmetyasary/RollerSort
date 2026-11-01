# Journey, lives, boosters and sound — 21 October 2026

Primary development and validation device remains the iPhone 18 Pro Max simulator. The iPhone 14 Pro Max is a secondary physical play-test device.

## Implemented flow

Cold launch goes directly from the Roller Sort splash into the current level if a life is available. A failed level or confirmed quit consumes one life; successful completion does not. Failure can retry or return to Journey. Journey provides current/upcoming level medals, Play, shop and settings. At zero lives Play stays locked and shows the refill countdown. Five lives maximum; one refills every 30 minutes, including time while the app is closed. The original refill deadline survives additional losses. Values use UTC and persist locally.

Magnet, Shuffle and Clean Up unlock at levels 6, 8 and 11. Each has a first-use explanation and a free charge. Inventory and tutorial completion persist. Empty inventory opens the ×3 / 1,800-coin purchase panel; insufficient coins cannot charge or grant items. Magnet targets any source box, including one behind a closed row, and sends its marbles to matching receivers as room becomes available. Shuffle moves accessible source boxes with an arc animation and updates their logical row/column coordinates. Clean Up attracts waiting marbles to matching receivers, preserving the puzzle's colour totals. These are prototype interpretations of the reference mechanics, not identical game implementations.

Full-board recovery offers a 900-coin cleanup before failure. Closing the offer consumes a life once. Settings Quit confirms a life loss. Existing shop purchase buttons remain previews and do not initiate real payments.

## Level and motion additions

Level layouts now change across levels 1–18: stepped, cross, pyramid and wider hard layouts. Levels 18+ currently reuse the hard layout with different seeded colours. Mystery covers appear from level 6. Tunnels from level 10 supply three additional source boxes and show the remaining count. Receiver queues account for those extra colours. Later chapters have a purple theme, and the hard layout has a difficulty label. These layouts have not all been certified solvable or difficulty balanced.

The earlier 120 Hz interpolated gravity/contact motion remains. An exact circular distance projection follows the iterative conveyor solver so dense belt configurations cannot retain residual overlaps. Lid lift uses eased position/rotation/fade; seating decelerates into its receiver. Praise text now fades near the end rather than starting translucent.

## Original audio

Tools/generate-audio.py produces ten original mono PCM WAV assets (about 512 KB total): contact, seat, open, complete, victory, fail, shuffle, magnet, click and rolling. Contact volume responds to impact speed; contact triggers are rate-limited. Preloaded clips and a 12-voice pool replace runtime-generated sine beeps. A quiet rolling layer fades in with active marbles and fades out when paused. Sound and haptic controls remain independent. Native Core Haptics cues remain tied to source opening, seating and completion.

## Validation

JourneyCheck verifies refill deadlines, consecutive losses, offline catch-up, 25 layout asset mappings and all ten sound assets. ProgressionCheck runs the actual game and checks upper-box magnet selection, shuffle coordinates, tunnel successor creation, cleanup tagging, single life consumption, Journey/Play transitions and the zero-life gate. SmokeCheck validates first-level completion and all live conveyor pair clearances. Native simulator screenshots and launch verification are recorded separately below after the signed Release build.

Native UI review caught and fixed a scaled Journey background exposing white margins, missing checkmark font glyph, a modal divider crossing titles at short heights, and tunnel badges colliding with the row above. Journey now keeps a full-bleed background, uses a vector checkmark, and tunnel counts fit inside the source cell. Booster assets were redrawn with gradient shading, glass and highlights. The no-lives modal countdown updates live.

Signed native simulator review confirmed direct level entry with lives, level-11 tunnel/mystery layout, Quit confirmation, one-life loss with preserved refill deadline, full-screen Journey and a zero-life cold launch that blocks Play. Evidence: `journey-simulator.png`, `level11-tunnels-simulator.png`, `no-lives-simulator.png`. The temporary simulator test state was backed up for restoration; physical-device progress was never edited. Sound preview: `roller-sound-preview.wav`.

Final native simulator run (sound enabled): 120.00 seconds, 6898 sampled frames, two levels completed, zero retries; average 16.672 ms, p95 16.710 ms, p99 16.747 ms, worst 33.332 ms, two frames above 25 ms. Simulator measurements do not establish physical-device performance. Report: `motion-profile-journey-audio.json`. Native gameplay recording: `journey-audio-gameplay.mp4` (video only; audio samples are provided separately). Original saved progression verified after profiling: level 7, 650 coins.
