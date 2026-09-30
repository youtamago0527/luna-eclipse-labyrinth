# Luna original-frame runtime QA

Opt-in flag: `--luna-frame-qa` (do not combine with other QA flags). Root must inject `ProductionPlaytest.Store` for this flag in LunaApp so no production progression save is accessed. This harness does not alter existing production scripts, import settings or source PNGs.

The normal LunaApp creates its normal dungeon and renderer. The harness clears enemies, blocks gameplay input with the existing modal flag, and sequentially assigns the original Resources sprites to the existing `Luna` SpriteRenderer: idle 1 + walk 4 + attack 3 for each of four directions, 32 total. It never moves or rescales the player. Every frame asserts fixed world/local position, scale and rotation; normalized pivot (.5,.1); identical PPU; enabled camera and visible renderer. JSON records actual sprite rect/pivot/PPU/bounds, transforms and camera viewport/orthographic size.

Output: `QA/luna-frames/<actual-width>x<actual-height>/`, 32 named PNGs plus `luna-frame-result.json`. Missing resources, changed transforms, missing captures, exceptions or 90-second timeout produce a failed report and exit nonzero. Root owns build and execution; creating this code is not an executed-pass claim.

Scope deliberately stops at static centre-position frames. These captures help humans compare feet, attack effects and scale at each actual screen size. They do not implement the larger audit plan's edge-cell alpha-bounds tests, moving-camera/continuous-animation recordings or subjective smoothness assessment. Original alpha bounds are not recomputed or normalized per frame. Re-run separately at desired resolutions; each resolution has its own output folder.
