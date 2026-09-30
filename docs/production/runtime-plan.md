# Production runtime QA

Launch the portrait Windows build with `--luna-production-qa` only (do not combine with `--luna-qa`). Root owns build and execution.

`ProductionPlaytest.Store` is a static in-memory `IRelicStore`. LunaApp must inject it into both RelicService and InventoryVault before constructing the title. No real save is read or written by those services. Existing volume preferences are read, never changed. The harness exposes no automatic normal-play behaviour.

Flow: title button → hub image launch → departure button → generated dungeon → walk to generated herb and sword → pickup confirmation → inventory equip → deterministic adjacent enemy damage → inventory healing → return confirmation → results → warehouse → hub → relics → hub.

Checks: exactly one settled BGM source and correct route track after fades, title silence, 12 original art resources load, return is recorded once, bag deposits survive reconstructing services from memory, healing does not restore satiety, pickup uses one turn. Ten portrait screenshots and `QA/production/production-result.json` are written beside the player data folder. Errors and a 180-second timeout write a failed report and exit nonzero.

Isolation/limits: unrelated generated enemies are cleared; one fixed enemy supplies damage. Movement uses GameManager while screen/inventory actions invoke actual UI Button callbacks. This tests callback integration, not physical pointer hit-testing. Screenshots need human visual review. BGM checks inspect playback state after crossfades, not perceived audio quality. Deep-floor BGM and platform-specific safe-area behaviour remain separate checks. Compilation/execution is not claimed until root runs the build.
