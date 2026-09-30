# 遺物UI・効果の読み取り監査

監査時点の実コード確認。コード変更・Unity起動・ビルドなし。

## 表示件数：修正済み

監査開始時、RelicView.csのcontent高さとforループはRelicBalance.Definitions全件を使用していたため、確定5系統に加えて無効3種も「試作案」と表示していた。「確定5系統のみを拠点UIへ表示」という要件には当時未達だった。

最小修正案：BuildでEnabledの定義だけ抽出し、その配列を高さ・ループで共用する。定義データは残してよい。

追加承認後、RelicView.csだけに上記フィルタを適用済み。現在はEnabled=trueの5件でリスト高さ・ループを共用する。合計値／開始範囲説明の変更は今回は見送り。Unity表示検証は親担当が行う。

資料再確認ではREADMEの「遺物5系統の効果接続」「未承認の通常報酬は自動付与しない」は現行実装と一致。relics.mdに残っていた候補3件のUI表示説明を、現行の非表示方針へ訂正した。

## 報酬保留：一致

RelicBalance.EnableTrialRewardTableの既定はfalse。LunaAppは通常RelicServiceを既定構築する。CompleteRunは通常Rank/RelicId=nullの機会として保存し、TryClaimは未割当を拒否する。テストのみtrueを使用。通常UIに未承認テーブルを有効化するボタンはない。

## 効果適用：一致

- 品質補正：LunaApp→RunModifiers→ItemManager。武器と盾にrolled(-3〜3)+補正。HP/攻撃/防御へ恒久直接加算はない。
- 満腹耐久：基本5行動＋BonusをCoreに適用。回復アイテムはHPのみ。
- 羅針盤：GetBonusはmin(Lv+1,HighestFloor)。Departure表示と起動時Clampへ接続。
- 倉庫拡張：帰還と倉庫UIへWarehouseCapacityを渡す。
- バッグ拡張：出発・持込・探索BagCapacityに20〜40枠を適用。

## 説明改善候補

RelicViewの「現在 +Lv / NEXT +Lv+1」は補正としては正しいが、羅針盤の実際の選択上限は最高到達階で制限される。合計値として「開始可能B1〜NF」「満腹消費N行動」「バッグN枠」「倉庫N枠」を表示すると実効値が読み取れる。現在未使用のbonus変数を活用可能。未確定最大Lvの「未定」表示は正しい。

## テスト補完候補

既存サービス/ゲームテストで保存、バッグ上限、満腹消費、開始階、負強化値加算などを検査している。追加候補はUIのEnabled表示件数=5、無効3名称が存在しない、通常報酬受取ボタンなし、倉庫Lv999→1019枠、鍛冶Lv999上限、羅針盤Lv>最高到達時の表示一致。既存テストをすべて再作成する必要はない。
