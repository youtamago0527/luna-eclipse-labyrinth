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
3. 元データはapplicationIdentifierとTeam IDが未設定、自動署名OFF、iOS最低版設定15.0。ビルド番号はApp Store Connectの履歴を確認して決める。
4. iOS用Build Profileのシーン順はStartupを先頭にする。現在のEditorBuildSettingsはDungeonが先頭なので、そのまま汎用ビルドしない。既存Windowsビルド処理の起動順も参照する。iOS書き出し、Xcode署名、ビルド検査、Archive、TestFlight用アップロードを進める。個人の試遊を優先し、公開招待リンク作成やApp Store本公開はしない。
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
本Git登録はソースと引き継ぎ整備のみ。Mac側の取得・コンパイル・署名・実機確認・TestFlightはこれから。
