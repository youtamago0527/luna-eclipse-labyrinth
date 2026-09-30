# 朝までの制作進行（2026-09-30 JST）

## 単一責任者と対象

- 親タスク `01a05172-eabe-7752-8715-7aa5c72db1cc` / `/root` がプロデューサー・統合担当。
- Unity `C:/Users/youta/UnityProjects/LunaEclipseLabyrinth` のみ。旧Web版、ARSÈNE、Macの別プロジェクトは編集禁止。
- 朝の到達目安は9:00 JST。品質目標は高く持つが「商用完成」「トルネコ超え」を未検証で宣言しない。
- 朝の報告は、遊べる実ファイル・確認済み機能・未完了・既知の問題を分ける。

## 担当と所有範囲

|担当（親配下の協働エージェント）|納品|編集可能範囲|
|---|---|---|
|`/root/combat` 今回は音楽担当|Sunoオリジナル探索・深層・帰還BGM、権利根拠|Resources/Audio/Generated、Scripts/Audio、docs/production/music.md|
|`/root/visuals` 画像担当|かわいい敵6種類以上、武器・盾・薬草・回復薬・巻物・羅針盤|Resources/Art、docs/production/art.md|
|`/root/permanent` 遺物担当|確定5系統、保存・重複受取防止・効果API・遺物画面|Scripts/Progression、Editor/RelicTests.cs、docs/production/relics.md|
|`/root` 統合担当|実際に拾う/使う/装備/帰還/再挑戦、素材接続、検証|Core、GameManager、UI、Renderer、Hub、LunaApp、共通Editor/QA|

Chromeを操作するのは音楽担当のみ。Unity Editor起動・停止・ビルドは親のみ。各担当は最初の少量納品を優先し、後から種類を増やす。既存担当範囲を変える場合は親が先に通知。

## 既存確定仕様を守る

- 恒久アイテムでHP/攻撃/防御を直接強化しない。
- 確定系統：鍛冶師の記憶（装備の最終乱数補正にLv加算）、満腹耐久、深層の羅針盤、倉庫拡張、バッグ拡張。
- 満腹100は回復不能な探索寿命。基本5行動で-1へ移行、食料による回復は作らない。
- バッグ20〜40、倉庫初期20と別管理。階層の人工上限は設けない。
- 開始可能階はmin(羅針盤Lv+1,最高到達階)。スキップ報酬の不正な重複を防ぐ。
- 通常報酬機会は20階ごと。ランク別テーブル/確率は旧指示で保留。ユーザーに仮バランスを有効にしてよいか非同期で確認中。返答前は本番抽選を勝手に有効化しない。
- 新規遺物案は既定無効の候補にできる。既存固有名/説明/画像を他ゲームからコピーしない。
- 大きなマス（横9）、ルナ本体0.85マス、追従カメラ、画面75%のダンジョン、Fog、拠点レイアウトを保護。

## 朝の受け入れ条件

1. 素材IDとゲームの敵/アイテムが一致し、床落ち武器は剣、盾は盾に見える。
2. 移動、攻撃、敵ターン、拾得、持ち物、装備/使用、階段、帰還が接続する。
3. 帰還報告、持ち帰り/倉庫、確定遺物効果と永続保存が接続する。未確定報酬は明示。
4. BGM音量、画面遷移、二重再生/音切れ/ライセンスを確認する。
5. Core120シード、実行版100移動/戦闘/拾得/階層の回帰テストに加え、保存・遺物・装備の検証。
6. 1170×2532 Game View、390×844実行版、最終WindowsビルドとソースZIPを用意する。

## 現在の状態

### 08:47 最終照合

- Downloadsの0818両ZIPはmanifestのサイズ/SHA256へ一致。ソースZIP内Assets/Packages/ProjectSettingsの298ファイルも現在ソースと全一致（相違0）。梱包後変更は制作文書のみ。
- 全担当の納品/監査完了を確認。実行版テストプロセスは残っていない。FINAL_VERIFICATION.mdへ証拠範囲と次担当への注意を記録。9時以降は新規実装せず0818版、HANDOFF、OPEN_ITEMSで最終報告しautomationを停止する。

