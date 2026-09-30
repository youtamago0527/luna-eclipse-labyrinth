# 配布ZIPの文書参照監査

2026-09-30。`Builds/Handoff/luna-windows-playable-20260930-0648.zip`（93,222,199 bytes）と `luna-unity-source-20260930-0648.zip`（68,400,497 bytes）をSystem.IO.Compression.ZipArchiveの読取のみで確認。展開・実行・Unity操作・Soak操作なし。対象は文書の同梱と参照経路であり、exe起動や音楽の利用条件を再検証したものではない。

## Windows ZIPで確認できた制作記録

ZIP内共通接頭辞：`LunaEclipseLabyrinth/`。全展開後、このフォルダー内の `Documentation/README.md` を開き、画像・音楽・フォントの記録は次の場所から読める。

| 対象 | 展開後の文書パス | 結果 |
|---|---|---|
| 追加モンスター・アイテム12点 | `Documentation/docs/production/art.md` | 実在。prompt、原本ファイル名、生成方法あり |
| 床・壁5点 | `Documentation/docs/production/terrain-art.md` | 実在。初版と採用版の区別あり |
| 下降階段 | `Documentation/docs/production/stair-art.md` | 実在。prompt・原本・制限あり |
| 画像出所分類 | `Documentation/docs/production/asset-provenance.md` | 実在 |
| 追加BGM3曲 | `Documentation/docs/production/music.md` | 実在。生成日時・公式DL・プラン確認・SHA256・promptの記録あり |
| 日本語フォント許諾文 | `Documentation/Assets/Resources/Fonts/OFL.txt` | 実在。READMEの相対位置と合う |

生成記録内の `C:/Users/youta/...` は制作者PC上の制作履歴パスであり、Windows ZIP展開先で直接開けるパスではない。生成原PNGやMP3を個別ファイルとして開く必要がある場合は別途ソースZIP側の `Assets/Resources/Art` / `Assets/Resources/Audio/Generated` を用いる。実行版のData内部から素材抽出する手順は案内しない。READMEからart/music/OFLへの相対記述はDocumentationを基準にすれば解決する。

## 0648版の不備（親へ報告済み）

1. `PLAY_GUIDE.md` 末尾は「README」と `docs/production/OPEN_ITEMS.md` を案内するが、同じ階層にREADMEはなく、実在するのは `Documentation/README.md`。OPEN_ITEMSはWindows ZIPのどの位置にもない。
2. `Documentation/README.md` が案内する `docs/production/PLAN.md`、`VISUAL_REVIEW.md`、`DUNGEON_REPORT.md` もWindows ZIP内にない。READMEだけでは最新制限・表示確認・実装履歴へ進めない。
3. `asset-provenance.md` が参照するLuna素材のREADME/generation-prompts/extractionはWindows ZIPのDocumentationにない。生成済み攻撃素材の詳細追跡にはソースZIPが必要。画像制作記録をWindows配布だけで完結させる場合はこの3文書も任意で追加する。

ソースZIPにはOPEN_ITEMS・PLAN・VISUAL_REVIEW・DUNGEON_REPORT、およびLunaのREADME/generation-prompts/extractionとFonts/OFLが存在する。したがって元文書紛失ではなく、Windows ZIPの収録対象・案内パスの不整合である。

最小修正案：WindowsのDocumentationへOPEN_ITEMS/PLANを `docs/production/` の相対構造で、VISUAL_REVIEW/DUNGEON_REPORTをREADMEと同階層に収録。PLAY_GUIDE末尾の案内を `Documentation/README.md` と `Documentation/docs/production/OPEN_ITEMS.md` へ明確化する。親担当がPackaging scriptを修正し、再作成版を別名で検査する。監査担当はZIPやscriptを書き換えていない。

## build17・0724版の再監査

`Builds/Handoff/luna-windows-playable-20260930-0724.zip`（173 entries）と `luna-unity-source-20260930-0724.zip`（330 entries）をZipArchive読取のみで再検査。展開起動QAは親担当の別検証であり、ここでは行っていない。

- Windows ZIPのPLAY_GUIDE末尾が `Documentation/README.md` / `Documentation/docs/production/OPEN_ITEMS.md` を明示するよう修正され、両方のentryが実在する。
- READMEからの `docs/production/PLAN.md`、`VISUAL_REVIEW.md`、`DUNGEON_REPORT.md` はすべてDocumentationを基準とした正しい相対位置に追加済み。0648版の主要な文書欠落は解消。
- art/music/terrain-art/stair-art/asset-provenance/OFLの同梱を継続確認。Lunaの詳細README/generation-prompts/extractionはWindows Documentationには含まれず、ソースZIPに存在する（前記のソース参照扱いを継続）。
- 両ZIPに `SourceArchive`、`Unverified`、`luna-seated-v1` を含むentryは0。未確認画像およびmetaの独立ファイル混入なし。
- Windows ZIPに `QA/` パスのentryは0。ソースZIPでは `Assets/Scripts/QA/DungeonMotionPlaytest.cs` とmetaの2件だけが該当する。これは検証用ソースコードであり、撮影PNG・結果JSON・実行ログ等のQA出力ではない。生成QA成果物の混入は確認されない。

この検査はZIP entryと文書参照の整合を確認したもので、UnityのData内バイナリに対する素材抽出検査ではない。ソースQAスクリプトを含むことと実行版にQA出力を同梱することを混同しない。新たな配布文書のブロッカーは見つからなかった。

## build20・0818朝版の最終文書監査

`Builds/Handoff/luna-windows-playable-20260930-0818.zip` をZipArchive読取のみで確認（93,247,211 bytes、174 entries）。展開・実行は親担当の別QAで、本担当は行っていない。

- `LunaEclipseLabyrinth/START_HERE.md` と `PLAY_GUIDE.md` が実在。START_HEREのbuild20表記、試遊手順、未完成項目、ソース版との区別を確認。
- PLAY_GUIDEが指定する `Documentation/README.md` と `Documentation/docs/production/OPEN_ITEMS.md` は実在。READMEのPLAN/VISUAL_REVIEW/DUNGEON_REPORT参照も同梱構造と一致する。
- Documentation内に `art.md`、`terrain-art.md`、`stair-art.md`、`asset-provenance.md`、`music.md` と `Assets/Resources/Fonts/OFL.txt` を確認。制作記録を読む入口と主要記録に欠落なし。
- `SourceArchive`、`Unverified`、`luna-seated-v1`、`QA/` に該当するZIP entryはすべて0。未確認素材の独立ファイル・QA成果物の同梱は見つからない。
- START_HEREのGame View画像は明確に「ソースZIPのScreenshots」と案内しており、Windows ZIP内にないことを欠落とは扱わない。START_HEREの「ZIP直下のPLAY_GUIDE」は実際には共通トップフォルダー `LunaEclipseLabyrinth` 内の同階層を意味する。

今回の文書・entry監査では配布を妨げる新たな問題は見つからなかった。Data内部のバイナリ素材を抽出しての検証、記載された各テストの再実行、SHA256照合は本監査範囲外。
