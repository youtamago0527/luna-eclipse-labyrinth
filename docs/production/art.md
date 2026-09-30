# 原画像制作記録 — 2026-09-30

制作方法：built-in `image_gen.imagegen`、各素材を別々の12回で生成。CLI/API fallback不使用。imagegen SKILL.mdおよびprompting/sample-promptsを読んで制作。原PNGは生成先から無加工コピー。リサイズ・切り抜き・透過加工・描き直しはしていない。

保存先の共通絶対パス：`C:/Users/youta/UnityProjects/LunaEclipseLabyrinth/Assets/Resources/Art/`。
原画像の共通保存先：`C:/Users/youta/.codex/generated_images/01a08128-c3f6-7ba0-a0c4-098171fe0c00/`。

| ID | 納品ファイル（共通絶対パスから） | 原画像ファイル |
|---|---|---|
| moon_slime | Monsters/moon_slime.png | exec-1ed6b781-cf16-4de1-ae56-aea55c87b417.png |
| moon_bat | Monsters/moon_bat.png | exec-f3e0abb5-7871-4c0d-8ae2-c9ede0e216ae.png |
| little_ghost | Monsters/little_ghost.png | exec-3ffd3637-6705-4098-aedc-53bec593b53d.png |
| mushroom | Monsters/mushroom.png | exec-319cb706-f4bb-4909-8d41-55a8a3859fda.png |
| mimic | Monsters/mimic.png | exec-2174a0fc-319b-4bae-915b-02fc2af6f52c.png |
| stone_golem | Monsters/stone_golem.png | exec-dd28d374-7030-43e5-9185-3e8ffb143381.png |
| moon_sword | Items/moon_sword.png | exec-96757fa6-71f5-4d33-aaa0-a18d3ad41dd3.png |
| moon_shield | Items/moon_shield.png | exec-c4857ffa-001d-4c78-aaf3-2adc70311d6c.png |
| herb | Items/herb.png | exec-f775007f-2ef4-4c01-8a5f-aef5dfec4226.png |
| potion | Items/potion.png | exec-05963da3-ecc9-49ed-805c-73455f1b778f.png |
| return_scroll | Items/return_scroll.png | exec-28d4ca56-d3d7-43ad-ba0b-0199ff1e34b6.png |
| depth_compass | Items/depth_compass.png | exec-9988f63e-898f-4216-b616-a7574bf2077c.png |

パンは制作前に中止し、深層の羅針盤に変更。食料・満腹回復効果は導入していない。薬草・回復薬はHP用途の絵。戦闘数値やCore/Renderer/UI/Importerはこの素材制作では変更していない。

## 検品

全12点1254×1254 PNG。全画像にalpha=0ピクセルがあり、背景を塗りつぶした偽透過ではないことを検査。目視で文字・枠・地面なし、輪郭切れなしを確認。不透明範囲の機械検査はalpha **>32**。座標はPNG左上原点、Maxを含むinclusive。`Assets/Resources/Art/art-metrics.json` に全12点の実寸・bboxを記録。画像そのものは変更しない。

コウモリの左右余白は16px程度で指示より狭いが、羽先は画像端へ接しておらず切れていない。巻物も横長。Unityではbboxに4px程度の表示余白を付け、最大辺基準で敵0.8マス／アイテム0.6マスへ正規化すると縦横比と余白を守れる。43pxでの最終表示は親タスクの実行版で確認する。

## 生成プロンプト記録

### moon_slime

Use case: stylized-concept. Asset type: isolated original 2D game monster sprite for a cute gothic moon dungeon. Generate ONE moon-colored slime, no other objects. Squat asymmetrical jelly body, pearly icy-blue upper surface with muted lavender lower body, two small midnight-blue friendly eyes and a tiny smile, one soft crescent-shaped internal highlight. Not a known franchise character, no pointy teardrop head. Three-quarter slightly top-down view for a tile-based dungeon. Polished hand-painted pixel-inspired RPG sprite with clean dark outline, broad readable color clusters and restrained highlights; crisp silhouette readable at only 43 pixels. Full creature centered inside a SQUARE PNG with at least 15% clear margin on all sides. GENUINELY TRANSPARENT background with alpha=0 outside creature. No ground, no cast shadow, no scenery, no checkerboard drawn into pixels, no text, no borders, no particles, no watermark. Keep body opacity mostly solid for clear readability on dark navy stone floors.