### 08:32 ソースZIPの独立読込確認

- 0818ソースZIPをBuilds/SourceCheck-0818/LunaEclipseLabyrinthへ新規展開し、LibraryなしからUnity 6000.6.3f1でインポート・コンパイル・LunaValidation.Validateを実行。08:31:53の検査結果passed=true/errors=[]、終了コード0。元プロジェクト/配布ZIPに変更なし。
- 遺物担当の独立read-only監査でもPackages/Editorに元プロジェクト絶対パスやlocal file依存なし。同一PCのライセンス/キャッシュを使用するため別PC完全保証ではない。詳細source-package-qa.md。
- 既存のFindFirstObjectByType非推奨警告、Editor検査のAppDomain.GetAssemblies警告は残る。コンパイルエラーではなく、今回の最終局面で動作コードの一括置換はしない。
- 0818/build20のWindows/ソースZIPを最終候補として維持。検証プロセスは正常終了。未確定の報酬などを埋める新仕様は追加していない。

### 08:18 朝版候補確定（build20・0818 ZIP）

- docs/production/HANDOFF.mdに遊び方・できた範囲・検証の限界・未完成を集約。Windows ZIP直下へSTART_HERE.mdとして収録。PLAY_GUIDEとDocumentationも維持。
- 0818 ZIPをBuilds/PackageCheck-0818へ新規展開し、そのexeで166チェック/16画面/画面外ボタン0を確認。画像担当のZIP監査で必要資料あり、未確認画像・QA成果物の混入なし。source/Windows/manifestをDownloadsへ同名コピーしhash一致確認。
- Windows:93,247,211bytes/174entries、SHA256 `3ABEBDECABDF3C17EF5C6624AD66294487A74B8A81C199409482BE37CE3D6C73`。source:68,419,327bytes/336entries、SHA256 `A16C932E2ECCC788ABD2E8EA83E1F7F813C601A2F719D182BE2CFBE56708DD0C`。
- 最新の引渡し候補は0818/build20。梱包後監査結果は作業フォルダーのPLAN/package-auditへ記録し、既存ZIPは上書きしない。全起動QAは完了。9時の報告ではこのZIPとHANDOFF/OPEN_ITEMSを参照し、商用完成やiOS成功と誤認させない。通常機能はbuild19と同じでbuild20は音楽検証追加。

### 08:03 音声遷移QA（build20）

- /root/combat納品MusicTransitionPlaytestをProductionPlaytestへ接続。一時2AudioSource上で0.1秒間隔の曲変更、fade中mute/volume、StopMusic後の非復活、timeScale0のfade完了を検査。終了後にtimeScale復元・一時GO破棄。通常音声ロジックは変更なし。
- build20の7組検証・Windowsビルド成功。production-audio20は390×844で166チェック成功（従来148＋音声18）。追加テスト後の拠点〜2探索も成功。実際のスピーカー出力/聴感/切替の音量段差は検査対象外。music-transition-qa.md参照。
- build20の従来223チェック回帰も08:04:29更新のruntime-result.jsonで成功を確認（Logs/runtime-build20.log）。起動したQAは終了済み。0747 ZIPはbuild19、通常プレイ処理は同一だが今回のQA追加は未収録。朝の最終ZIPは最新記録を含め別時刻で作る。

### 07:48 配布候補更新（build19・0747 ZIP）

