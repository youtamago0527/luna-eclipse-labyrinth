# Mac / TestFlight 引き継ぎ

2026-09-30。Windows試遊版0818/build20のUnityソースをGitで受け渡す。
これはiOS動作確認済み・TestFlightアップロード済みのビルドではない。

## Macで取得

ターミナルで、まだ同名フォルダが存在しない作業場所を選んで実行する。
既存のアルセーヌや別のルナプロジェクトは上書きしない。iCloud外のローカルディスクを推奨。

```sh
git clone --single-branch --branch unity/testflight-preview-20260930 https://github.com/youtamago0527/luna-eclipse-labyrinth.git LunaEclipseLabyrinth-Unity
cd LunaEclipseLabyrinth-Unity
git branch --show-current
```

このブランチはWeb版と履歴を分離したUnity用ルート。`main`はWeb版なのでマージしない。
取得後、Unity Hubにこのフォルダを追加。Unity **6000.6.3f1** とiOS Build Supportを使用。
`Assets/Scenes/Startup.unity`を開く。Libraryは再生成される。Git LFSの取得操作は不要。

## 作業担当への依頼

ユーザーは既存MacとApple Developerアカウントを利用し、ブラウザ試遊版ではなくTestFlightを希望。
まずHANDOFF.md、OPEN_ITEMS.md、PLAN.md、music.md、art.md、relics.mdを読む。

1. MacのUnity/iOS Build Support/Xcode/署名チームを確認し、コンパイルとStartupの動作を確認。
2. App Store Connectの既存ルナ登録を確認してからBundle IDを設定。Appleアカウントは共用するが、アルセーヌのアプリIDを流用しない。
3. Bundle IDとTeam IDは登録確認まで未確定。ビルド番号はApp Store Connectの履歴を確認して決める。下記の設定メニューはこれらを勝手に変更しない。
4. 下記の専用メニューでiOS書き出し、Xcode署名、ビルド検査、Archive、TestFlight用アップロードを進める。個人の試遊を優先し、公開招待リンク作成やApp Store本公開はしない。
5. iPhoneでsafe area、タッチ、音、保存、中断復帰を検証。Windows成功をiOS成功と報告しない。

購入・新たな契約同意・認証入力が必要な場合はユーザーへ依頼する。
署名鍵、証明書、パスワード、API秘密鍵をGitやiCloudに保存しない。

## 保持する制約

- 旧Web版、ARSENE、他プロジェクトを変更しない。
- 大きなマス、0.85マスのルナ、追従カメラ、拠点、確定遺物5系統を保持。
- 未承認のランク別報酬は無効のまま。HP/攻撃/防御の直接恒久強化、満腹回復を追加しない。
- ミッションやショップ等は未完成。完全な探索途中再開は未実装。試遊終了は帰還してから。
- 素材のクレジットとライセンス文書を保持。未確認素材はSourceArchiveにありGit対象外。

## 検証範囲

Windowsの配布版166項目、実行時223項目、ソース新規展開のUnity検査は成功済み。
Mac側の取得・iOSコンパイル・署名・実機確認・TestFlightはこれから。

## iOS書き出し設定（専用メニュー）

まずこのブランチを `git pull --ff-only` で更新する。変更が競合する場合は上書きせず確認する。
MacのUnity Hubで6000.6.3f1に **iOS Build Support** があることを確認し、なければそのEditorへのモジュール追加を行う。
Windows側からMacへのモジュールインストールは実行していない。

1. Unity 6000.6.3f1で開き、`Luna > iOS > 1 Prepare Export Settings` を実行。
   縦固定、iOS 15以上、実機Device SDK、IL2CPP、ARM64、Metal、Minimal stripping、Startup→Dungeonの順に設定する。
   元のBundle ID/Team ID/ビルド番号/署名方式と端末ファミリーを保持する。素材加工やゲーム仕様変更はしない。
2. Build Profilesでプラットフォームを **iOS** に切り替え、インポート完了を待つ。
   専用書き出しはシーン配列を明示するため、他のBuild Profileの独自シーン順には依存しない。