### moon_bat

Use case: stylized-concept. ONE original cute moon bat monster isolated game sprite, square canvas, genuine transparent alpha background. Small rounded navy-purple fluffy body, oversized triangular ears with dusky rose inner ears, two bright turquoise eyes, tiny fangs, two wide scalloped lavender bat wings lifted in a friendly hover pose. Distinct wide-wing silhouette, full body and wings visible with 15% margin all sides. Three-quarter slightly overhead view for top-down dungeon. Polished crisp pixel-inspired hand-painted RPG style, dark clean outline, broad readable color shapes, minimal detail readable at43px, subtle silver-blue upper-left moonlight. No known franchise design. No ground or shadow, no scenery, no effects around subject, no checkerboard pixels, no labels or border. Only the bat on transparent pixels.

### moon_sword

Use case: stylized-concept. Asset type: ONE isolated moonlight sword inventory and floor-drop icon for original dark fantasy RPG. Square image, genuine transparent alpha background. Elegant readable silver longsword angled bottom-left hilt to top-right blade tip, deep midnight-blue center fuller, pale cyan cutting edge, restrained antique-gold crescent crossguard, navy wrapped grip and tiny blue pommel. Single broad clean weapon silhouette recognizable at43px, hand-painted crisp pixel-inspired RPG icon, limited broad shaded color clusters, no excessive filigree, original design. Entire sword within middle70% of canvas with clear15% margin around all extremities. No glow clouds, no particles, no hands, no holder, no ground/shadow, no border, no text, no watermark, no drawn checkerboard. Alpha0 outside sword.

### moon_shield

Use case: stylized-concept. ONE moon shield icon for a gothic RPG inventory and floor pickup. Square PNG with genuine transparent alpha background. A compact elegant kite-shaped shield viewed nearly front-on from slight overhead: midnight navy enamel face, bold silver crescent moon in center, beveled silver edge with restrained antique gold corner brackets and one blue gem. Thick readable silhouette, hand-painted crisp pixel-inspired sprite with broad color clusters, readable at43px. Entire shield centered, occupies only65% canvas height, at least17% transparent padding all sides. No character, no hands, no straps outside silhouette, no scenery, no ground or shadow, no text or labels, no border around image, no glow cloud, no checkerboard drawn. Original design. Alpha0 outside shield.

### herb

Use case: stylized-concept. Asset type: ONE isolated healing herb item sprite for RPG floor pickup and inventory. Genuine transparent alpha background, square canvas. A small bundle of three broad pointed mint-green leaves with pale silver veins, tied by a short midnight-blue ribbon, viewed slightly from above. Compact elegant silhouette, fresh emerald and jade colors with silver moonlight highlight, crisp clean dark outline, hand-painted pixel-inspired RPG art using broad simple color clusters legible at43px. Center the ENTIRE herb within middle65% of canvas, ample transparent margin. This is medicinal herb, no edible food. No pot, no soil, no ground or shadow, no particles, no extra objects, no label/text/border/watermark or checkerboard. Outside herb must be truly transparent.

### little_ghost

Use case stylized-concept. ONE original cute little ghost monster sprite for a top-down gothic dungeon. Square genuine transparent PNG. Small pearly white and pale lilac ghost with rounded head, two deep blue oval eyes, a shy little smile, two short flowing sleeve-like arms and a single curled wisp tail instead of legs. No clothing or props. Slight overhead three-quarter view, crisp dark-blue outline, broad clean shaded color clusters, polished hand-painted pixel-inspired RPG sprite readable at43px. Full ghost centrally framed occupying60% canvas height with20% empty padding on every side. No ground/shadow, no particles/aura outside ghost, no scenery, no text/border/watermark/checkerboard. Alpha0 background, mostly opaque white creature for dark-floor legibility.

### mushroom