- Settings修正込みbuild19を0747 ZIPへ収録。新規Builds/PackageCheck-0747へ展開し、そのexeで148チェック・16画面成功、画面外ボタン0。ソース/Windows ZIPとmanifestをDownloadsへコピーしSHA256一致を確認。
- Windows ZIP 93,242,357 bytes、173 entries、SHA256 `0FB1647710C329B8871DD80881BA9CF1F6878EACE7276599E82B2D6BF2297991`。source 68,411,727 bytes、331 entries、SHA256 `84AA0CECA004488DF1502C8FE034F6DC0B4D57A2BC6E1D1DE3891EE5F5C0C424`。
- 遺物担当がREADME/OPEN_ITEMS/PLAYTEST_GUIDEと実装を照合し仕様誤記なし。final-scope-audit.md参照（これはZIP後の文書）。未確定報酬は無効を維持。
- /root/combatへ独立QA MusicTransitionPlaytest.csの追加を依頼中。急速曲切替・mute・StopMusic・timeScale0を一時GameObjectで検査するCoroutine、通常ゲームコードには変更しない。未納品/未コンパイル時点なので、次回確認してから親がProductionPlaytestへ呼び出しを結線・ビルドする。

### 07:38 設定画面修正（build19）

- 音楽担当の読み取り再監査ではfade残留や継続二重再生の明白な問題なし。短時間の連続曲変更で一瞬音量が落ちる可能性はあり、聴感保証はしない。
- build18へ実設定画面QAを追加。144チェック成功でも撮影画像でSliderの塗り/つまみの縦伸長を発見。build19でfillを薄いTrack配下、handleを高さ0Rail配下に修正し、見出しへの侵食を解消。
- build19の7ロジック群・Windowsビルド成功。390×844と360×640の各148チェック・16画面撮影成功。小画面の全16画面で画面外ボタン0、主要ゲーム操作44px未満0（設定リンク等は主要ゲーム操作集計外）。検査はvalue変更によるcallbackで、実マウスドラッグやiPhone指操作ではない。
- 詳細はsettings-qa.md。旧0724 ZIPはbuild17なのでこの修正を含まない。修正済みexeはBuilds/DungeonPreview。次の時刻別ZIPへ反映する。

### 07:24引渡し候補（build17・0724 ZIP）

- build17の223チェック回帰も成功（07:21:44）。ZIP名0724は識別ラベルで、実際の梱包・展開確認は07:22〜07:23に実施。
- `Builds/Handoff/luna-windows-playable-20260930-0724.zip` を新規展開し、展開先exeで124チェック・15画面撮影成功。全15画面で画面外ボタン0、主要ゲーム操作44px未満0。ソースZIPも作成し、両方をDownloadsへコピー済み。
- Windows ZIPへPLAN／OPEN_ITEMS／VISUAL_REVIEW／DUNGEON_REPORTを追加し、PLAY_GUIDEからの参照を監査済み。未確認画像・SourceArchive・QA成果物は除外。詳細は `package-audit.md`。
- 0724 ZIPはこの追記より前の文書状態を含む。ビルド内容はbuild17で一致し、梱包後の検証結果は現作業フォルダーに記録。未確定報酬、ミッション、買い物、iOS実機等は引き続き未完了。

### 07:22の確認（build17）

- build16の50周Soak完了：50周・150階・8,754チェック、約18分27秒。40〜50周の11連続サンプルで割当メモリ430,833,743 bytesが一定、記録資源数も50周固定。この限定試験での観測であり、全環境のリークなしを保証しない。
- build17のコンパイル・7ロジック検証群・Windowsビルド成功。390×844の通し試験124チェック成功（従来118＋音量保存隔離6）。15画面撮影、実操作ボタン経由の帰還・倉庫・持込再出発を確認。
- QA/preview/補助フラグを含む任意の`--luna-`起動は進行と音量変更をメモリへ隔離。通常起動の保存は維持。補助フラグ単独はコードレビューのみ、production QAは実行検証済み。
- ZIPの不足資料を同梱する修正も準備済み。07:22時点の0648 ZIPは旧build15であり、build17の再梱包と展開後検証はこれから。

### 07:01の継続作業（ソース修正・未ビルド）

