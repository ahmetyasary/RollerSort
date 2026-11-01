# iPhone 18 Pro Max simulator — 21 October 2026

Target: iPhone 18 Pro Max, iOS 27.0, simulator UDID `0E21A74E-34B2-404B-8BC7-B62AE5964626`, ARM64 Release build, Unity 6000.6.1f1. Screen captures are 1320 × 2868 pixels.

Primary workflow: continue development and main validation on the iPhone 18 Pro Max simulator. The physical iPhone 14 Pro Max is a secondary hands-on testing target; deploying to it does not change the primary target (user instruction, 21 October 2026).

## Defect found and fixed

The first actual simulator launch reproduced a severe scale defect: most of the interface was off screen and the source boxes filled the display. The game created CanvasScaler during Start, cached a root scale while canvas.scaleFactor was still 1, and never recalculated when CanvasScaler became ready. At this display width that produced roughly threefold enlargement. Earlier offscreen UI renders manually reset the root transform and therefore failed to expose this initialization bug.

The canvas now receives the initial width scale when it is created, and the display layout also invalidates its cache whenever canvas.scaleFactor changes. `VisualCheck.CheckScaleInitialization` reproduces the delayed transition from canvas scale 1 to 3 and verifies that the final pixel scale stays correct. That regression check and the visual navigation checks passed.

The corrected native simulator app was installed and launched. Gameplay fills the background with correctly sized UI. The shop opens through the on-screen plus button, contains all four visible packages, and hides gameplay underneath.

Evidence from the running simulator:

- `Docs/iphone18pro-max-gameplay.png`
- `Docs/iphone18pro-max-shop.png`
- `Docs/iphone18pro-max-flow.png`
- `Docs/iphone18pro-max-flow.mp4`

A 120-second automatic gameplay run is recorded separately in `Builds/simulator-profile.log`. Simulator frame timings do not establish physical iPhone performance. Tactile feel cannot be verified on the simulator.

Rebuild using `RollerSort.Editor.MobileBuild.ExportIOSSimulator`, then build `Builds/iOSSimulator/Unity-iPhone.xcodeproj` with the iphonesimulator SDK. The export restores the project's device SDK and architecture settings afterward.

## Completed gameplay measurement

120.007 seconds; 6,897 sampled frames after the first five seconds; two completed levels; zero retries. Average frame time 16.674 ms, p95 16.721 ms, p99 17.033 ms, worst 33.377 ms. Three frames exceeded 25 ms and two exceeded 33.34 ms. Twenty generation-zero garbage collections were counted. Raw report: `Docs/iphone18pro-max-profile.json`. Player progression was restored after the run.
