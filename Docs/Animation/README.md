# Journey animation

Current level uses a softly pulsing white/cyan focus ring. The route shows four medals with 160-unit spacing: current at the bottom, two upcoming medals above it, and the next preview fading behind the HUD. The old arrow, ŞİMDİ badge, and numbered Play label were removed so the layout matches the supplied reference. Hard milestones use a red medal and localized difficulty badge; level 25 is the first red Hard milestone in the reference range.

Victory awards are persisted once before the celebration; the visible coin counter catches up only as twelve animated coins arrive at the HUD. Continue is guarded against repeated input. Journey scrolls smoothly by 160 units from the completed level to the next, with input blocked until the transition settles. Direct gameplay uses a numbered level transition and fade reveal.

Regression test: JourneyAnimationCheck.Run passed coin flight, delayed HUD credit, duplicate Continue protection, 10→11 map movement, settled current halo, and direct next-level reveal. The final iPhone 18 Pro Max simulator preview is `journey-23-to-24-reference.mp4`; its settled reference frame is `journey-level-23-hard.png`. Normal launch restores the player at level 11 with 750 coins.

The 2026-10-22 motion pass also matches the supplied gameplay recording. A marble now follows its physical arc into a receiver, pops into its seat, punches the receiver and emits a five-star burst. A completed receiver uses a stronger fourteen-star burst, lift, tilt and fade. The completion gift opens with an animated progress fill. Continue presents the reward on the completed route node, sends twelve coins along curved paths to the HUD, counts the visible balance from its previous value to the persisted value, replaces the old node with a check, and scrolls the route to reveal the next level. The Play button runs a subtle diagonal gloss every 2.65 seconds.

The verified iPhone simulator capture for this pass is `video-reference-dynamics.mp4`; `video-reference-dynamics-final.png` is its settled frame. `JourneyAnimationCheck.Run` and the native simulator build both passed.

The completion reward now always leaves the coin row on the completion card and flies directly to the top-right HUD before any Journey or next-level transition begins. Twelve coins follow staggered curved paths; the visible balance increments only when each coin arrives. `card-to-hud-coin-flow.mp4` records this corrected sequence.

Gameplay audio follows the supplied recording's event rhythm: opening a box starts a short four-hit falling/contact cascade, physical collisions still add velocity-sensitive impacts, every seated marble plays a rising pop, and the third marble layers the completion sound with two bright delayed notes. This keeps sound tied to simulation events rather than a fixed soundtrack.
