# Dynamics, localization and balance pass

The supplied recording was reviewed in 0.5-second increments. It established the expected centered Play label, compact settings layout, immediate sound/haptic state feedback, language-safe auto sizing, balanced level pacing and readable reward feedback.

Implemented changes:

- Replaced the incomplete display font with a static 850-weight Nunito build. It contains Turkish `ç, ğ, ı, İ, ö, ş, ü` glyphs and is distributed under the included OFL license.
- Button labels use an inset auto-fit area. The Journey Play/Oyna label is anchored explicitly to the visual center of the yellow button.
- Sound and vibration controls update in place. Enabled controls retain full colour; disabled controls dim and receive a red off mark. Sound gives an audible confirmation when enabled and vibration gives a haptic confirmation when enabled.
- Level rewards now start at 50 coins, increase every five levels, and add bonuses for Hard and Very Hard milestones. Daily quest reward is 250 coins.
- Level colour generation uses a deterministic balanced bag and repairs triple colour runs. The receiver totals remain derived from the generated board so every generated level conserves its colours.

Validation: language resource test, journey/life/audio test, reward animation test and native iPhone simulator build passed. The Turkish Journey screenshot is `Builds/verification/turkish-font-game.png`.
