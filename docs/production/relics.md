# 恒久遺物・Unity実装

## 確定設計と試作案

ユーザーの第1期指示に従い、HP・攻撃・防御の直接恒久強化はしない。満腹最大100、回復なし。統合先は基本5行動に `SatietyIntervalBonus` を加算する。装備品質は乱数で得た強化値に加算し、負数も維持する。

実装済みの5効果は月炉の残響（装備品質、上限999）、静夜の香炉（満腹消費間隔、上限未定）、星底の羅針盤（到達済み開始階、上限未定）、月蔵の鍵束（倉庫+Lv、上限999）、宵縫いの留め具（バッグ+Lv、上限20）。最大Lv=0は「未定」であり、999固定を意味しない。基礎倉庫20枠は暫定。

薄月の研晶・夜帳の銀糸・月環の振り子は追加案。定義はあるがEnabled=falseで効果と報酬抽選から除外する。上限30/30/15も候補値にすぎない。現行の拠点遺物UIはEnabled=trueの確定5系統だけを表示し、この3案は非表示にする。

通常版は20Fごとの獲得機会を記録するだけ。ランクと付与アイテムは未確定。`EnableTrialRewardTable=false` が既定値。trueはテスト用で、D/C/B/A/Sの境界20/40/80/120/200、到達ランク以下の有効遺物から均等候補選択という仮値。ユーザー承認なしに本番で有効化しない。

## 統合API

```csharp
using LunaEclipse.Progression;
var relics = new RelicService(); // PlayerPrefs、Webセーブとは別キー
RelicBonus bonus = relics.GetBonus();
int bagCapacity = bonus.BagCapacity; // 20〜40
int warehouseCapacity = bonus.WarehouseCapacity;
int interval = 5 + bonus.SatietyIntervalBonus;
int enhancement = rolledEnhancement + bonus.EquipmentEnhancement;
int maxStartFloor = bonus.MaxStartFloor;
var report = relics.CompleteRun(runId, maxFloor, startFloor); // 保存失敗ならnull
// デザイナーが確定させた報酬の付与のみ。grantIdは同一報酬で不変。
bool saved = relics.Grant("approved-event:unique-id", "bag_expansion");
RelicView.Build(parent430x932, relics, () => Navigate("hub"));
```

CompleteRunはrunId単位で一度だけ確定。スキップ開始の階そのものは通過報酬に含めない。過去階へ遡って報酬を自動生成しない。TryClaim(rewardId)は割当済みの報酬だけ受け取る。未割当報酬は受け取れない。保留機会を承認する場合は `Grant(pending.Id, approvedRelicId)` とすると当該機会も受取済みになる。Grant/Claim/報告は保存失敗でメモリ状態を戻す。既存正常保存を.backupに保持、壊れた元データは次保存時.corruptへ退避。オンライン改ざん耐性は対象外。

## 参考調査

https://wikiwiki.jp/yousha-no-to/equipment （勇者の塔 非公式Wiki・装備アイテム）

https://wikiwiki.jp/yousha-no-to/guild-hat （同・ギルド帽子）

非公式資料で確認できるのは、装備の重複成長と周回後の確率クエストという構造。ギルド帽子は25〜40%と報告されるが、最低目撃階は公式の確定解放条件ではない。本作のB20F解放・ミッション確率は過去ユーザー合意／独自設定として扱う。これらを参考に「周回成果が次回探索の環境改善になる」点だけを採用し、名称・画像・説明文は新規作成。タイムアタック本体はこのサービスに含まない。

## 検査

Assets/Editor/RelicTests.cs の Luna/Test Relics は保存、上限、再読込、負強化値加算、開始候補階、20F保留、重複報酬拒否、保存失敗巻戻し、バックアップ復元、テスト報酬受取、無効候補拒否を検査する。Unity起動・実行は統合担当が行う。