3. iOS Player Settingsで、App Store Connectと照合したルナ専用Bundle ID、Apple Team ID、Automatic Signing、正の整数ビルド番号を入力する。
   購入・署名鍵生成・アプリ登録はこのメニューでは行わない。
4. `Luna > iOS > 2 Validate Export Readiness` を実行。バージョン、モジュール、アクティブターゲット、識別情報と既存プロジェクト検証を確認する。
5. `Luna > iOS > 3 Export Xcode Project` で `Builds/iOS/Luna-Xcode-<UTC日時>/` へ非DevelopmentのXcodeプロジェクトを書き出す。
   既存書き出しを上書きしない。Archiveやアップロードは自動実行しない。
6. Xcodeで生成されたUnity-iPhone.xcodeprojを開いて進める。App Store用アイコン、SDK要件、署名、実機動作は別途確認が必要。

### コマンド実行（Mac側の担当向け）

Unity Editorを閉じてから、プロジェクトルートで実行。
次の3環境変数には、Mac側で確認した実値を設定する。秘密鍵やパスワードではないが、公開Gitに固定値として保存しない。

```sh
# 事前に設定する値: LUNA_IOS_BUNDLE_ID / LUNA_APPLE_TEAM_ID / LUNA_IOS_BUILD_NUMBER
# 未設定・仮ID・ARSENEのID・不正な番号は処理を止める。
: "${LUNA_IOS_BUNDLE_ID:?Set the confirmed Luna Bundle ID}"
: "${LUNA_APPLE_TEAM_ID:?Set the confirmed Apple Team ID}"
: "${LUNA_IOS_BUILD_NUMBER:?Set an unused positive build number}"
export LUNA_IOS_BUNDLE_ID LUNA_APPLE_TEAM_ID LUNA_IOS_BUILD_NUMBER
mkdir -p Logs
"/Applications/Unity/Hub/Editor/6000.6.3f1/Unity.app/Contents/MacOS/Unity" \
  -batchmode -quit -buildTarget iOS -projectPath "$PWD" \
  -executeMethod LunaEclipse.EditorTools.LunaIOSBuild.ConfigureAndExport \
  -logFile "$PWD/Logs/ios-export.log"
```

batchmode中のプラットフォーム切り替えに依存せず、起動引数でiOSを選ぶ。
設定だけ行う場合は `-executeMethod LunaEclipse.EditorTools.LunaIOSBuild.Prepare` を使う（3環境変数は不要）。
設定テストは `LunaEclipse.EditorTools.LunaIOSBuildTests.Run`。これはiOSモジュールなしでも走る設定検査であり、iOSビルド検証ではない。

API参照: [Unity 6 iOS設定](https://docs.unity.com/en-us/engine/6000.6/script-reference/unityeditor/playersettings/ios)、[ARM64設定](https://docs.unity.com/en-us/engine/6000.0/script-reference/unityeditor/playersettings/setarchitecture)。

### Windows側で確認した結果（2026-09-30）

- Unity 6000.6.3f1でEditorスクリプトのコンパイルと設定テスト25項目が成功、batchmode終了コード0。
- `Library/LunaIOSSettingsTests.json`: passed=true、xcodeExportTested=false。
- 既存LunaValidationもpassed=true、errors=[]。設定の再適用、シーン順、IL2CPP/ARM64/Metal、識別情報・Windowsバックエンドの保持と仮ID拒否を検査。
- DungeonCoreTestsの回帰検査も成功。120シードの部屋・通路、100移動、戦闘、拾得、5階連続進行等を確認、batchmode終了コード0。
- iOS表示名APIが共通productNameを変更する挙動を検出し、表示名変更を削除。共通製品名と保存先に影響する変更は残していない。
- WindowsにはiOS Build Supportがなく、Macには接続できないため、モジュール導入・Xcode生成・署名は未検証。この25項目をiOSビルド成功と扱わない。
