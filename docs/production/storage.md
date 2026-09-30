# Unity倉庫・持ち帰り基盤

`LunaEclipse.Progression.InventoryVault` は戦闘データと独立したDTO `StoredItem { Id, Name, Kind, Enhancement }` を保存する。`ItemData`から明示コピーして渡す。旧Web保存とは別の `luna.unity.inventory.v1` を使用。

```csharp
var vault = new InventoryVault();
var items = run.Bag.ConvertAll(item => new StoredItem {
    Id=item.Id, Name=item.Name, Kind=item.Kind, Enhancement=item.Enhancement
});
bool saved = vault.DepositRun(runId, items, relics.GetBonus().WarehouseCapacity);
// saved=false時は探索のバッグを消去したり、登録完了と表示しない。
WarehouseView.Build(parent430x932, vault, relics.GetBonus().WarehouseCapacity,
    () => Navigate("hub"));
```

`DepositRun` は同じrunIdを二度登録しない。倉庫容量に達した分は `Profile.Pending` に永続保存し、削除しない。`Profile.LastReturned` は直近登録品のコピー履歴で、所持品として二重に扱わない。

`Withdraw(warehouseIndex)` は倉庫から永続保留トレイへの移動。出撃持込や外部へのアイテム引き渡しではない。`Deposit(pendingIndex,capacity)` で回収し、`Swap(warehouseIndex,pendingIndex)` なら満杯でも無損失入替できる。UIでは「保留へ」「倉庫へ」の二段階操作で同じ結果にできる。売却・削除・出撃持込は実装していない。

変更はコピー上で行い、保存成功後のみProfileに反映する。正常前版 `.backup`、破損原本 `.corrupt` を保持する。保存失敗時にはメモリ上の所持品を変更せず、LastErrorを表示する。バックアップは直前正常保存なので復元時に最後の操作が巻き戻る場合はある。これはローカル保存でありクラウド同期ではない。

テスト入口 `LunaEclipse.EditorTools.InventoryVaultTests.Run`：満杯保留、コピー独立性、runId重複拒否、入替、再読込、負強化値、保存失敗巻戻し、バックアップ復元。Unity実行は統合担当が行う。
