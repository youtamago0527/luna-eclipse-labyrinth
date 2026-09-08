# Claude担当分 実装メモ（月蝕タイムアタック・ミッション）

対象ブランチ: `feature/claude-mission-ui`
担当範囲: `CLAUDE.md` に記載の通り、既存画面（`index.html` / `hub.html` / `dungeon.html` / `main.js` / `js/dungeon/**` / `assets/**`）とは独立した以下のみ。

## 追加したファイル

- `mission.html` — スマホ用ミッション確認画面の単独プレビュー
- `mission.css` — ミッション画面専用スタイル（`env(safe-area-inset-*)` 対応、拠点と同じ黒紺・青い月光・金装飾）
- `js/missions/mission-config.js` — 解放条件・発生率・報酬階・ランク区分の設定値（仮値）
- `js/missions/mission-system.js` — 純粋ロジック（DOM・保存・通信に触れない）
- `js/missions/mission-preview.js` — `mission.html` だけで挙動確認するための接続処理（プレビュー専用、本番結線には使わない）
- `tests/mission-system.test.mjs` — `mission-system.js` の単体テスト（Node標準の `node:test`）

外部フレームワーク・CDN・課金・広告・ランキング・通信・保存機能は追加していない。

## テスト結果

```
$ node --test tests/mission-system.test.mjs
# tests 29
# suites 0
# pass 29
# fail 0
# cancelled 0
# skipped 0
# todo 0
```

全29ケース合格。`node --version` は `v22.22.2` で確認（リポジトリに追加の依存関係なし、`node:test` / `node:assert/strict` のみ使用）。

さらに `mission.html` を `python -m http.server` 経由でPlaywright（Chromium）から開き、4状態（解放前／未発生／発生中／結果確定）のプレビュー切替と、解放→抽選→到達階更新→結果確定→OKで未発生へ戻る一連の操作をコンソールエラーなしで確認済み（faviconの404のみ、ゲーム動作に無関係）。

## `mission-system.js` の状態モデル

```js
{
  unlocked: boolean,        // B20F到達で true
  relicAcquired: boolean,   // 「月蝕の砂時計」を獲得済みか
  relicLevel: number,       // 0=未獲得、1=Lv.1（強化前）。強化はこのモジュールの外側（遺物強化システム）が更新する
  status: 'locked' | 'idle' | 'active' | 'result',
  activeRun: null | { startedAt: number(ms), timeLimitSec: number, floorReached: number },
  lastResult: null | { floorReached: number, rewards: RewardTier[], rank: RankThreshold },
}
```

すべての関数は状態を受け取り新しい状態を返す純粋関数（`state` を直接書き換えない）。

## 既存進行への結線ポイント（Codex側で実施）

1. **B20F初到達時**（ダンジョン進行側で最高到達階を更新する箇所）
   `isUnlockReached(maxFloorReached, MISSION_CONFIG)` が `true` になった最初のタイミングで一度だけ `unlockMission(state, MISSION_CONFIG)` を呼ぶ。恒久遺物「月蝕の砂時計」Lv.1の付与（インベントリ／遺物一覧への反映）はセーブデータ側の処理として別途行うこと。ここでは `relicAcquired`/`relicLevel` が状態として立つのみ。

2. **拠点で「帰還後、次の潜行へ進む」操作をした時**
   `canRollOccurrence(state)` が `true`（=解放済みかつ `idle`）の場合のみ `rollForNextDescent(state, MISSION_CONFIG)` を呼ぶ。`status==='active'` の間は `canRollOccurrence` が `false` を返すため、実装側で個別に「発生中は呼ばない」ガードを重複して書く必要はない。

3. **ダンジョン内で階層が変わるたび**
   `recordFloorReached(state, currentFloor)` を呼び、`activeRun.floorReached` を更新する（`status!=='active'` のときは何もしないので安全に毎回呼んでよい）。

4. **制限時間経過・帰還・死亡（全滅）のいずれかでタイムアタックを終える時**
   終了理由は3通りある。**（a）`isTimeExpired(state.activeRun, Date.now())` が `true` になった時**、**（b）プレイヤーが自発的に帰還した時**、**（c）その潜行中に全滅（ゲームオーバー）した時**。`finalizeMissionRun(state, MISSION_CONFIG)` はどの理由で呼んでもまったく同じに動く（呼び出し時点の `activeRun.floorReached` だけを見て報酬とランクを確定する、終了理由を引数に取らない設計）。**そのため死亡時にも必ず同じ関数を呼ぶこと** — 死亡だからといって報酬を取り消したり `finalizeMissionRun` を呼ばずに `idle`/初期状態へ戻したりしない。「到達済みの途中階報酬もすべて獲得」という確定仕様は、成功終了時と同じく死亡終了時にも適用される。実際の報酬付与（アイテム・ランク品の獲得処理）はセーブデータ側で `lastResult.rewards` / `lastResult.rank` を読んで反映すること。
   （※他ゲームの類似ギミック——一定階数から先は帰還でも死亡でもクエスト達成扱いになり毎回アイテムを獲得する型——を踏まえた設計。トリガーの種類を問わず「その時点の到達階で報酬確定」という単一ルールに統一しておくと、終了理由ごとに分岐を書かずに済む。）

5. **結果画面を閉じた時**
   `acknowledgeResult(state)` で `idle` へ戻し、次回の抽選ができるようにする。

6. **表示用データが欲しい時**
   毎回状態から作り直すのではなく `buildMissionViewModel(state, MISSION_CONFIG, Date.now())` を呼べば、`{ screen: 'locked'|'idle'|'active'|'result', ... }` の形で画面表示に必要な値（発生率%、残り時間、現在到達階、次の報酬階、途中報酬一覧、確定結果）がまとまって返る。`mission.html` の4パネル（`[data-screen]`）はこの `screen` 値と1:1で対応している。

## 未確定・要調整（仮値）

`js/missions/mission-config.js` に集約済み。数値以外のロジック変更は不要で、ここだけ差し替えれば挙動を調整できる。

- `timeLimitSec`（制限時間、現状180秒）
- `rewardTiers`（階層別の途中報酬内容）
- `rankThresholds`（到達階とランクの対応）
- `maxRelicLevel`（発生率が上限40%に達する強化段階数の目安。UIの強化レベル入力上限として使用）

## 未実装（担当範囲外）

- 実際のセーブデータへの保存・読込
- 「月蝕の砂時計」の強化（レベルアップ）システム本体
- 拠点・ダンジョンからの結線コード自体（本メモの結線ポイントを参照して実施）