- 50周Soak PID80196は実行継続中。元build16を上書きせず、結果は7:15に確認する。
- 保存隔離監査でQA/preview中の音量手動変更だけは実PlayerPrefsに残ることを発見。担当がLocalSettingsへSessionOnly（--luna-付き起動）時のメモリ音量辞書を追加、通常起動の音量保存は維持。rootはLunaAppの進行保存先も同じSessionOnly条件へ統一し補助flag単独でも隔離。
- ProductionPlaytestへ6検査追加：音量変更、NaN/Infinity拒否、Clamp、実PlayerPrefs key有無/値不変、隔離有効。**新ソースはSoak終了後のbuild17でコンパイル・実行検証が必要**。まだ実行済みとは報告しない。
- 0648 Windows ZIPのPLAY_GUIDE参照先OPEN_ITEMS/PLAN/VISUAL_REVIEW/DUNGEON_REPORT未収録を画像担当が検出。Package-Preview.ps1に必要文書を追加し、PLAY_GUIDEのDocumentation配下パスを明記。次のZIPで実在照合する。既存0648ZIPは未変更。
- PLANの古いSTATUS-0455を優先する案内を修正し、現在時刻付き更新/OPEN_ITEMSを優先と明記。過去履歴は残す。

### 06:45の継続作業

- build15の25周耐久が06:44:54完了。25帰還/再出発・75階・4362チェック・552.15秒で成功。帰還後world/camera0、AudioSource4、Texture66/Sprite50。managed約2.4〜2.8MBだがallocatedBytesは3周362.8MB→25周429.5MBで増加しており、漏れ不存在を断定しない。担当に読取追加監査を依頼。
- 0648 ZIP（build15）作成し `Builds/PackageCheck-0648` へ全展開して実起動。隔離保存の2周118チェック成功、画面外0/主要44px未満0。Assembly-CSharpの元build/展開品SHA256一致。最新Game View同梱sourceは68,400,497bytes/328entries、Windowsは93,222,199bytes/169entries。ZIP内PLAY_GUIDE.mdに一般向け操作説明を追加。0623 ZIPは上書きせず保持。
- メモリ増加の原因を絞るためQA担当へ50周オプションとMaterial/Mesh/Font/RenderTexture/AudioClip数の観測を追加依頼。通常ゲームに変更なし、強制GC/Unloadはしない。次buildで実行する。
- 0648の両ZIPとmanifestをDownloadsにも同名コピーしhash一致確認。現在の試遊受渡し候補は0648/build15。追加QAのみのbuild16ビルド・7組検証成功。06:50頃に `--luna-soak-qa --luna-soak-cycles=50` 開始、ログ `Logs/soak50-build16.log`、結果 `QA/soak-50/soak-result.json`。約18分想定・30分上限。7時heartbeatでまだ実行中なら重複起動/ビルドせず、7:15に確認する。

### 06:30の継続作業

- Unity Editorの実Game Viewを1170×2532で06:31:37撮影し `Screenshots/dungeon-game-view.png` を更新。親はcomputer-useでルナのEditorだけ確認、Play停止して閉じた。画像担当も目視監査、詳細portrait-qa.md。
- QA担当が実GameManagerの端往復モードを追加。4端×往復×実測30/60指定、カメラclamp発動必須、透明canvasを含むviewport boundsを記録。中央試験結果とは別フォルダに保存する。
- build15成功（今回のコード差分はQAのみ）。390×844端移動は1312チェック/16歩/96中間位置成功、clamp216sample・viewport外0。360×640も1243チェック/16歩/96中間位置、clamp132sample・viewport外0。双方実測約30/60fps。各1生成マップであり全マップ/実機の保証ではない。
- build15で25周Soak再走を06:35:39開始、PID9292（--luna-soak-qa、Logs/soak-build15.log）。旧build6結果はLogs/soak-build6-result.jsonへ保存。QA/soak/soak-result.jsonのmtimeが今回起動後であることを確認してから判定する。実行中に重複起動・ビルド上書きしない。0623ZIPはbuild14の保持済み中間版。
- 朝の未完成事項をOPEN_ITEMS.mdへ整理。実装済み検証とiOS実機未確認、報酬判断待ちを区別する。未確定報酬は引き続き無効。

