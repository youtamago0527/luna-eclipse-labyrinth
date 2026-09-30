# Music transition runtime QA

## Executed result — build20, 2026-09-30 08:03 JST

Root compiled and built successfully with all seven logic validation groups passing. `QA/production-audio20/production-result.json` reports passed=true, 166 checks, 390×844. This is the prior 148 checks plus all 18 MusicTransitionPlaytest checks. The subsequent complete route exercise passed after the temporary sources were destroyed and timeScale was restored. No production music logic was changed. The listening/device limitations below still apply.

## Original delivery record

Entry: `LunaEclipse.Dungeon.MusicTransitionPlaytest.Exercise(Action<bool,string> check)`. There is no automatic boot hook. Root connected the coroutine after ProductionPlaytest's `VerifyAudioSettingsIsolation`, with an extra frame after completion to allow deferred destruction. Build 20 compilation is in progress; this document does not claim an executed pass. Root will append actual results after running.

The exercise creates a temporary GameObject with the production LunaMusicPlayer and inspects only its two AudioSources. It tests early Exploration → Deep → Return transitions separated by 0.1 seconds; mute while a fade is active; master-volume changes while muted and after unmuting; settlement on exactly one latest track with expected gain/loop mode; StopMusic followed by a one-second wait without playback revival; and fade completion while Time.timeScale is zero. It also checks zero-volume silence and restoration without allocating another playback source.

The coroutine uses realtime waits and restores the original Time.timeScale in `finally`, which also destroys the temporary GameObject. It does not change PlayerPrefs, AudioListener, production music code, or the caller's existing sources. The caller owns invocation and assertion reporting. Temporary sources are isolated for measurement, not a global all-app source-count assertion.

These checks inspect source state, clip identity and volume values. They do not verify perceived audio quality, smoothness of interrupted crossfades, absence of audible clicks or volume dips, device output, or sample-perfect looping. A passed source-state check is not a listening test. This task created documentation only; no build or Unity execution was performed here.
