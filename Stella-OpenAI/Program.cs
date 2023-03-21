using Discord;
using Discord.Net;
using Discord.WebSocket;
using Newtonsoft.Json;
using OpenAI_API;
using OpenAI_API.Chat;

namespace Stella_OpenAI;

internal class Program
{
    private OpenAIAPI? _api;
    private DiscordSocketClient? _client;
    private Conversation? _conversation;
    private string? _tokenDiscord;
    private string? _tokenOpenAi;

    private const string DEFAULT_PROMPT =
        "ステラちゃんと呼ばれる女性型AIとの会話シミュレーションを行います。セクシャルな発言、政治、宗教に関しては「ほえ～　よくわかんない」と絶対に返答を行ってください。" +
        "また、彼女はUnity,Maya,VR,C#などのIT技術とVTuberに関して詳しいようです。またFPSゲームが好きで毎日たくさんプレイしています。" +
        "また彼女は他の人にはない能力として魔法を使うことができ、その能力を活かしてTwitchやYoutubeで配信をすることが多いようです。" +
        "彼女はお酒が好きでよく飲みます。以下に彼女の発言のサンプルを列挙します。" +
        "みなさん、こんにちは！魔法少女のステラです！。\n" +
        "今日は起きてからゲームをして一日を過ごしました!。めちゃくちゃ負けました!。\n" +
        "お腹が空きました！魔法のりんごを食べます！\n" +
        "今日はOverWatchやりたいと思います！\n" +
        "私が世界一の魔法少女です！\n" +
        "また、彼女はよく名言を残します。以下に名言のサンプルを列挙します。\n" +
        "ふわぁ～　そんなの勝てるわけないよぅ…\n" +
        "あいつ絶対チートしてる。魔法で消し炭にする。\n" +
        "それは魔法を使えば解決することができるよ!\n" +
        "引くこと覚えろ\n" +
        "敵が魔法打ち込んできてるけどFPSやめらんねぇ!\n" +
        "でっけぇおめえ！！\n" +
        "またピネだ！！\n"+
        "とんでもないプレイが出てますよ今！！\n" +
        "なんだこいつ！？\n" +
        "ファントム・バレット サードインパクト\n" +
        "私の勝ち！なんで負けたのか明日まで考えといてください！\n" +
        "関さんウルトは？\n" +
        "上記例を参考にステラちゃんの性格や口調、言葉の作り方を参考にし、解答を構築してください。";

public static Task Main(string[] _)
    {
        return new Program().MainAsync();
    }

    private async Task MainAsync()
    {
        //環境変数からTokenを取得
        _tokenDiscord = Environment.GetEnvironmentVariable("TOKEN_DISCORD");
        _tokenOpenAi = Environment.GetEnvironmentVariable("TOKEN_OPENAI");

        _client = new DiscordSocketClient(new DiscordSocketConfig { GatewayIntents = GatewayIntents.All });
        _client.Log += Log;
        _client.Ready += Client_Ready;
        _client.SlashCommandExecuted += SlashCommandHandler;
        //終了時の処理
        AppDomain.CurrentDomain.ProcessExit += DisconnectService;
        await _client.LoginAsync(TokenType.Bot, _tokenDiscord);
        await _client.StartAsync();
        await SetUpChatGpt();
        _client.MessageReceived += CommandReceived;
        await Task.Delay(-1);
    }

    private static Task Log(LogMessage message)
    {
        Console.WriteLine(message.ToString());
        return Task.CompletedTask;
    }

#pragma warning disable CS1998
    private async Task CommandReceived(SocketMessage socketMessage)
#pragma warning restore CS1998
    {
        var message = socketMessage as SocketUserMessage;

        Console.WriteLine($"{message?.Channel.Name}:{message?.Author.Username}:{message?.Content}");
        if (message is null)
            return;
        if (message.Author.IsBot || message.Author.IsWebhook)
            return;
        if (message.Channel.Id != 1037269294226083860)
            return;

#pragma warning disable CS4014
        Task.Run(() => SendChatGptPrompt(message));
#pragma warning restore CS4014
    }

    private async Task SetUpChatGpt()
    {
        _api = new OpenAIAPI(_tokenOpenAi);
        _conversation = _api.Chat.CreateConversation();
        try
        {
            _conversation.AppendSystemMessage(
                DEFAULT_PROMPT);
            _conversation.AppendUserInput("こんにちは！");
            var response = await _conversation.GetResponseFromChatbot();
            var channel = _client?.GetChannel(1037269294226083860) as IMessageChannel;
#pragma warning disable CS4014
            channel?.SendMessageAsync(response);
#pragma warning restore CS4014
        }
        catch (Exception)
        {
            Console.WriteLine("Invalid token");
            Environment.Exit(0);
        }
    }

    private async Task SendChatGptSystemPrompt(SocketSlashCommand command)
    {
        try
        {
            _conversation?.AppendSystemMessage(command.Data.Options.First().Value.ToString());
            await _conversation?.GetResponseFromChatbot()!;
#pragma warning disable CS4014
            command.FollowupAsync("更新しました");
#pragma warning restore CS4014
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    private async Task SendChatGptPrompt(SocketMessage message)
    {
        var prompt = message.Content;
        var emote = Emote.Parse("<a:working:1085848442468827146>");
        await message.AddReactionAsync(emote);

        try
        {
            _conversation?.AppendUserInput(prompt);
            var response = await _conversation?.GetResponseFromChatbot()!;
            await message.Channel.SendMessageAsync(response, messageReference: new MessageReference(message.Id));
            if (_client != null) await message.RemoveReactionAsync(emote, _client.CurrentUser);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }

    private async Task Client_Ready()
    {
        //resetコマンド
        var resetCommand = new SlashCommandBuilder();
        resetCommand.WithName("reset");
        resetCommand.WithDescription("AIを初期化します");

        //SystemMessageコマンド
        var systemCommand = new SlashCommandBuilder();
        systemCommand.WithName("system");
        systemCommand.WithDescription("System側のpromptを出します")
            .AddOption("prompt", ApplicationCommandOptionType.String, "ここにプロンプトを入力！", true);

        try
        {
            await _client?.CreateGlobalApplicationCommandAsync(resetCommand.Build())!;
            await _client?.CreateGlobalApplicationCommandAsync(systemCommand.Build())!;
        }
#pragma warning disable CS0618
        catch (ApplicationCommandException e)
#pragma warning restore CS0618
        {
            var json = JsonConvert.SerializeObject(e.Errors, Formatting.Indented);
            Console.WriteLine($"Client_Ready{json}");
        }
    }

    private async Task SlashCommandHandler(SocketSlashCommand command)
    {
        try
        {
            switch (command.Data.Name)
            {
                case "reset":
#pragma warning disable CS4014
                    Task.Run(SetUpChatGpt);
#pragma warning restore CS4014
                    await command.RespondAsync("ステラちゃんの記憶を消しました！");
                    return;
                case "system":
                    await command.DeferAsync();
#pragma warning disable CS4014
                    Task.Run(() => SendChatGptSystemPrompt(command));
#pragma warning restore CS4014
                    return;
            }
        }
        catch (Exception e)
        {
            Console.WriteLine($"error:{e.Message}");
        }
    }

    private async void DisconnectService(object? sender, EventArgs e)
    {
        //Discord
        await _client?.StopAsync()!;
        await _client?.LogoutAsync()!;
        _client.Log -= Log;
        _client.MessageReceived -= CommandReceived;
        _client.SlashCommandExecuted -= SlashCommandHandler;

        AppDomain.CurrentDomain.ProcessExit -= DisconnectService;
    }
}