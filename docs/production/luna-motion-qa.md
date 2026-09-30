# Runtime Luna motion check

Run separately with `--luna-motion-qa`, intended at 390×844. Root must add this flag to LunaApp's in-memory `ProductionPlaytest.Store` selection before running. No real progression writes, build or Unity launch are performed by this change. New harness is in `Assets/Scripts/QA`; it follows existing Dungeon namespace runtime-initializer QA conventions.

The production GameManager and renderer create the dungeon. Enemies are cleared, then actual GameManager.Move performs four cardinal one-cell outward/return pairs at requested targetFrameRate 30 and again 60 (16 steps). Every rendered frame records player/camera positions, scale, progress, actual deltaTime and unscaledDeltaTime. Assertions cover monotonic bounded progress, no perpendicular drift, fixed scale, final grid coordinate, restored directional idle sprite, at least one intermediate position per step, and camera position matching map-boundary clamping rather than incorrectly requiring constant player/camera offset.

Output: `QA/luna-motion/WxH/motion-result.json`, fail on first exception or after 90 seconds. TargetFrameRate is a request, not proof of actual FPS: recorded deltas are the evidence. Central connected-floor round trips do not guarantee boundary clamps are exercised; each sample explicitly reports cameraClamped. Subjective animation smoothness and other device refresh rates require separate review. No executed-pass claim until root runs the integrated build.

## Build 12 observed result

Root reported 2,588 checks passed over 16 steps, with 320 intermediate samples. Both requested rates had mean measured delta approximately 0.006944 seconds (about 144 FPS), and cameraClamped was false in all samples. This verifies that execution at its observed rate, not actual 30/60 FPS or boundary-clamp coverage. VSync was a possible explanation for the requested rate not controlling playback.

## Follow-up harness adjustment

Only this opt-in QA component now sets QualitySettings.vSyncCount to zero during its run. Original VSync and targetFrameRate values are recorded/restored on completion or destruction; no persistent settings or normal-game source changes are made. Reports include per-requested-rate sample count, mean/min/max unscaled delta and observedMeanFps (reciprocal of mean delta). Samples cover actual movement observations after the existing settling delay, not an FPS guarantee. Root will preserve the prior report and execute build 13; that rerun is not claimed complete here.

## Build 13 measured result

Optional subsequent edge mode: combine `--luna-motion-qa --luna-motion-edge-qa`. Output goes to `QA/luna-motion-edge/WxH/`, preserving centre results. Preparation uses BFS/core movement to four extremal walkable cells, then production GameManager observes one adjacent floor step and its return per edge at each requested rate (16 observed steps total). At least one recorded cameraClamped sample is mandatory; a map that never activates a clamp cannot pass as edge coverage. Sprite bounds projected into the camera viewport are recorded per sample; these include transparent canvas and are observation-only, not an opaque-pixel clipping assertion. No Unity execution is claimed for this added mode.

The actual `Builds/DungeonPreview/QA/luna-motion/390x844/motion-result.json` was read after root's run: passed, 390×844, 924 checks, 16 steps and 96 intermediate samples. Original VSync count was 1; this QA run disabled it as described above.

| Requested rate | Samples | Mean delta (seconds) | Min–max delta (seconds) | Observed mean FPS |
|---|---:|---:|---:|---:|
| 30 | 73 | 0.033338893 | 0.033295799–0.033454295 | 29.994997 |
| 60 | 137 | 0.016670249 | 0.016647903–0.016740002 | 59.987106 |

The motion assertions therefore passed at measured rates close to both requested targets in this run. All recorded cameraClamped values remained false (zero clamped samples), so this does not establish movement behaviour at map-edge clamps. This result is specific to the tested build/device/resolution; subjective smoothness and other refresh rates remain outside its assertions. This documentation update did not change code or launch Unity.

## Build 15 edge-mode measured results

Both actual `Builds/DungeonPreview/QA/luna-motion-edge/<resolution>/motion-result.json` files were read and confirmed passed:

| Resolution | Checks | Steps | Intermediate samples | Clamped samples | Canvas bounds outside viewport | Measured FPS (30 / 60 requested) |
|---|---:|---:|---:|---:|---:|---:|
| 390×844 | 1,312 | 16 | 96 | 216 | 0 | 29.9969 / 59.9754 |
| 360×640 | 1,243 | 16 | 96 | 132 | 0 | 29.9939 / 59.9688 |

Each run recorded 76 samples at requested 30 and 140 at requested 60. Actual clamp activation was observed, unlike the central build 13 run. No recorded projected full-sprite canvas extended outside its camera viewport in these samples; this is an observation, not a new opaque-alpha clipping assertion.

Each resolution tested one generated map. These results do not guarantee every generated map, all movement paths, other devices or mobile hardware. The check totals also include setup paths, which differ between maps. This update only documents root-executed JSON evidence; no code was changed and Unity was not launched by this documentation task.
