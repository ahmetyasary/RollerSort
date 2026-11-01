# Motion pass — 21 October 2026

This pass addresses movement continuity and runtime UI work. It does not treat a successful level-completion test as proof of device smoothness.

- Render marble positions between the 120 Hz simulation samples rather than displaying only the latest sample. Moving belt sockets use the same interpolated clock.
- Keep the established 22 equally spaced belt positions and single-row stadium path.
- Preserve incoming velocity at the feeder, use damped steering instead of constant-speed position changes, and begin belt admission at the actual marble position.
- Replace the delayed horizontal snap during tray admission with one continuous quadratic curve.
- Advance the visible tray queue during the completion animation; reuse receiver, covers and seated marbles instead of destroying and rebuilding the column.
- Prewarm 54 marble actors, 36 sparkles, colour sprites and short audio clips. Cache actor body/shadow references. Reuse the colour-name table.
- Put moving sockets, marble actors, source boxes and tray columns on separate child canvases to limit static mesh rebuilding.
- Update safe-area transforms only when screen geometry changes. Stop button spring updates once the button settles.
- Start the Core Haptics engine ahead of interaction and keep it ready while enabled. Remove the duplicate belt-admission haptic; retain actual seating and completion cues. Suspend the engine in the background.

## Verification

The first headless gameplay run passed: six boxes, 54 matched marbles, victory, and no live belt overlap. Full-ring geometry sweep also passed. A second run with the final canvas, particle-pool and audio changes also passed. The full-ring minimum centre spacing was 26.61908 against a painted diameter of 25.4.

Physical measurement uses `-roller-profile`: 120 seconds of automatic gameplay, first five seconds excluded from frame percentiles, a 60 fps target, and JSON output under the app's Documents directory. Profiling does not save player progression. The `baseline` build was exported and installed before the motion edits. The final revision is `smooth`.

Device frame-time comparison is pending: iOS rejected the initial baseline launch because the phone had locked. No before/after frame-rate improvement is claimed without the device reports.

Final Unity export and signed Xcode Release build succeeded. The updated app was installed on the connected iPhone 14 Pro Max (device 00008120-0012502C2278C01E). Launch/frame-time validation remains pending device unlock. The baseline app is retained locally at Builds/MotionBaseline.app for an actual before/after comparison.
