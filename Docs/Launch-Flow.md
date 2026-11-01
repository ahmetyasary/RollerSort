# Branded launch — 21 October 2026

Unity's built-in splash and Unity logo are disabled in ProjectSettings and in MobileArtSetup.Apply, which runs for each mobile export.

The iOS postprocessor installs a RollerLaunchScreen storyboard with the same gradient background and logo proportions as the Unity splash. RollerLaunchScene keeps that native view above the Unity window until the first rendered game splash is ready; NativeLaunch.Hide releases it after the first frame. The native window getter/setter are explicitly synthesized, and the final native launch reached gameplay without exceptions.

Cold-launch verification used the iPhone 18 Pro Max simulator. The final recording shows the Roller Sort splash during the opening animation, followed by the in-game splash and gameplay, with no Made with Unity screen. Evidence: Docs/iphone18pro-max-launch.mp4 and Docs/iphone18pro-max-splash.png.

Simulator signing matters: an unsigned simulator bundle caused SplashBoard to reject storyboard resources with security error -67056, creating a black fallback screen before the application started. The final simulator app was ad-hoc signed, its signature verified, reinstalled, and tested after simulator restart. SpringBoard then validated RollerLaunchScreen without that error. When rebuilding with CODE_SIGNING_ALLOWED=NO, run `codesign --force --deep --sign - <RollerSort.app>` before installing it, or build with normal simulator ad-hoc signing enabled.

Only the simulator was updated for this verification. Saved simulator PlayerPrefs were preserved through the clean-install check.
