# 倉庫から次探索への持込

`SelectForNextRun(warehouseIndex,bagCapacity)` は選択品を倉庫から永続Loadoutへ移す。解除は `Unselect(index,warehouseCapacity)`。満杯ならPendingへ戻す。選択画面を閉じても選択は保存される。

親側の出発処理順序:

1. 新しい一意なrunIdを生成する。
2. `vault.BeginExpedition(runId, bagCapacity)`。失敗時は出発しない。
3. `vault.GetExpeditionItems(runId)` のコピーをDungeonRunへ渡す。同じrunIdで起動する。
4. 帰還・死亡時とも現在バッグ全体を `DepositRun(runId, currentBag, warehouseCapacity)` へ渡す。

Loadoutは出発成功時にInExpedition（持込保全トレイ）へ移る。別ActiveRunIdがある状態で出発できない。帰還保存に成功した時だけ元持込トレイを消す。使用した薬や自分で床へ置いた品は現在バッグにないので通常帰還時には戻らない。死亡による追加ロストは設けない。

アプリ起動時は `RecoverInterrupted(warehouseCapacity)` を呼ぶ。失敗したら出発を止める。未確定探索の元持込品を倉庫／満杯ならPendingへ戻し、当該runIdを終了扱いにして古いrunの後日返却を拒否する。**復元対象は持込元品のみ。途中で拾った未確定アイテムや移動状態はセーブしていない。** 復元は探索の続き再開ではない。

既存Version1データに新フィールドがなければ空リストとして初期化する。すべてコピー→保存成功→Profile切替の順で確定。IDと強化値を保持し、帰還時の重複IDは保存を拒否する。

出発UI overload: `ExpeditionScreens.Departure(parent,relics,vault,launch,back,openStorage)`。既存シグネチャも維持。`WarehouseView.Build(parent,vault,capacity,close,bagCapacity=20)`。ボタンは「持込選択」「選択解除」。結果／倉庫は非ゼロ強化値をKindによらず表示する。

Editorテスト `LunaEclipse.EditorTools.VaultLoadoutTests.Run` を追加。Unity起動は統合担当が行う。
