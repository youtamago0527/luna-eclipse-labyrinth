# 地形原画像 — 2026-09-30

## 改訂：1枚の敷石を1マスとして認識できる版

親タスクの目視検品で初版の2×2石が4マスに見えるため、採用床を `floor_single.png` / `floor_single_cracked.png` に変更する。旧画像は原本として保存している。新2点は内部を区切る目地がなく、目地は画像の外周だけ。1254×1254・全alpha255を確認。生成後加工なし。

生成元：`exec-5ebbc22c-ccf7-4cad-bf91-a9bac9db9702.png`（通常）、`exec-db7e6947-f11a-426c-afe5-7f22cf516750.png`（亀裂）。共通生成元/保存先は下記と同じ。

通常床プロンプト：Use case stylized-concept. ONE SINGLE LARGE SQUARE PAVING SLAB texture for 2D dungeon: this entire square image represents ONE grid cell and ONE stone. Absolutely NO internal mortar lines, NO central vertical/horizontal seams, NO divisions into smaller blocks. A single continuous broad black-navy blue-gray stone face covers entire canvas. The ONLY mortar is a narrow2% recess along the FOUR OUTER EDGES, with slightly chipped pale blue-gray bevel just inside edge. Surface has subtle low-contrast stone grain and one small natural diagonal scratch, no busy texture. Soft uniform blue moonlight, hand-painted refined gothic RPG material, readable at43px, fairly visible blue-gray stone not black. Perfectly flat overhead orthographic view, seamless repeated boundary style, no directional shadows, no perspective. Full bleed opaque square PNG every pixel filled. No purple magic, no motif, no objects, no text/labels/UI, no blank margins, no decorative frame. One slab only, never a grid of stones.

亀裂床プロンプト（通常床をedit target指定）：Use case precise-object-edit. Image is edit target: one SINGLE square stone floor slab with mortar only at four outside edges. Add ONE narrow restrained purple magical hairline crack following existing diagonal scratch near upper-left interior, with tiny branch. Keep outer10% edges unchanged, keep all stone texture, brightness, color, framing and dimensions otherwise identical. The glowing purple crack must stay inside the slab, never touch edge, no particles or surrounding aura. CRITICAL remains ONE unpartitioned stone; no internal vertical/horizontal mortar or divisions. Full-bleed OPAQUE top-down texture, no labels, text, UI, new objects, blank margin or perspective. At43px gameplay it reads as one big blue-gray stone with a subtle violet fissure.

新2点も数学的な端pixel一致は保証しない。外周の暗い目地で連続を自然に見せる構成。built-in生成は合計5回。

built-in `image_gen.imagegen` を3回使用。imagegenスキルを再適用。CLI/API fallbackなし。既存キャラクター/アイテム12素材およびゲームコードは変更していない。画像生成後の原PNGは無加工コピー。

保存先共通：`C:/Users/youta/UnityProjects/LunaEclipseLabyrinth/Assets/Resources/Art/Terrain/`
生成元共通：`C:/Users/youta/.codex/generated_images/01a08128-c3f6-7ba0-a0c4-098171fe0c00/`

| 保存名 | 用途 | 生成元ファイル |
|---|---|---|
| floor_stone.png | 1グリッドマスの平面床。黒紺の大きい石板、青白い摩耗縁 | exec-46156507-18a0-42fd-a36e-897ede9e60f5.png |
| floor_cracked.png | 同じ床の控えめな紫亀裂版。通常床をedit targetとして生成 | exec-d5bcbc5c-d9ef-4e03-aa67-321ccb6359b0.png |
| wall_stone.png | 壁マス用の細かい石積み。上面ハイライトと暗い目地 | exec-0a81c566-dab8-40c0-b93d-48dfd35dce7d.png |

## 検品・接続上の注意