### 06:16の継続作業

- build13でQA限定VSync無効化（通常ゲームは不変）し実測29.99/59.99fpsを確認。16歩、924チェック、96中間位置成功。中央往復であり端の連続追従の保証ではない。
- 配布監査で未使用 `luna-seated-v1.png` の制作元が未確認。name/GUIDとも参照なしを確認し、PNG/metaを `SourceArchive/Unverified` に保管移動。削除なし、Assetsおよび配布用ソース/Windows ZIPの対象外。現行画像は62点。
- build14はその画像除外だけでゲームコードはbuild13と同一。ビルド/7組ロジック検証成功。360×640の通しQAは118チェック/15画面成功、画面外0・主要操作44px未満0。従来223チェックも06:22:28成功。今回起動したQAはすべて終了。
- 再現可能なローカルZIP作成 `scripts/Package-Preview.ps1` を追加。許可リストによる入力、既存ZIP上書き拒否、SHA256/件数manifest、ログ/Library/QA成果/未確認SourceArchive除外。外部公開処理なし。
- `Builds/Handoff` に0623時刻の中間更新ZIP作成済み（build14）。source 68,786,620 bytes/326 entries、Windows 93,219,628 bytes/168 entries。manifestにSHA256、全entry展開読取確認済み。今後変更があれば朝版は別時刻で再作成する。Editor Game View画像はまだ05:01の履歴なので最新実行画面と混同しない。次回はGame View更新と継続的端移動の確認を優先。

### 06:01の継続作業

- 画像担当から新規下降階段 `Art/Terrain/stairs_down` を受領。原本無加工、1セルのResourceTileに接続し、従来の階段ロジック/判定は変更しない。制作プロンプトはstair-art.md。
- 音楽担当を独立QAへ再割当し、実GameManager.Moveの16歩・30/60指定時の補間/倍率/カメラ追従測定を追加。`--luna-motion-qa`はメモリ保存・無音で起動。
- 遺物担当の監査で試作候補3件の本番表示を発見。Enabledの確定5系統のみ表示する最小修正。報酬抽選は無効のまま。
- build12成功、7組ロジック検証とLunaValidation成功。実移動QAは2588チェック/16歩/320中間位置成功。ただし30/60指定の両方が実測平均0.006944秒（約144fps）なので30/60fps検証とは呼ばない。端clamp発動0回の中央試験。
- 390×844の拠点〜2周QAは118チェック成功、15画面の検査対象ボタン画面外0、主要9操作44px未満0。階段43pxの段差/下降穴を親目視。06b-stairs-approach.png参照。
- 従来223チェックもbuild12で06:10:32成功（100移動/戦闘/拾得/階段/B2）。起動したQAは終了、Editorも閉じている。0505 ZIPは旧版。次回はQA限定vSync無効で実測30/60の補間検査、最新ソース/Windowsアーカイブ、最終Game View更新が残る。

現在の状態は上記の時刻付き更新と `OPEN_ITEMS.md` を優先する。`STATUS-0455.md` は04:55時点の履歴で、現在の残作業一覧ではない。以下も進行の履歴として読むこと。

- 開始時：基本Core120シードと実行版223チェック成功、表示改修済み。
- 音楽：3用途MP3納品、権利根拠はmusic.md。LunaAppへ探索/深層/帰還接続、元拠点BGM維持。
- 画像：敵6・アイテム6納品。原PNG無加工、art-metricsによる表示範囲正規化。床落ち画像と敵種を接続済み。
- 遺物：確定5系統・保存・倉庫・出発階選択・帰還画面を接続。rankテーブルは未承認のため本番無効。
- 親：武器/盾の装備、HP回復、置く、帰還保存を接続。保存失敗時は探索を凍結して再試行できる。旧Web版は未変更。
- 統合ビルド1：Core120シード、RelicTests、InventoryVaultTests、GameplayContentTests、ProgressionIntegrationTestsが成功。Windowsビルド成功（Logs/production-build.log）。実行版の新テストはこれから。
- /root/combat に追加のProductionPlaytest（実際の画面遷移/音楽/画像/保存）の独立ファイルを依頼中。その他担当の納品完了。親だけがUnityを起動する。

