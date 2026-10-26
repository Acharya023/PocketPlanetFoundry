# Pocket Planet Foundry

Unity 6 mobile idle game starter project. Open this folder in Unity Hub as an existing project using Unity 6000.6.7f1 (or a compatible Unity 6 editor). The editor setup script creates the starter scene and adds it to the build settings on first import.

## Current playable slice
- Tap the planet to mine Ore.
- Buy a Drill for automatic production and upgrade its level.
- Unlock Refinery Ridge and Launch Basin.
- Claim capped offline production.
- Launch the colony ship to reset run progress and gain permanent Stardust research.
- Local save with schema version and backup copy.

## Play
Open the project, wait for package import and script compilation, then press Play. The game builds its portrait UI at runtime. Tap the planet, buy drills, and advance toward the zone and launch thresholds. Mouse clicks work in the Editor; the UI is sized for touch.

## Project state
This is the first gameplay slice. Ads, IAP, cloud backup, real analytics, art assets, tutorial, localization, and store setup are not yet connected. Their service interfaces and requirements are recorded in the accompanying game specification. The current ad action stays unavailable rather than awarding a simulated reward.
