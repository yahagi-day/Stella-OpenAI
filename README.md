# Stella-OpenAI

[![Deploy to DegitalOcean](https://github.com/yahagi-day/Stella-OpenAI/actions/workflows/main.yml/badge.svg)](https://github.com/yahagi-day/Stella-OpenAI/actions/workflows/main.yml)
<a href="https://discord.com/api/oauth2/authorize?client_id=1086013762210893917&permissions=395328&scope=bot"><img alt="Invite Link" src="https://img.shields.io/badge/bot-invite-blueviolet"></a>

<img width="600" alt="Stella-Chan-Icon" src="image/ステラちゃん.png"></img>

このコードは、Discordで動作するチャットボット「ステラちゃん」を実装したC#プログラムです。OpenAIのo3-miniモデルとDALL-E 3を使用して、高度な会話AI機能と画像生成機能を提供します。上記の`bot Invite`からサーバーにBotを招待することができます。

## 技術仕様
- **.NET 8.0** を使用したクロスプラットフォーム対応
- **OpenAI o3-mini** による高性能な会話AI
- **DALL-E 3** による高品質な画像生成
- **チャンネル別会話管理** - 各Discordチャンネルで独立した会話履歴

## 使用しているライブラリ
- [Discord.Net 3.17.1](https://www.nuget.org/packages/Discord.Net/3.17.1)
- [Microsoft.Extensions.DependencyInjection 9.0.1](https://www.nuget.org/packages/Microsoft.Extensions.DependencyInjection/9.0.1)
- [OpenAI 2.1.0](https://www.nuget.org/packages/OpenAI/2.1.0)

## 環境変数
- `TOKEN_DISCORD`: Discord Botのトークンを設定します。
- `TOKEN_OPENAI`: OpenAI APIのトークンを設定します。

## 開発・デプロイ

### ローカル開発
```bash
# プロジェクトのビルド
dotnet build --configuration Release

# アプリケーションの実行
dotnet run --project Stella-OpenAI
```

### Docker使用
```bash
# Dockerイメージのビルド
docker-compose build

# Docker Composeで実行
docker-compose up -d
```

## SlashCommand
- `/version` : Stella-Chanのバージョンを表示します。
- `/enable` : Stella-Chanとの会話をコマンドを実行したチャネルで有効化します。
- `/disable` : Stella-Chanとの会話をコマンドを実行したチャンネルで無効化します。
- `/reset` : 実行したチャンネルのStella-ChanのConversationを初期化します。
- `/create-image` : Stella-Chanがお絵描きをしてくれます。実行するとModalが表示されるので書いて欲しい絵を入力してください。
- `/sudo` : 一時的な管理者権限を付与します（特定サーバーのみ）。

## キャラクター設定
ステラちゃんは以下の特徴を持つ魔法少女AIです：
- Unity、Maya、VR、C#開発に詳しい
- VTuber文化とストリーミングに精通
- FPSゲーム（特にOverwatch）が得意
- 音楽制作（DAW、DTM、Vocaloid）に造詣が深い
- アルゴリズムとプログラミングの専門知識
- 魔法をテーマにした個性的なキャラクター

## 自動デプロイ
GitHub ActionsによりDigitalOceanへの自動デプロイを実装。Cloudflareトンネルを経由してセキュアにデプロイが行われます。
