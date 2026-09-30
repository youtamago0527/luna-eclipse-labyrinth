# ソースZIPからのクリーン読込確認

2026-09-30 08:32 JST。親が0818ソースZIPをBuilds/SourceCheck-0818/LunaEclipseLabyrinthへ新規展開し、同梱していないLibraryを新規生成するUnity 6000.6.3f1のbatchmode起動を実施。元作業フォルダーのLibraryはコピーしていない。

実行メソッド：LunaEclipse.EditorTools.LunaValidation.Validate。コンパイルと素材インポートが完了し、Library/LunaValidationReport.jsonのpassed=true、errors=[]を確認。Logs/source-package-0818.logは終了コード0で正常終了。起動シーン・コンパイル済みLunaApp・拠点7素材・縦画面設定を検査する。全7ロジック検査やWindows再ビルドをこのクリーン展開先で実行したわけではない。

独立担当もPackages/manifest.json、packages-lock.json、Assets/Editorを読取監査。組込みパッケージのみでlocal file依存なし。Editor入出力はAssets/Library/Builds等の相対パス、元UnityProjects/iCloud/ユーザー名への絶対参照は見つからない。

同じWindows PCのインストール済みUnityとライセンス・グローバルキャッシュを使用するため、別PC・オフライン・別Unity版での完全動作保証ではない。元プロジェクト・配布ZIPは変更せず、展開先は検証用として保持した。通常セーブやプレイヤー起動は行っていない。
