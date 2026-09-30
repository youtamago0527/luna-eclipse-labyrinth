# Luna BGM production — 2026-09-30

## Delivered files

All three are original, text-prompted Suno v6 instrumentals. No artist, existing song, reference audio, cover, voice model, or remix was used. Generation and download were performed in Chrome through Suno's own UI. The Publish/Share actions were not used.

| Use | Resource filename under `Assets/Resources/Audio/Generated` | Length | Source |
|---|---|---:|---|
| Quiet moonlit exploration | `luna-moonlit-footsteps.mp3` | 164.360 s | https://suno.com/song/7a53278e-48b8-400d-b1a2-0796cc116f61 |
| Tense deeper exploration | `luna-beneath-the-eclipse.mp3` | 147.720 s | https://suno.com/song/8bffbe4b-91fd-46d9-8aab-bff63890b51f |
| Return / modest achievement cue | `luna-home-under-moonlight.mp3` | 13.120 s | https://suno.com/song/53f3df5c-1e10-40de-8757-186a9db6a4c1 |

The originals remain in `C:/Users/youta/Downloads/` with their English track titles. Project copies are byte-for-byte copies; source metadata, embedded artwork, and Suno attribution/IDs remain intact. No trimming, stream capture, watermark removal, or re-encoding was performed.

## Generation / authorized download record

- Account screen https://suno.com/account showed **current Pro plan**, monthly billing, next billing date **2026-10-17** before and after creation.
- Initial balances: **2310 generation credits / 20 Downloads**.
- Final balances: **2280 generation credits / 17 Downloads**, confirmed at approximately **2026-09-30 04:28 JST**.
- Used 30 existing generation credits (three requests producing two candidates each) and three existing official Download allocations. No purchase, subscription change, or new agreement was accepted.
- Suno song pages displayed generation dates **2026-09-30 04:22 JST** for exploration/deep and **04:23 JST** for return.
- Official MP3 download metadata timestamps: exploration `2026-09-29T19:24:10Z`; deep `2026-09-29T19:25:35Z`; return `2026-09-29T19:26:55Z`. These metadata times correspond to the download/export step, not the earlier UI generation minute.
- Each download used **More options → Download → MP3 → Unlock & Download**. UI quota decreased 20→19→18→17. Only the three selected songs are included in Unity.

Other generated candidates remain on Suno and were not downloaded or bundled:

- Exploration alternate: `ac51cfd4-56bc-400d-84cc-30be0ccce324`
- Deep alternate: `964358eb-5efe-4689-a363-df90c311714d`
- Return alternate: `db395bcd-864f-44c7-8150-49b8a66dc2e1`

## License basis checked on creation date

Official terms: https://suno.com/terms — sections **Pro and Premier Accounts**, **Permitted Commercial Use**, and **Remixes**.
Official download-policy explanation: https://suno.com/blog/suno-updates-tos

Current terms require a permitted Download through Suno's approved channel, within the applicable tier's allocation, for commercial use. Creating a song alone is not sufficient. The selected originals were both generated under the visibly active Pro plan and downloaded through that official mechanism. Based on that evidence, these three files satisfy the checked Suno commercial-use prerequisites for inclusion in this game's build, subject to the remaining service/platform terms. Suno does not guarantee that copyright vests or that generated music is unique. This is a recorded implementation decision, not a guarantee of exclusive copyright.

Do not substitute undownloaded alternate candidates or a collaborative Remix under this record. Preserve the source metadata and this record with project handoff. Existing user-provided music is unaffected.

## Prompts

Lyrics were left blank as the UI instructed for instrumentals.

1. `Instrumental dark fantasy exploration, 72 BPM, moonlit stone ruins, celesta, felt piano, soft strings, airy pads, subtle pulse, gentle dynamics, sparse melody, seamless loop feel, no vocals or big climax`
2. `Instrumental dark fantasy deep dungeon, 96 BPM, low strings, muted piano ostinato, glass bells, restrained frame drum pulse, tense moonlit mystery, steady low-fatigue loop, no vocals, no jump scares, no climax.`
3. `Short instrumental game return cue, 35 seconds, warm piano, celesta and gentle strings, moonlit sanctuary, quiet relief after danger, small uplifting resolution, restrained dynamics, no vocals, clean soft ending.`

The selected return cue is 13.120 seconds despite the requested 35 seconds; it fits a short results transition. Exploration tracks were prompted for restrained repetition but are not sample-edited seamless loops.

## Technical verification and integration

- All MP3s decoded completely with FFmpeg without decode errors: **48 kHz, stereo**.
- Exploration mean / peak: **−18.0 / −3.3 dBFS**.
- Deep mean / peak: **−17.9 / −3.1 dBFS**.
- Return mean / peak: **−17.3 / −3.7 dBFS**.
- No technical clipping indicated by these measured peaks. Auditory final review in the actual game mix remains necessary; the automation did not perform a human listening assessment.
- Recommended initial AudioSource gains: exploration **0.30**, deep **0.23**, return **0.35**. Lower against gameplay SE if needed.
- Recommended Unity import: streaming for the two long BGM clips, compressed in memory for the short cue; music AudioSources must remain spatialBlend=0.
- New optional helper: `Assets/Scripts/Audio/LunaMusicPlayer.cs`, namespace `LunaEclipse.Audio`. Add once to an active GameObject, call `Play(LunaMusicPlayer.Mood.Exploration/Deep/Return)`, `SetVolume(0..1)`, `SetMuted(bool)`, `StopMusic()`.
- Helper reuses two AudioSources and crossfades track changes over 0.8 seconds; exploration/deep repeat, return plays once. It does not auto-start, alter scenes, or touch the existing BGM. Root owns connection to game screens and should stop existing background music before using it to avoid double playback.

## SHA-256

- `luna-beneath-the-eclipse.mp3`: `2535CFAB843EC9C1902D59593CC1B87E5E2CED50AAACB32949649E8944284E3C`
- `luna-home-under-moonlight.mp3`: `A61F2830750EBD403584D2EDB24C975AB313EEB8FAC4F2C4E4BFA6C6C5BB15CC`
- `luna-moonlit-footsteps.mp3`: `02CD1E39918A7B516D7DF57DCC9F4671180334587B470362348E0BF4D460D01C`
