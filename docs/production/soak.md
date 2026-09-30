# Bounded dungeon soak test

Extended mode: `--luna-soak-qa --luna-soak-cycles=50` runs 50 cycles/150 floor visits, writes to separate `QA/soak-50/`, and allows 1,800 seconds. Only exact values 25 and 50 are supported; missing or invalid values use the original 25-cycle/900-second behaviour and `QA/soak/`. Each snapshot additionally records all loaded Material, Mesh, Font, RenderTexture and AudioClip object counts to help identify growth categories. These counts are diagnostic observations, not causal claims or new hard limits. No forced garbage collection or resource unload was added. The extended run has not been executed by this source change.

Opt-in player flag: `--luna-soak-qa`. Do not combine with other QA flags. Root must select `ProductionPlaytest.Store` for relic and vault constructors for this flag before app boot. No Unity launch or real save writes are performed by this source author.

The harness starts at title, then repeats 25 hub → departure → dungeon → results → hub cycles. Each expedition visits B1, B2 and B3, walking both stair paths using GameManager movement and waiting for each animation/turn. Generated enemies are cleared to isolate resource lifecycle. A generated herb is picked up via GameManager and full-health use is verified as a non-consuming rejection. This is not an enemy AI stress test.

After each return and three destruction frames, camera, world-layer object and AudioSource counts must equal the initial hub baseline, and no GameManager may remain. Live Texture2D and Sprite populations may grow by at most 16 beyond the third completed cycle to permit bounded resource/font/art caching; no force-unload or garbage collection is used to disguise lifetime issues. Start/end allocated and managed memory and all cycle samples are recorded. Passing establishes only these bounded observations, never a general leak-free guarantee.

Output: `QA/soak/soak-result.json` beside player data. Reports duration, completed cycles, floor visits, assertion count, per-cycle object/resource/memory figures and failure details. First error or 900-second timeout exits nonzero. Expected run length is several minutes due to real movement animations; root owns compilation/execution and should run asynchronously while remaining responsive.

## Recorded execution result

Root executed and reviewed the soak report: **passed**, 25 completed cycles, 75 floor visits and 4,566 checks in 544.65 seconds (about 9 minutes 5 seconds), within the 900-second budget. After warmup, the observed live Texture2D count stayed at 66 and Sprite count stayed at 50. Passing includes the configured return-to-baseline camera, world-object and AudioSource assertions.

These figures describe this specific bounded execution. They do not prove absence of every memory leak, long-term stability on other devices, or production combat balance. Consult the generated JSON for per-cycle memory measurements; no unreported memory delta is inferred here. The result was supplied by root; this documentation update did not launch Unity, modify QA code, or rerun the test.

## Build 15 rerun and memory caveat

Read directly from `Builds/DungeonPreview/QA/soak/soak-result.json` after root's run (finished 06:44:54 per root): passed, 25 cycles, 75 floor visits, 4,362 checks, 552.1527 seconds. At cycles 3 and 25, cameras/world roots/world-layer objects were zero, AudioSources four, Texture2D objects 66 and Sprites 50. Counts passed their configured bounds throughout the run.

Allocated memory increased from 362,807,375 bytes at cycle 3 to 429,457,487 at cycle 25 (increase 66,650,112 bytes, about 66.65 decimal MB). The separately sampled final report value was 429,981,775 bytes. Managed memory over cycles 3–25 ranged from 2,379,776 to 2,797,568 bytes. Stable object counts and this managed range do not explain the allocated-memory growth; do not describe the run as leak-free or assign the cause to native, font, audio, or caching without profiling evidence.

Read-only lifetime review: DungeonRenderer registers its generated textures/sprites/tiles and destroys them plus its world root in OnDestroy; DungeonUI destroys its minimap texture; LunaApp destroys outgoing screen children and unregisters its LocalSettings event. ArtLibrary retains one Sprite per resource key in a static cache; UiKit retains a single Font; LunaMusicPlayer maintains two persistent sources and StopMusic stops but does not clear their clip references. Those retained resources are bounded by the currently used keys/clips and do not by themselves identify a per-cycle leak. No clear unbounded renderer/UI/audio lifetime defect was established from this code review. A follow-up allocation/object-type profile or longer plateau measurement is needed to investigate the observed increase. No code changes or Unity launch were made for this update.

## Completed 50-cycle follow-up

Read directly from `Builds/DungeonPreview/QA/soak-50/soak-result.json`: passed, 50 cycles, 150 floor visits, 8,754 checks, 1,107.0981 seconds (18 minutes 27 seconds). No forced GC or unload was used.

Allocated memory (decimal bytes):

| Cycle | Allocated bytes |
|---:|---:|
| 0 | 219,086,927 |
| 3 | 392,757,327 |
| 25 | 428,539,983 |
| 30 | 430,112,847 |
| 39 | 430,243,919 |
| 40–50 | 430,833,743, identical at every sampled cycle |

The exact observed plateau begins at cycle 40 and lasts through cycle 50 (11 consecutive return samples); it is not accurate to claim a plateau from cycle 25 or 30. Growth from cycle 25 to 50 was 2,293,760 bytes (about 2.29 MB). Managed memory across cycles 1–50 ranged from 2,318,336 to 3,207,168 bytes, with the maximum at cycle 34; the separate final snapshot was 2,424,832 bytes.

From cycle 1 through 50, resource counts were constant: Texture2D 66, Sprite 50, Material 10, Mesh 1, Font 1, RenderTexture 0, AudioClip 7. Baseline cycle 0 had Texture2D 23, Sprite 7, Material 9 and AudioClip 2, with the other categories unchanged. These counts show no observed per-cycle object accumulation in the measured categories, but cannot identify why the allocated-memory total grew before cycle 40.

The longer run establishes a late sampled plateau under this isolated, enemy-cleared test workload, not general leak-free behaviour, every gameplay path, or device-wide memory safety. Allocation source and cache/native ownership remain unproven without a profiler attribution study. This update changed documentation only after reading the completed report.
