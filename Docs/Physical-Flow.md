# Marble flow revision — 21 October 2026

The supplied close-ups exposed two concrete motion defects. Funnel marbles were being pulled to indexed triangular queue positions, so they formed a rigid pyramid and overlapped while rearranging. Conveyor admission also waited for rotating discrete sockets; the feeder frequently missed the next socket and produced alternate empty spaces.

## Implementation

`RollerMotion.cs` replaces the feeder with gravity-driven circle contacts against two sloped walls and a physical outlet. Contact positions are solved iteratively; velocity follows the corrected motion. There are no queue indices, row targets or colour-based pile positions. Source marbles emerge bottom row first, grow from their source size, and use a shorter release stagger. Pending marbles are hidden until released.

Conveyor marbles now keep continuous arc-length positions, independent of decorative belt sockets. Entry uses the actual incoming x position. Iterative contact constraints let an entering sphere push adjacent conveyor spheres apart and redistribute free space. A cubic transfer curve ends with the same position and tangent speed as the conveyor. Circular spacing remains large enough to clear the painted sphere diameter through both turns. Render interpolation between 120 Hz simulation samples remains enabled.

## Verification

The continuous arc-pitch geometry sweep passes: minimum centre distance 26.13831 for painted diameter 25.4. The headless live motion test passed: all six source boxes released, 54 marbles matched, victory, and no live belt overlap. Initial tests caught transfer overlap and fragmented-gap admission deadlock. Actual entry phase and pressure-driven conveyor contacts corrected both; 64 contact iterations passed the live overlap assertion.

Conveyor contacts permit pressure-driven rolling relative to the decorative belt. This prevents fragmented empty spaces from stopping incoming marbles. Belt capacity remains 22 and the source capacity rule is unchanged.

## Native iPhone 18 Pro Max simulator

Signed Release build installed and recorded at native 1320 × 2868 resolution. The 120-second run reported 6,900 sampled frames, mean 16.667 ms, p95 17.282 ms, worst 28.071 ms, one frame above 25 ms and none above 33 ms. One level completed, zero retries. These are simulator measurements, not a physical-device performance claim.

The recording shows packed marble trains, continuous curved belt entry and free rolling on the ramps instead of indexed triangular rows. The second level retained an unopened source because diagnostic autoplay stops opening sources above its active-marble threshold; this run does not establish multi-level autoplay completion or all possible funnel load patterns. Full first-level completion and belt clearance are covered by the separate smoke test.

Evidence: `iphone18pro-max-physical-flow.mp4`, `motion-profile-physical-flow.json`; build log `Builds/physical-flow-xcode.log`.
