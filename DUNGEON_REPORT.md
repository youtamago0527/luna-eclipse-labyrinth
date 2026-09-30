# ダンジョン基盤・第一段階の実装報告

表示改修後の最新仕様と画面は [VISUAL_REVIEW.md](VISUAL_REVIEW.md) を参照。

1. **Unityバージョン**：6.6 / 6000.6.3f1。
2. **場所**：`C:/Users/youta/UnityProjects/LunaEclipseLabyrinth`。既存の独立したルナUnity拠点版を拡張。ARSÈNE RPG・Web版・チュートリアルプロジェクトは変更なし。
3. **Scene**：`Dungeon.unity`（PlayでB1F開始、Windowsビルドの先頭Scene）と`Startup.unity`（タイトル→既存拠点→迷宮）。UIとワールドは実行時生成。
4. **Scriptと責務**：下記一覧。
5. **実装済み**：4方向1マス移動、移動／カメラ補間、ターンロック、敵追跡と固定ダメージ戦闘、薬草の明示拾得、20枠バッグ表示、10ターンごとの満腹減少、階段移動、探索状態とミニマップ、最新3件ログ、スマホ用ボタンとPC入力、死亡後の再挑戦。
6. **未実装**：8方向移動、薬草等の使用効果、飢餓ダメージ、本格保存、武器／防具、恒久強化、遺物、ショップ、オンライン、広告・課金。完成商品ではなくプレイ可能な第一段階です。
7. **1マス**：Unity Grid＋Tilemap、1×1 Unity Unit。独自の64px石床／壁。ルナは320×240の共通キャンバス、PPU150、共通pivot(0.5,0.1)、身体の高さを基準画像で0.85マスに正規化、Point・無圧縮。
8. **見える範囲**：横9マス×縦約14.54マス。1170×2532基準の縦画面、中央1170×1890。全マップ縮小表示はミニマップのみ。実行ウィンドウ初期430×932は同比率への縮小表示。
9. **カメラ**：Orthographic。表示領域のアスペクト比からサイズを算出。ルナと同じ補間で追従（移動0.14秒、後追い遅延なし）、マップ外側でclamp。敵表示はプレイヤー表示後に0.12秒で補間。Screen.safeArea内にUIを収める。
10. **生成**：39×45、5〜7個の部屋（幅5〜7、高さ5〜8）を1マス幅の通路で接続するRoom＋Corridor方式。短い行き止まりも追加。中央セクターから開始。シードと階数から再現可能。各階に別の地形、敵3体、薬草4個。開始位置付近は安全距離を確保。
11. **既知の制限**：iPhone実機／iOSビルド未検証。正式タイル素材ではなく視認性優先の仮素材。長時間のモバイル負荷・メモリー計測は未実施。ルナHP20攻撃2／スライムHP5攻撃1の指定固定値を使用し、旧Web版の複雑な計算式は未移植。Windows非表示起動ではキャプチャ失敗のため、表示状態のDirect3D11で実行時検証。Editor初回起動時にUnityEditor.Search.SearchDatabaseの検索インデックス処理でArgumentOutOfRangeExceptionを1件確認。ゲームはPlay可能で、Windows実行版の検証ではこの例外は発生していません。Editor側の検索エラーは未解消です。
12. **次工程**：実機で操作感確認→正式な床／壁／敵素材→8方向とアイテム使用の仕様確定→保存。数値は`DungeonRules`に集約。

## Script一覧

|場所|責務|
|---|---|
|LunaApp / UiKit|画面遷移、Safe Area、共通UI、音声|
|HubView / SettingsView / LocalSettings|既存拠点、プラットフォーム別説明、音量保存|
|Dungeon/GameManager|入力、モデルと表示の接続、効果音|
|Dungeon/TurnManager|入力ロックと表示処理の完了待ち|
|Dungeon/Core/DungeonRun|プレイヤー位置・状態、ターン進行、戦闘、階層継続|
|Dungeon/Core/DungeonMap|グリッド判定、部屋情報、探索・視界|
|Dungeon/Core/DungeonGenerator|部屋／通路の生成、開始地点／階段|
|Dungeon/Core/EnemyAI|認識、壁・敵を避ける経路探索|
|Dungeon/Core/ItemManager|床アイテム配置|
|Dungeon/Core/DungeonRules|数値と敵／アイテムデータ型|
|Dungeon/Rendering/DungeonRenderer|Tilemap、キャラ表示、カメラ、HPバー、プールと生成リソース解放|
|Dungeon/UI/DungeonUI|HUD、ボタン、確認・バッグ・拡大地図、ミニマップ|
|Dungeon/DungeonPlaytest|明示フラグ時のみの実行時検証|
|Editor/LunaProjectTools, LunaValidation, DungeonCoreTests, DungeonEntry, DungeonSpriteImporter|Scene準備、ビルド、検証、Play開始、素材設定|

## 検証

- Core：120シードの全床／階段到達、生成位置の重複／壁内防止、100移動、100敵追跡入力、固定ダメージと死亡、20枠制限、5階連続移動、満腹度0のclamp。
- 実行版：390×844で218チェック成功。100回移動と表示座標、連打ロック、拾得・バッグ更新、敵への攻撃とHP、階段到達、B2F継続。移動テスト中は敵を除外して分離し、戦闘は基準HP・攻撃値で別検証。
- 手動確認：Unity EditorのGame Viewを1170×2532に設定し、Play中に方向ボタンで移動、薬草の拾得確認、バッグが1/20に更新されることを確認。実機のノッチ／ホームインジケーターは未確認です。
- 証跡：`Library/DungeonCoreTests.json`、実行版の `QA/runtime-result.json`、`QA/dungeon-b1.png`、`QA/dungeon-b2.png`。
- 商用ゲームの素材・コード・マップは使わず、ゲームデザインだけを参考にしています。
