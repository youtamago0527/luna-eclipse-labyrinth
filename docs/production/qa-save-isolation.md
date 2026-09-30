# QA保存隔離・読み取り監査

## 現状：build17で実証済み

LocalSettings.SessionOnlyとLunaAppのストア選択により、任意の`--luna-`接頭辞を持つ起動は音量・進行ともメモリ保存を使用する。通常のフラグなし起動は従来どおりPlayerPrefsを使用する。

`Logs/production-build-17.log`と`Builds/DungeonPreview/QA/production-build17-isolation/production-result.json`を確認。build17の実行版は`passed: true`、124チェック成功。追加の音量変更・NaN/Infinity拒否・Clamp・PlayerPrefs値とHasKey不変の検査を含む。補助フラグ単独の分岐はコード読み取り確認のみで、単独実行を実証したものではない。

以下の「進行保存」「補助フラグの注意」「LocalSettingsは共通」は修正前の監査履歴。現行の不具合を示すものではない。

## 修正前の監査履歴

Unity起動、実PlayerPrefs操作、コード変更は行っていない。50周Soak実行中のプロセスには触れていない。

## 進行保存

LunaApp.AwakeのproductionQA判定は `--luna-qa`、`--luna-production-qa`、`--luna-soak-qa`、`--luna-frame-qa`、`--luna-motion-qa` を含む。これら、または `--luna-preview` があるとRelicService／InventoryVaultにProductionPlaytest.Storeを渡す。

StoreはDictionaryだけを使うMemoryStoreでFlushは空。Interrupted復元・出発escrow・帰還を含め、同じサービスインスタンスを使うため進行データは通常PlayerPrefsへ届かない。ProductionPlaytestの再読込確認にもStoreが明示されている。Editorサービステストも各自の注入ストアを使う。

通常起動はnullを渡してPlayerPrefsRelicStoreを使用する。キーはluna.unity.relics.v1とluna.unity.inventory.v1（backup/corrupt含む）。QAが別キーをPlayerPrefsに書く設計ではなく、ストアそのものをメモリへ置き換える設計。

## 補助フラグの注意

`--luna-frame-edge-qa`、`--luna-motion-edge-qa` はそれぞれ本体frame/motionフラグと併記するモード指定。単独ではQAは始まらず、進行ストア隔離も有効にならない。同様にラベル／Soak回数引数だけではQA起動条件を満たさない。親担当の既存起動は本体フラグ併記であり、この漏れを踏んだ証拠はない。

## LocalSettingsは共通（隔離範囲外）

LocalSettings.BgmVolume／SeVolumeは常に通常PlayerPrefsのluna.settings.bgm-volume／luna.settings.se-volumeを読む。setterはSetFloatとSaveを呼ぶ。SettingsViewのスライダーをQA／preview中に人が変更すれば実音量設定へ保存される。

現行自動テストに音量setterの呼び出しは確認されない。SettingsView初期化はSetValueWithoutNotifyなので画面を開くだけでは書かない。音量をミュートするQA処理はAudioListener.volumeであり、PlayerPrefsを変更しない。したがって現在の自動Soak／通しテストの保存隔離は保たれるが、「QAモードならあらゆる実設定へ一切書かない」とまでは保証しない。

将来音量UIの自動テストを行う場合、LocalSettingsにも同じQA検出に基づくメモリ保存を用意するか、テストがsetterを触らない境界を明記する。ユーザーの音量を無断で初期化しない。

## 追加修正の現状（実証前）

上記は修正前の監査結果。追加承認後、LocalSettings.SessionOnlyを追加し、任意の`--luna-`接頭辞がある起動では音量をstatic辞書へ読み書きするよう変更した。初回の既存音量読取りは許可するが、SetFloat／Saveは呼ばない。通常起動の保存、Changed通知、NaN／Infinity拒否、Clamp処理は維持する。

親担当はLunaAppのストア選択もSessionOnlyへ統一した。補助フラグ単独を含め、luna専用フラグで進行ストアが隔離される。ProductionPlaytestには音量変更、NaN、Clamp、PlayerPrefs値とHasKey不変の計6検査を追加済み。

この追記時点では50周Soakが旧build16で実行中。新ソースは未コンパイル・未実行で、修正検証成功とは扱わない。Soak終了後に親担当がbuild17で確認予定。