Use case stylized-concept. ONE original cute walking mushroom monster, isolated top-down dungeon sprite. Square genuine transparent PNG. Huge round dusty violet mushroom cap with three broad pale crescent-like flecks, squat ivory stem body, two tiny dark eyes and a determined friendly mouth, two tiny root feet. No hands holding things. Slight top-down three-quarter view shows cap and face. Polished hand-painted pixel-inspired RPG sprite, dark crisp outline, bold readable silhouette and broad color clusters at43px. Entire creature centered within middle65% square canvas with generous clear margins. Palette ivory/violet/dusky rose with cool moonlit top highlight, distinct from slime or ghost. No ground, no shadow, no spores/particles, no aura, no scenery, no text/border/watermark/checkerboard. Transparent alpha0 outside creature.

### mimic

Use case stylized-concept. ONE cute original mimic treasure-chest monster sprite, square transparent PNG for top-down gothic RPG. Compact midnight-brown wooden chest with simple aged gold metal bands, lid half open forming a friendly toothy mouth with only four rounded ivory teeth, two small violet eyes on lid, tiny claw feet. No tongue hanging out, no treasure outside, no blood. Slight overhead three-quarter view showing top and front. Polished hand-painted pixel-inspired RPG art, clean outline, broad shapes recognizable at43px. Whole chest centered within middle65% canvas with ample transparent margins. Distinct box-shaped silhouette. Genuine alpha0 outside chest, no floor/shadow/scene/particles, no text/border/watermark/checkerboard. Restrained navy/gold/violet palette.

### stone_golem

Use case stylized-concept. ONE original little stone golem monster sprite, isolated square genuinely transparent PNG. Cute squat chunky humanoid assembled from rounded slate-blue stone blocks, large head, tiny glowing cyan rectangular eyes, broad shoulder stones, stubby heavy arms and feet, small pale blue crystal set in chest. Friendly stubborn expression, no weapon. Three-quarter slightly overhead view for top-down dungeon. Polished hand-painted pixel-inspired RPG sprite, clean dark outline, broad faceted shading, readable43px silhouette distinct from blob/ghost. Entire golem centered in middle65% of canvas, ample transparent padding all sides. No rubble outside body, no ground/shadow/particles/aura, no scenery, no text/borders/watermark/checkerboard. Alpha0 outside golem. Slate gray navy with subtle purple cool moonlight.

### potion

Use case stylized-concept. ONE healing potion icon, original gothic fantasy RPG floor pickup and inventory asset, square PNG genuinely transparent alpha. A squat round clear glass bottle filled with ruby-rose healing liquid, short cork neck, simple antique-gold neck ring, small silver crescent charm on navy cord attached to bottle. Broad round readable silhouette, hand-painted crisp pixel-inspired RPG style with clean outline and broad highlights recognizable at43px. Center entire potion within middle65% canvas with generous padding. Slight overhead three-quarter view. No text/label, no extra bottles, no floor/shadow, no background, no particles/aura, no border/watermark/checkerboard. Transparent alpha0 outside the single bottle.

### return_scroll

Use case stylized-concept. ONE return-scroll item icon for original gothic moon RPG. Square genuine transparent PNG. A short ivory parchment scroll partly rolled at both ends, angled slightly from lower-left to upper-right, bound with navy ribbon and one round blue wax seal stamped with a simple silver crescent symbol. NO writing on parchment, no letters or numbers. Elegant antique-gold scroll-end caps, warm parchment against cool navy seal. Crisp hand-painted pixel-inspired RPG icon with broad simple shading and readable rolled-paper silhouette at43px. Slight top-down three-quarter angle. Center complete scroll inside middle65% canvas, generous transparent padding. No hands, no other objects, no ground/shadow/particles/background/border/watermark/checkerboard. True alpha0 outside scroll.

### depth_compass

Use case stylized-concept. ONE depth-compass relic icon for original gothic moon RPG inventory/floor drop, square genuine transparent PNG. A compact round antique gold pocket compass with dark midnight-blue face, bold silver four-point needle with one cyan-blue point, simple crescent accent at center, tiny top suspension ring. Slight overhead three-quarter view. Strong round silhouette and broad metal shapes readable at43px, hand-painted crisp pixel-inspired game icon, restrained detail and cool moonlight edge. Center entire compass within middle65% canvas with generous clear padding. NO numerals, letters, labels or any text. No long chain, no extra props, no hand, no floor/shadow, no magic particles/cloud, no background/border/watermark/checkerboard. Alpha0 outside compass.
