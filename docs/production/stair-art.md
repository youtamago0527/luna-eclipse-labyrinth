# 下降階段タイル

2026-09-30。imagegenスキル全文と共通prompt referencesを読み、built-in `image_gen` のgenerateモードで1点を新規生成。CLI/API代替なし。既存 `Assets/Resources/Art/Terrain/floor_single.png` を事前目視し、黒紺の石材と青い縁の色調をテキスト指定に反映。参照画像の切抜・画像加工・リサイズなし。

- 原本：`C:/Users/youta/.codex/generated_images/01a08128-c3f6-7ba0-a0c4-098171fe0c00/exec-7ec0d0bf-5a6a-421b-95d0-2ca82a255e14.png`
- Unity用コピー：`Assets/Resources/Art/Terrain/stairs_down.png`
- Resource ID：`Art/Terrain/stairs_down`
- 1254×1254 PNG。原本をそのままコピーし原本も保存。コード・Importer・Unity設定は変更していない。

## 生成プロンプト

```text
Use case: stylized-concept. Asset type: one square terrain tile for a top-down 2D dungeon game, not a concept-art scene. Generate a single full-bleed opaque square tile of a descending stone staircase cut into black-navy gothic stone floor. Five broad horizontal steps descend from the bottom entrance towards the dark recess at the top, strongly readable at 43 pixels wide. Camera almost directly overhead, orthographic, grid-aligned vertical sides and horizontal steps, NOT isometric, no vanishing-point perspective. Centered rectangular recessed stairwell occupies about 70 percent of tile width and 80 percent height. Outer perimeter on ALL four sides is continuous opaque flat dark slate-blue floor flush to square image edges, matching weathered blue-black slate with fine chips and subdued cool lunar edge highlights. Clear blue-gray step edges fade darker with descent, modest inner side-wall shadows convey depth. Very restrained tiny aged-gold marks in the stone corners only, no bright gold border. Painterly detailed game texture but large clear silhouettes; no ground outside tile, no blank margin, no transparency, no text, no letters, no labels, no characters, no props, no UI frame, no diamond shape, no glow bloom, no multiple tiles or grid sheet. One tile fills the entire image.
```

## 検品と制限

中央の暗い窪みと横長の段差、青黒い石材、四隅の控えめな金装飾を目視確認。四辺まで不透明な石床があり、余白・文字・人物・UIなし。正方形で非isometric、入口は画像下側。約5段という指定に対し見える段は6段程度だが、階段として読める構成のため再生成はしなかった。

内壁には深さ表現による軽い先細りがあり、厳密な無遠近図ではない。通常床と色調は近いが画素単位のシーム一致は保証しない。周囲に細分化した縁石を含むため、全体を1グリッドとして表示する。Unity実表示43pxでの判読性・明度・既存床との接続は親担当の描画接続後に要確認。原PNGは加工せず表示寸法をUnity側で合わせる。