### 04:55以降の更新

- 倉庫→持込選択→出発→再帰還の2周ループ実装・105チェック成功。中断持込品復元もサービス検証成功。
- 地形3種類（1枚敷石/紫亀裂/石壁）を新規生成して接続。初案2×2敷石は採用せず原本保管。
- `/root/combat` に `DungeonSoakTest.cs` の25周×3階・オブジェクト数検証を追加依頼。rootは `--luna-soak-qa` のメモリ保存接続済み。納品→最終ビルド→実行が次。
- QA実行のみ音声出力をミュート（AudioSourceと遷移は検査）。通常ゲーム音量設定は変更しない。
- Editorスクリーンショットは `--luna-preview` 付きで開くと実セーブを書き換えず確認できる。
- build-4で持込/新地形込み223チェックを再走成功（04:55）。build-5は追加Soakテスト内のPath名衝突でコンパイル失敗。担当がPathToへ修正し、build-6で再検証中。
- 05:01: build-6成功（全6組ロジックテスト含む）。最終実行版の2周ループ105チェック再成功。Editorの1170×2532 Game ViewをScreenshots/dungeon-game-view.pngへ撮影・目視確認済み。
- 25周SoakをPID84240で開始、結果はBuilds/DungeonPreview/QA/soak/soak-result.json。900秒上限。未完了なら同じテストを重複起動せず結果を待つ。Editorは--luna-previewで開いている。ビルド更新する前にEditorと実行版の使用状況を確認する。
- Downloadsへ0505名の中間ZIP作成済み: luna-unity-source-20260930-0505.zip（67.9MB）、luna-windows-playable-20260930-0505.zip（94.0MB）。商用完成ではない。朝の最終ZIPは別の時刻名で更新する。ZIP後の進行記録追記は朝版に含める。

## 運用

### 05:50の端表示・数値監査

- build10の223回帰を05:46:41に再走成功。
- 音楽/QA担当がLunaFramePlaytestに実マップ最外歩行セル4方向の攻撃12枚撮影を追加。--luna-frame-qa --luna-frame-edge-qa併用、QA/luna-frames-edgeへ保存。既存中央32枚結果は保持。
- 数値担当の監査で破損保存のint最大/最小強化値による能力値逆転を検出。親が装備加算と満腹間隔をlong中間計算＋int範囲の飽和へ変更。通常数値/バランスは不変。追加異常値テスト込みbuild11の7組検証/ビルド成功。
- build11端撮影: 390×844は263チェック・12枚、360×640は267チェック・12枚成功。全画像の透明余白込み投影boundsがcamera viewport内。実生成マップの極端歩行セルを検査した結果で、あらゆるマップ/連続動作を保証しない。親と画像担当が390の端攻撃を目視し切れなし。
- 最新実行物build11。従来223回帰を実行中（Logs/runtime-build11-regression.log）。0505 ZIPは未更新。Editorは閉じたまま。
- build11従来223回帰も05:51:02に成功。起動した全QA終了済み。360×640の下端攻撃02を親が目視し切れなし確認。
- 次工程候補：通常Animate中のカメラ相対位置/終了足元のサンプル検査（静止撮影との区別を維持）、実装済み画像の階段・壁角の見栄え改善。既存仕様を守る範囲で作業し、未承認報酬や新規戦闘仕様は追加しない。

### 05:36の小画面改善

