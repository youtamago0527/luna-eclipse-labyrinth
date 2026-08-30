# ルナ移動素材

`source-sheet.png` は背景込みの仕様シート原本です。ゲーム用の個別透過フレームではありません。

仕様シートから4方向×4枚の歩行フレームを `walking/{方向}/` に切り出し、暗い台紙部分を透明化してあります。現在の表示フレームは92×122で、方向は `down / left / right / up` です。

待機時は各方向の0番フレームを維持します。専用idle素材が追加された場合は `js/dungeon/asset-manifest.js` の `idle.framesByDirection` のみ差し替えます。
