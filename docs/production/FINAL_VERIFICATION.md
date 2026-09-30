# 引き渡しファイルの最終照合

2026-09-30 08:47 JST。採用は0818/build20。ファイル内容の変更は行わず、読み取り照合だけを追加。

- DownloadsのWindows ZIPとUnityソースZIPをBuilds/Handoff/manifest-20260930-0818.jsonのサイズ・SHA256へ再照合。双方一致。
- ソースZIPのAssets/Packages/ProjectSettings配下298ファイルを現在の作業プロジェクトへ内容ハッシュ照合。相違0。梱包後の文書追記は比較対象外で、ゲームの修正漏れがないことをこの範囲で確認。
- 展開Windowsの166チェック、build20の223チェック、ソース新規展開のUnity検査成功は各結果JSONとログに保存済み。
- この確認時点でLunaEclipseLabyrinth実行版プロセスは残っていない。テストで通常保存を書き換えないメモリ隔離を使用。

0818版に同梱されたSTART_HERE/PLAY_GUIDEは検証範囲と未完成を明示する。最終報告でも商用完成、iOS実機動作、全環境リークなし、聴感検証済みとはしない。

## 次の制作者へ

ソースを開くときはUnity 6000.6.3f1、Startup.unityから。作業プロジェクトと展開検証コピーを同時に編集しない。編集の正本はC:/Users/youta/UnityProjects/LunaEclipseLabyrinth。Builds/SourceCheck-0818は検証用コピー。

仕様決定待ちの通常遺物報酬・ミッションを先に確定する。OPEN_ITEMS.mdを参照。QAの数は限定条件の自動検査であり、遊びの完成度を数値だけで保証するものではない。
