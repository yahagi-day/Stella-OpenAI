# CLAUDE.md

このファイルは、このリポジトリのコードを扱う際にClaude Code (claude.ai/code) に対するガイダンスを提供します。

## プロジェクト概要

Stella-OpenAIは、.NET 8.0とC#を使用して実装された「ステラちゃん」という名前のDiscordチャットボットです。このボットは、OpenAIの言語モデル（現在はo3-mini）とDALL-E 3画像生成をDiscord.Netと統合して、Discordサーバーで会話AIと画像生成機能を提供します。

## アーキテクチャ

### コアコンポーネント

- **Program.cs**: Discordクライアントを初期化し、イベントハンドラーを設定し、ボットのライフサイクルを管理するメインエントリーポイント
- **ChatGptClass.cs**: OpenAI .NET SDKを使用してチャット補完と画像生成の両方のOpenAI API相互作用を処理
- **Discord/**: Discord固有の機能を含む
  - **DiscordEventHandler.cs**: メッセージイベントを管理し、チャンネルごとのChatGPTインスタンスを維持
  - **SlashCommandModule.cs**: 基本的なスラッシュコマンド（version、sudo）を実装
  - **ChatGptCommandModule.cs**: ChatGPT関連のスラッシュコマンド（enable、disable、reset、create-image）を実装

### 主要なアーキテクチャパターン

- **チャンネルごとの状態管理**: 各Discordチャンネルは`DiscordEventHandler.GptClasses`辞書に格納された独自のChatGptClassインスタンスを持ち、個別の会話コンテキストを維持
- **イベント駆動処理**: ボットはDiscordイベント（メッセージ、スラッシュコマンド、モーダル送信）に非同期で応答
- **依存性注入**: サービス管理にMicrosoft.Extensions.DependencyInjectionを使用
- **環境ベースの設定**: DiscordとOpenAIトークンはWindows/Linux互換性を持つ環境変数から読み込まれる

## 開発コマンド

### ビルドと実行
```bash
# プロジェクトのビルド
dotnet build --configuration Release

# アプリケーションの実行
dotnet run --project Stella-OpenAI

# デプロイ用パブリッシュ
dotnet publish --configuration Release --output ./publish
```

### Dockerコマンド
```bash
# Dockerイメージのビルド
docker-compose build

# Docker Composeで実行
docker-compose up -d
```

## 環境変数

必須の環境変数:
- `TOKEN_DISCORD`: Discord botトークン
- `TOKEN_OPENAI`: OpenAI APIキー

オプションのDocker環境変数:
- `ASPNETCORE_ENVIRONMENT=Production`
- `DOTNET_ENVIRONMENT=Production`

## ボット機能

### スラッシュコマンド
- `/version`: ボットのバージョンを表示
- `/enable`: 現在のチャンネルでChatGPT会話を有効化
- `/disable`: 現在のチャンネルでChatGPT会話を無効化
- `/reset`: 現在のチャンネルの会話履歴をリセット
- `/create-image`: DALL-E 3画像生成用のモーダルを開く
- `/sudo`: 一時的な管理者ロール付与（サーバー固有）

### メッセージ処理
- ボットは有効化されたチャンネルの全ての非ボットメッセージに応答
- 各チャンネルは独立した会話履歴を維持
- ゲーム/技術/音楽テーマを持つカスタム「ステラちゃん」パーソナリティプロンプトを使用

## デプロイ

プロジェクトはGitHub Actionsを使用してCloudflareトンネル経由でDigitalOceanへの自動デプロイを行います。ワークフローはアプリケーションをビルド、パブリッシュし、rsyncとsystemctlを使用してサービス管理のためにデプロイします。

## キャラクター設定

ボットは「ステラちゃん」を体現し、以下の専門知識を持つ魔法少女AIキャラクターです：
- Unity、Maya、VR、C#開発
- VTuber文化とストリーミング
- FPSゲーム（特にOverwatch）
- 音楽制作（DAW、DTM、Vocaloid）
- アルゴリズムとプログラミング
- 魔法をテーマにした個性的な特徴