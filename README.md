# Stella-OpenAI
[![Deploy to DegitalOcean](https://github.com/yahagi-day/Stella-OpenAI/actions/workflows/main.yml/badge.svg)](https://github.com/yahagi-day/Stella-OpenAI/actions/workflows/main.yml)
<a href="https://discord.com/api/oauth2/authorize?client_id=1086013762210893917&permissions=395328&scope=bot"><img alt="Invite Link" src="https://img.shields.io/badge/bot-invite-blueviolet"></a>

このコードは、Discordで動作するチャットボット「ステラちゃん」を実装したC#プログラムです。

## 必要なライブラリ
- Discord.Net
- Newtonsoft.Json
- OpenAI_API

## 環境変数
- `TOKEN_DISCORD`: Discord Botのトークンを設定します。
- `TOKEN_OPENAI`: OpenAI APIのトークンを設定します。

## 機能
1. Discordサーバー上でチャットを受け付けます。
2. 受け取ったメッセージをOpenAI APIを利用して返答を生成します。
3. チャットボットの設定や振る舞いを制御するためのスラッシュコマンドを実装しています。

## メインクラスとメソッド
- `Program` クラスがメインの実装クラスです。
- `MainAsync()` メソッドで、Discordへの接続やイベントの設定を行っています。
- `Log()` メソッドは、ログメッセージをコンソールに出力します。
- `CommandRecieved()` メソッドは、メッセージ受信時にトリガーされ、OpenAI APIに入力を送信して応答を取得します。
- `SetUpChatGPT()` メソッドは、OpenAI APIの初期設定を行います。
- `SendChatGptSystemPrompt()` および `SendChatGptPrompt()` メソッドは、それぞれSystemメッセージおよびUserメッセージをOpenAI APIに送信し、応答を取得します。
- `Client_Ready()` メソッドは、Discordクライアントの準備が整った際に実行され、スラッシュコマンドの設定を行います。
- `SlashCommandHandler()` メソッドは、スラッシュコマンドが実行された際にトリガーされ、それに対応する処理を行います。
- `DisconnectService()` メソッドは、プログラム終了時にDiscordから切断し、イベントハンドラーを解除します。

## スラッシュコマンド
- `/reset`: AIの状態をリセットします。
- `/system`: System側のプロンプトを追加します。
