# 出発・結果UI

namespace `LunaEclipse.Progression`、画面基準430×932。SafeAreaは親側のみで適用する。

```csharp
ExpeditionScreens.Departure(parent, relics,
    floor => LaunchDungeon(floor), () => Navigate("hub"));
ExpeditionScreens.Results(parent, relics, vault,
    () => Navigate("hub"), () => Navigate("relics"), () => Navigate("storage"));
```

Departureは初期1F。上下限で±ボタンを無効化し、開始候補が3階以上ある場合は1F／最深候補への移動ボタンを表示。候補が1階でも確認画面を通す。満腹100、探索中回復不能、バッグ容量、装備品質補正、5行動＋遺物Lvの消費間隔を表示する。

ResultsはRunsを後方から調べ、`grant:`を除いた最終通常探索を表示。遺物獲得機会の未割当は「報酬調整待ち」、付与済みは「受取済み」。有効なRelicIdが割り当てられた未受取報酬のみ受取ボタンを設ける。TryClaim結果後に保存エラーを含め再描画する。

倉庫のLastRunIdが通常探索のRunIdと一致するときのみLastReturnedを「今回の品」として表示する。片側保存に失敗した場合に前回の品を誤表示しない。保留トレイの全体件数、両サービスのLastErrorもスクロール領域に表示。結果画面には売却・廃棄・未確定報酬の強制付与を設けない。

Unity Editor起動・ビルドはこの担当では実行していない。統合担当が画面遷移／日本語表示／スクロールを検査する。
