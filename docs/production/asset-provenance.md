# Resources画像の出所監査

2026-09-30。対象はUnity `Assets/Resources` 以下の画像。初回列挙はPNG63点（他の列挙対象拡張子jpg/jpeg/webp/gif/bmp/svgなし）、下記の原本退避後は現行62点。生成や画像加工はせず、既存文書・会話内の提供記録・ファイルを確認した。本書は制作経路の分類であり、著作権・商標・配布許諾を法的に確認した証明ではない。ユーザー提供でも第三者の権利確認済みとは断定しない。音声・フォント・コードは本監査の対象外。

## 分類一覧

以下のパスは `Assets/Resources/` を基準とする。

| 範囲 | 点数 | 分類・根拠 |
|---|---:|---|
| `Art/Monsters/{moon_slime,moon_bat,little_ghost,mushroom,mimic,stone_golem}.png` | 6 | 当制作でbuilt-in image_gen新規生成。`docs/production/art.md` に全promptと原本パス。 |
| `Art/Items/{moon_sword,moon_shield,herb,potion,return_scroll,depth_compass}.png` | 6 | 当制作でbuilt-in新規生成。同上。既存商用キャラをコピーしない指示で制作。 |
| `Art/Terrain/{floor_stone,floor_cracked,floor_single,floor_single_cracked,wall_stone}.png` | 5 | 当制作でbuilt-in生成。`terrain-art.md` にpromptと原本。floor_stone/floor_crackedは未採用の初版保管。 |
| `Art/Terrain/stairs_down.png` | 1 | 当制作でbuilt-in新規生成。`stair-art.md` にpromptと原本。 |
| `Hub/moon-ruins.png` | 1 | ユーザー提供「拠点Ui.png」を使う指示に基づく既存Web素材の移植。 |
| `Hub/buttons/{equipment,shop,storage,relics}.png` | 4 | ユーザー提供「拠点ボタン.png」の切り出し利用指示に基づく既存素材。画像内ラベルもユーザー指定。 |
| `Hub/buttons/enter-dungeon.png` | 1 | ユーザー提供「ChatGPT Image 2026年8月30日 16_40_38.png」に対応する既存迷宮ボタン素材。 |
| `Hub/characters/luna-hub-original.png` | 1 | ユーザー提供「ルナ拠点画像.png」をそのまま使う指示に基づく既存素材。 |
| `Hub/characters/luna-seated-v1.png` | 1 | **原制作経路未確認**。旧Web `assets/hub/characters/` に同名あり。今回確認した文書に生成元・提供元の明示がなく、ファイル名だけでAI生成／提供素材とは断定しない。Unity ScriptsおよびWebのcs/css/html/js検索では使用参照を確認できなかった。 |
| `Luna/source/official.png` | 1 | 同梱 `Luna/README.md` がユーザー提供「ルナポリゴンキャラ絵.png」の無変更コピーと明記。 |
| `Luna/walk/{down,left,right,up}_01..04.png` | 16 | 提供原本から抽出した派生素材。`Luna/extraction.json` に抽出範囲・足元・倍率、READMEに工程。 |
| `Luna/idle/{down,left,right,up}.png` | 4 | 上記各方向の歩行1枚目を使用した派生素材。 |
| `Luna/attack/down_01..03.png` | 3 | 提供原本の剣攻撃3枚からの抽出。 |
| `Luna/source/attack-{left,right,up}-generated.png` | 3 | 既存制作時のbuilt-in image_gen追加生成。`Luna/generation-prompts.json` に方向別promptと参照原本を明記。ユーザー提供画像そのものではない。 |
| `Luna/attack/{left,right,up}_01..03.png` | 9 | 上記追加生成ストリップから抽出・色キー透過した派生素材。README/extraction.jsonに工程。 |
| `Luna/contact-sheet.png` | 1 | 上記採用フレームをまとめた確認用派生一覧。独立した第三者素材として扱わない。 |

合計63点。新規Art18点、Hub8点、Luna37点。Lunaを一括で「全てユーザー提供」とするのは不正確で、追加攻撃12点（生成元3＋抽出9）は提供原本を参照した生成派生である。

上の一覧は初回監査時点の履歴。親担当が `Hub/characters/luna-seated-v1.png` と対応 `.meta` を、削除せずプロジェクト内の `SourceArchive/Unverified/`（Assets外）へ移動した。元ファイル名およびGUIDがmeta自身以外のAssets/ProjectSettingsに参照されていないことを親が確認。退避先2ファイルの存在とResources PNGの再列挙62点を本担当も確認した。現行内訳はArt18・Hub7・Luna37。出所未確認という判定自体は変更していない。

親報告では `Package-Preview.ps1` が `SourceArchive/Unverified/` をソースZIP・WindowsZIP双方から除外し、build14を再構築中。退避は配布混入を防ぐためで、権利確認完了を意味しない。この文書更新時点ではbuild14完成やZIP内容を本担当が独立検査したものではない。

## 配布前の注意

- `luna-seated-v1.png` は取得元・制作元が現行資料だけでは確定できないため、親へ要確認として報告した。その後親が上記の退避・配布除外を実施。本担当は文書更新のみ。
- 未使用でもResources内の原本・一覧画像・旧版は配布ビルドへ含まれる可能性がある。公開前に必要な実行素材と制作資料を分ける判断が必要。今回は権限範囲外のため移動しない。
- ユーザー提供の元作者・生成サービス・取得条件はこの監査で確認していない。必要な権利記録は提供者へ確認する。生成記録がある素材にも第三者権利の完全非侵害を保証する記述はしない。
- 当担当が今回外部サイトからダウンロードした画像はない。確認できた生成素材はbuilt-inによるもので、CLI/API代替を用いていない。

## build12 階段実表示の目視

`Builds/DungeonPreview/QA/production-stairs-build12/06b-stairs-approach.png` を閲覧。390×844の画面でルナの右隣に約43px四方の階段が1セル内に入り、中央の暗い下降穴と複数の横段を判別できる。既存石床より暗く入口として区別でき、人物やログに重なっていない。上部の広い暗域は未探索Fogであり、UI余白の問題とは判断しない。親から118チェック成功（1セル寸法・resource検査含む）の報告あり。この目視では自分でUnity起動や再検証は行っていない。