- build9: 通常操作ボタンが44px未満になる画面だけ大きい操作配置へ切替。360×640の方向ボタンは実測44.49px、攻撃83.41×86.95px。横9マス・ルナ倍率・カメラ追従・拠点は変更なし。
- 小画面時は操作欄を132design px高くし、viewportは69.4%へ。標準390×844以上は従来の74.6%。小画面の操作性と引換えの明示的調整で、マスの縮小ではない。ログは操作欄の直前へ追従。
- build9ビルド・7組ロジック検証成功。compact360の2周108チェック成功、9つの操作ボタンすべて44px以上、画面外0。standard390再検証中。
- standard390も108チェック成功、44px以上・画面外0。build9の223回帰も05:36:44成功。
- build10では小画面用HUD/ログ文字を拡大し、全32フレーム撮影QAを統合。--luna-frame-qaはメモリ保存＋無音。実行結果は以下へ追記する。
- build10ビルド/7組検証成功。390×844の全32フレーム230チェック成功、全PNG保存。親が右/上攻撃02の剣先・斬撃を実画面で確認。中央静止フレームの確認であり、連続動作の品質保証とは区別する。
- build10 compact360-v10は108チェック成功、画面外ボタン0・9操作の44px未満0。小画面HUD/ログ拡大を目視確認。最新実行物はbuild10。build10の従来223回帰は次回再走する（直前build9は成功）。
- `/root/combat`の独立ファイルLunaFramePlaytest.csを納品受領、build10へ統合。画像担当へ実出力の抜取目視を依頼済み。現在起動したQAはすべて終了、Editorは閉じたまま。0505 ZIPは未更新。

### 05:22の検証更新

- build7で223チェック再走成功（05:18）。build8はHP回復表記の曖昧な＋を「HP8回復」へ置換、サイズ別QAレポートを追加しビルド/7組テスト成功。
- build8の360×640 / 430×932実行版は各108チェック成功、14画面の検査対象ボタンの画面外0。portrait-qa.md参照。短い360×640では操作パッド約35pxという既知制約を記録。
- build8の223チェックも05:20:52に再走成功。今回起動したQAはすべて終了済み。Editorは閉じている。
- 画像担当が全32ルナPNGとimport/描画設定を再監査。元画像端切れなし、共通倍率・pivot、歩行足元差最大1素材px。luna-animation-audit.md参照。実行時全フレーム撮影は次工程。
- READMEにiOS実機未検証・遺物報酬保留・途中保存範囲・旧ZIP区別を追記。最新WindowsはBuilds/DungeonPreview、0505ZIPは旧中間版。

### 05:15の検証更新

- build-6の25周Soak完了: 75階、4,566チェック、544.65秒で成功。ウォームアップ後Texture2D 66 / Sprite 50固定、帰還時のCamera/WorldRoot/WorldObjectは0へ戻った。詳細soak.md。一般的なメモリリーク不存在の証明ではない。
- 保存破損監査で孤立持込品、所有ID重複、完了済みActiveRunの拒否とバックアップ復元を追加。旧v1データ移行・元破損データ保持・未知種類の無害化も検証。save-audit.md参照。
- バッグのHP回復説明を短縮、能力行を拡大、装備中表示をボタンへ移して品名との重複を防止。
- build-7成功。従来6組＋VaultCorruptionTestsの7組成功。実行版2周ループ108チェック成功。非表示起動の画像は黒かったため表示起動で再走し108チェック再成功。05b-equipped-bag.pngで装備中表示・説明文の改行解消を親が目視確認。
- 0505 ZIPは旧build-6の中間保存。今回の修正は現在のUnityソースとBuilds/DungeonPreviewにあり、朝版ZIPへ収録予定。
- 次の安全な仕上げ：最終build-7以降の223チェック再走、各方向アニメーションの足元/切れの再監査、異なる縦画面比率でUI境界確認、READMEの実装/未実装と配布物の照合。ランク報酬の許可待ちを新仕様で埋めない。iOS実機検証は未実施として明示する。

15分ごとのこの会話へのフォローを設定する。変化のない通知は出さず、納品、テスト失敗、必要なユーザー操作、完了のみ知らせる。PCとアプリが動作している必要がある。新規課金/外部公開/ストア提出/権限回避は行わない。
