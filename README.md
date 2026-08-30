# ルナの不思議なダンジョン

「アルセーヌ」とは別に管理・公開する、独立したWebゲームです。

## ローカルで確認

```powershell
python -m http.server 8080
```

ブラウザで `http://localhost:8080` を開きます。

## GitHubへ接続

1. GitHubで空のリポジトリ `luna-mystery-dungeon` を作成します。
2. このフォルダで次を実行します（URLは作成したリポジトリのものに置換）。

```powershell
git remote add origin https://github.com/youtamago0527/luna-mystery-dungeon.git
git push -u origin main
```

## 公開URLを分ける

GitHub Pagesを使う場合、リポジトリの **Settings → Pages** で `main` ブランチのルートを公開します。
通常のURLは次の形になります。

`https://youtamago0527.github.io/luna-mystery-dungeon/`

Netlifyを使う場合も、この新しいGitHubリポジトリを別サイトとして登録します。公開フォルダはプロジェクトルート、ビルドコマンドは不要です。

## 管理方針

- 既存ゲームのファイル・Git履歴・公開設定を混ぜない
- 画像は `assets/`、ゲーム処理は将来 `js/` に分割する
- 公開URLが決まったら canonical / OGP URLを `index.html` に追加する