- 3点とも1254×1254。全ピクセルalpha255。全面不透明、余白・文字・UI・透視投影なし。
- 画像1枚を1Unity Unitのマスへ貼る。床画像内の2×2石板は意匠であり、歩行グリッドを4分割しない。
- 床と亀裂床の石配置・輪郭が揃っている。壁は細かい横積みで床の大石板と区別できる。
- **数学的な完全シームレスではない**。反対側端1列のRGB平均絶対差（0–255）：通常床左右10.72／上下13.82、亀裂床左右10.74／上下13.99、壁左右8.60／上下24.71。
- 特に壁の上下繰返しには石積みの段差・途切れが見える可能性がある。横に並ぶ壁には比較的使いやすいが、外角/内角/笠石の専用オートタイルではない。生成指示は3段だったが出力は約8段の細かい積み石となったため、43pxでの可読性は実ゲームで確認が必要。
- Full-bleed出力と目視の自然な接続を確認したが、完全なwrap pixel一致は保証しない。背景メッシュや当たり判定を変えて無理に合わせず、必要なら次工程で専用接続素材を追加する。
- 地形は透明bbox対象ではないため `art-metrics.json` に追加していない。原PNGの加工も行っていない。

## 生成プロンプト

### floor_stone

Use case: stylized-concept. Asset type: seamless tileable 2D dungeon stone FLOOR texture, one square image equals one grid cell. Generate a perfectly overhead orthographic flat paving texture of black-navy gothic stone, clearly readable medium-dark blue-gray values under soft cool moonlight. Four large slightly irregular worn square stone blocks in a 2x2 layout, shallow chips and tiny natural hairline scratches, restrained texture not noisy, subtle beveled edges. The outer edges must join seamlessly when repeated horizontally and vertically: continue the same mortar gap thickness and stone tones at opposite edges. Full bleed OPAQUE square texture covering every pixel, no transparent pixels and no blank margins. No purple magic cracks, no gold ornament, no focal emblem, no objects, no perspective, no directional cast shadows, no vignette, no frame or border, no text, no letters, no UI. Stylized hand-painted RPG environment art, refined stone material, broad readable forms at43px, top surface only, uniform diffuse blue ambient light.

### floor_cracked

入力：上記floor_stone原画像をedit targetとして指定。

Use case: precise-object-edit. Input image is the edit target: seamless square navy gothic stone floor. Create a CRACKED FLOOR variant by changing ONLY two narrow irregular hairline fissures inside the stone surfaces to dim violet magical light. Preserve exact composition, stone blocks, mortar alignment, diffuse blue-gray brightness, outer edge pixels/style, and square full-bleed opaque format. Purple light must be restrained, no bright white core, no sparks, no particles or glow spilling off tile. Keep the outer10% border region unmodified so this can adjoin the normal floor texture in any direction. The stone remains dominant and walkable, no hole. No text, label, UI, added border, perspective, blank margin, shadow or transparency. Top-down flat stone floor for43px tiles.

### wall_stone

Use case stylized-concept. Asset type: seamless square WALL stone texture for a top-down2D gothic dungeon, each square is one blocked grid tile. Full bleed OPAQUE image, no margin or transparent pixels. Dense black-navy stone masonry distinct from broad flat floor pavers: chunky small staggered blue-charcoal rectangular stones with raised cool-gray chiseled top bevels and deep dark violet mortar recesses. The stones have slightly lighter TOP surfaces and darker narrow vertical inset faces giving wall mass, but keep overall orthographic texture, NO vanishing point or building illustration. Three horizontal courses of staggered blocks filling whole square. Opposite edges should seamlessly continue repeating brick courses horizontally and vertically. Soft uniform blue moonlight, readable medium-dark stone faces, slight chips and worn edges, refined hand-painted RPG texture, broad forms clear at43px, no noisy stripes. No magical purple glow/cracks, no symbols, no door, no objects, no scene, no border/frame, no text/UI, no vignette or external shadow. All pixels stone or mortar.
