# Display and shop correction — 21 October 2026

The supplied device screenshots showed a real regression: fitting the entire 450 × 975 board into the iPhone safe area created purple margins. The screen backgrounds also inherited rounded sprites from the generic panel helper, and unmasked lower-board content remained visible beyond the shop.

The display root now covers the physical display; the existing top HUD spacing provides the notch inset on iPhone 14 Pro Max. Screen surfaces use opaque rectangular fills and clip their contents. Opening the shop hides gameplay, and closing it restores gameplay. Settings retain the shop behind the modal and return to it when dismissed.

The shop now has its own life and currency HUD, grouped item quantities, unlimited-life durations, stacked coins, a Bundles divider, authored gold/blue card sprites and a patterned background. Product contents occupy a clipped ScrollRect. All four cards fit in the initial portrait reference viewport. Price buttons open a prototype notice and never take payment.

Closed source boxes hide their underlying open-box image until their lid opens, removing the double rim. Wide purple buttons use a wide rounded asset rather than stretching a square asset. The rounded font is a static 650-weight instance of Fredoka (Google Fonts, SIL OFL; licence retained under Assets/Resources/Fonts).

## Visual verification

`RollerSort.Editor.VisualCheck.Run` renders the actual Unity UI through Metal at 945 × 2048 into:

- `Docs/layout-gameplay.png`
- `Docs/layout-shop.png`
- `Docs/layout-settings.png`

These are offscreen Unity renders, not screenshots from the phone. The check also verifies that the shop hides gameplay and that settings → shop → gameplay navigation restores the expected screen. The rendered images were inspected and iterated to remove the last-card clipping and replace the font.

Device performance profiling reads launch arguments via NSProcessInfo on iOS; IL2CPP's managed command-line arguments did not expose the profiling argument on the earlier build. No earlier launch is counted as a completed measurement.

Final signed iOS Release build succeeded and was installed on the iPhone 14 Pro Max. The app launched successfully and logged `ROLLER_PROFILE_ARMED layout`. The device session then entered the background and terminated with exit code 0 before `ROLLER_PROFILE_COMPLETE`; no complete two-minute frame report is available. The final headless gameplay regression passed: six boxes, 54 matched marbles, victory, and no belt overlap. Visual navigation verification passed.

## Content size adjustment

Reduced gameplay, shop controls/cards, and dialog content to 90% after device feedback that the interface looked too large. Full-screen background surfaces and shop scenery retain their original coverage. Gameplay coordinates remain in the same logical space, with one shared parent transform scaling the board, marbles, sockets and trays together. The Unity visual/navigation check passed after this adjustment.
