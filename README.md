# ルナと月蝕の迷宮 — Unity夜間統合版

## 2026-10-03 操作・持ち物更新

長押し移動、自動拾得、歩行ログ削除、10枠ごとのページ表示、カテゴリ整頓、装備時の強化値識別とマイナス装備の呪いを追加。
マイナス抽選は遺物補正前5%。既存ダンジョンBGMは進入時に再生開始。詳細・検証範囲は [COMFORT_UPDATE_20261003.md](docs/production/COMFORT_UPDATE_20261003.md)。
通常20枠は2ページ、拡張遺物の追加枠は別ページで維持する。

## Mac / TestFlight 用の取得（2026-09-30）

このUnity版は `unity/testflight-preview-20260930` ブランチです。`main` は旧Web版のため切り替え・マージしないでください。
Macの取得コマンドと未完了のiOS作業は [docs/production/MAC_TESTFLIGHT.md](docs/production/MAC_TESTFLIGHT.md) を参照。
朝の検証済みソースをGitへ登録したもので、TestFlightへアップロード済みという意味ではありません。

## 2026-09-30 制作中の統合内容

- かわいい敵6種、剣・盾・薬草・回復薬の床表示とバッグ画像。
- 武器/盾の装備、HP回復、置く、帰還結果、永続倉庫、遺物5系統の効果接続。
- 探索/深層/帰還用のSunoオリジナルBGM3曲。拠点は元の指定曲を維持。
- 通常Windows起動はタイトル→拠点→出発確認→ダンジョン。UnityのDungeonシーンは直接探索。
- 詳細と最新検証は `docs/production/PLAN.md`。商用完成版ではなく、試遊・調整用ビルド。

## 表示・スケール改修

大きなマスを維持し、ルナ本体の高さ0.85マス、描画領域の拡大、操作欄の圧縮、半透明ログ、部屋／通路／行き止まり、未探索の霧を調整。最新の画面寸法と検証は [VISUAL_REVIEW.md](VISUAL_REVIEW.md) を参照。

## 最新：ダンジョン第一段階を実装

`Assets/Scenes/Dungeon.unity` を開いてPlay、またはメニュー `Luna > Play Dungeon`。
Windows版は `Builds/DungeonPreview/LunaEclipseLabyrinth.exe` です。
移動・装備補正付きの乱数なし戦闘・拾得・バッグ・満腹度・階層移動・ミニマップを接続しました。
実装範囲・制限・Script一覧・テストは [DUNGEON_REPORT.md](DUNGEON_REPORT.md) を参照してください。
同報告は第一段階の履歴です。現在の追加実装・検証結果は `docs/production/PLAN.md` を優先してください。

操作：矢印／WASDで移動（敵がいれば攻撃）、Space/Jで攻撃、Eで拾う、ピリオドで待機。画面ボタンも同じ処理に接続。階段の上で「階段を降りる」。右上ミニマップを押すと拡大地図です。
バッグの「使う」でHP回復（満腹度は回復しません）。剣/盾は「装備」、床へ戻すなら「置く」。
帰還後の倉庫と遺物は保存します。探索途中の中断復帰は未実装のため、アプリを閉じる前に拠点へ帰還してください。

`Luna/Validate Dungeon Core` でロジック検証。実行版に `--luna-qa -force-d3d11` を渡すと独立したセッションで通し検証し、終了時に実行版の `QA` フォルダーへ結果・画面を保存します。通常のプレイでは検証は起動しません。

## プロジェクト

Unity 6.6 (6000.6.3f1)。既存Web版とは独立した2D UI移植プロジェクトです。

## 開く

Unity Hubの「追加」でこのフォルダーを選択。`Assets/Scenes/Startup.unity` を開いて再生するとタイトルから開始します。
拠点は430×932、ダンジョンは1170×2532基準。縦横比を保って余白を付け、Screen.safeArea内に操作UIを収めます。
UIはLunaAppから実行時に生成されるため、停止中のSceneには表示されません。

## 拠点と接続

- タイトル → 拠点 → ダンジョン、設定／メニュー → 戻る
- 提供された拠点背景・ルナ・ボタン画像を原本のままコピー
- BGM／ボタンSE、PlayerPrefsによる音量保存
- iOSネイティブとブラウザ向け説明の分離
- 迷宮入口で開始階を選択。帰還時に到達記録、持ち帰り品、20階ごとの報酬機会を保存
- 未承認のランク別報酬テーブルは無効。「報酬調整待ち」の機会を保持し、勝手な報酬抽選はしません
- 倉庫の「持込選択」で次の探索へ装備や薬草を持ち込めます。途中終了時は元の持込品を復元します（その探索で新しく拾った未確定品は対象外）
- 買い物、ミッション、図鑑等は未完成。既存Web版のすべてを移植した状態ではありません

既存Web版のlocalStorage保存データは変更・自動移行しません。

## 開発

`Luna/Create Startup Scene` でシーンとインポート設定を準備。
`Luna/Validate Project` で必須素材と設定を検証。
`Luna/Build Windows Preview` で `Builds/DungeonPreview` にプレビューを作成。
配布時はexeだけでなく、同じフォルダーのData・DLL等も一緒に渡してください。

### 検証・配布の制限

- iOS向けの画面分岐とsafe-area対応はありますが、iOSビルド・実機での操作/音声検証は未実施です。Windows版の成功をiOS動作保証にはできません。
- 確定5系統の遺物効果は接続済みですが、通常探索のランク別報酬は未承認のため自動付与しません。
- `--luna-qa` は移動/戦闘等、`--luna-production-qa` は拠点から2周の操作/保存、`--luna-soak-qa` は25周の耐久確認です。すべてメモリ内の隔離保存で通常セーブを書き換えません。
- `Downloads` の0505名ZIPはbuild6時点の中間版です。その後の修正は現在のプロジェクトと `Builds/DungeonPreview` にあります。朝の最終配布物は別時刻名で案内します。
- 新しいローカル試遊アーカイブは `Builds/Handoff` に時刻別で保存します。`manifest-日時.json` のSHA256で識別できます。`scripts/Package-Preview.ps1` は既存ZIPを上書きせず、ソースとWindows一式を作成します。外部公開はしません。
- 未使用で制作元未確認だった旧ルナ画像1点は `SourceArchive/Unverified` に原本保管し、実行版・ソースZIPの両方から除外しています。現在の拠点ルナ素材は変更していません。

## 素材

- 画像とBGM：既存Webプロジェクトからの提供素材。包括的な再配布ライセンスを付与するものではありません。
- 効果音：OtoLogic https://otologic.jp/ 、CC BY 4.0 https://creativecommons.org/licenses/by/4.0/ 。設定画面にクレジットを維持。
- 追加画像：`docs/production/art.md` に生成記録。追加BGM：`docs/production/music.md` に公式DL・プラン確認・利用根拠。元の提供素材とは区別。
- 日本語フォント：Noto Sans CJK JP (Noto CJK project)、SIL Open Font License 1.1。`Assets/Resources/Fonts/OFL.txt` 同梱。

元Webプロジェクト、iCloud側のファイル、既存Unityチュートリアルプロジェクトは上書きしません。
