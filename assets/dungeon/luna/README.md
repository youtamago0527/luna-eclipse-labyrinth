# ルナ移動素材

`source-sheet.png` は背景込みの仕様シート原本です。ゲーム用の個別透過フレームではありません。

移動実装前に、32×32・同一基準点の透過PNGを `idle/{方向}/` と `walking/{方向}/` に用意し、`js/dungeon/asset-manifest.js` へ登録します。方向は `down / left / right / up` です。
