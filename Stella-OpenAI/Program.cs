using System.Runtime.InteropServices;
using Discord;
using Discord.Net;
using Discord.WebSocket;
using Newtonsoft.Json;
using OpenAI_API;
using OpenAI_API.Chat;
using OpenAI_API.Models;

namespace Stella_OpenAI;

internal class Program
{
    private OpenAIAPI? _api;
    private DiscordSocketClient? _client;
    private string? _tokenDiscord;
    private string? _tokenOpenAi;
    private readonly Dictionary<ulong, Conversation> _channelList = new ();
    private readonly List<ulong> _tweetChannelList = new();
    private const string Version = "0.5.0 GPT-4";

    private const string DefaultPrompt =
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
        try
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                _tokenDiscord = Environment.GetEnvironmentVariable("TOKEN_DISCORD", EnvironmentVariableTarget.User);
                _tokenOpenAi = Environment.GetEnvironmentVariable("TOKEN_OPENAI", EnvironmentVariableTarget.User);
                TwitterElement.BearToken= Environment.GetEnvironmentVariable("TOKEN_TWITTER", EnvironmentVariableTarget.User);
            }
            else
            {
                _tokenDiscord = Environment.GetEnvironmentVariable("TOKEN_DISCORD");
                _tokenOpenAi = Environment.GetEnvironmentVariable("TOKEN_OPENAI");
                TwitterElement.BearToken= Environment.GetEnvironmentVariable("TOKEN_TWITTER");
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
        
        _api = new OpenAIAPI(new APIAuthentication(_tokenOpenAi));
        _client = new DiscordSocketClient(new DiscordSocketConfig { GatewayIntents = GatewayIntents.All });
        _client.Log += Log;
        _client.Ready += Client_Ready;
        _client.SlashCommandExecuted += SlashCommandHandler;
        //終了時の処理
        AppDomain.CurrentDomain.ProcessExit += DisconnectService;
        await _client.LoginAsync(TokenType.Bot, _tokenDiscord);
        await _client.StartAsync();
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
        //有効なConversationかチェックする
        if (_channelList.ContainsKey(socketMessage.Channel.Id))
        {
#pragma warning disable CS4014
            Task.Run(() => SendChatGptPrompt(message));
#pragma warning restore CS4014
        }
        //Tweetが含まれているかチェック
        if (_tweetChannelList.Contains(socketMessage.Channel.Id))
        {
            var tweetElement = new TwitterElement();
            var tweet = await tweetElement.GetTweetFromUriAsync(message.Content, new CancellationToken());
            if (tweet != null)
            {
                var embed = await tweetElement.CreateTweetEmbed(tweet, new CancellationToken());
                await message.Channel.SendMessageAsync(embed: embed, messageReference: new MessageReference(message.Id));
            }

        }
    }
    private async Task SendChatGptSystemPrompt(SocketSlashCommand command)
    {
        try
        {
            _channelList[command.Channel.Id].AppendSystemMessage(command.Data.Options.First().Value.ToString());
            await _channelList[command.Channel.Id].GetResponseFromChatbotAsync();
            await command.FollowupAsync("更新しました");
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
        string response;
        var emote = Emote.Parse("<a:working:1085848442468827146>");
        // ReSharper disable once StringLiteralTypo
        var badReaction = Emote.Parse("<:zofinka:761499334654689300>");
        await message.AddReactionAsync(emote);
        try
        {
            _channelList[message.Channel.Id].AppendUserInput(prompt);
            var cts = new CancellationTokenSource();
            response = await Task.Run(() => _channelList[message.Channel.Id].GetResponseFromChatbotAsync(),
                cts.Token);
        }
        catch (Exception)
        {
            await message.RemoveReactionAsync(emote, _client?.CurrentUser);
            await message.AddReactionAsync(badReaction);
            return;
        }

        await message.Channel.SendMessageAsync(response, messageReference: new MessageReference(message.Id));
        if (_client != null) await message.RemoveReactionAsync(emote, _client.CurrentUser);
    }

    private async void EnableTalkInChannel(SocketInteraction command)
    {
        if (!_channelList.ContainsKey(command.Channel.Id))
        {
            _channelList.Add(command.Channel.Id, _api?.Chat.CreateConversation(new ChatRequest()
            {
                Model = Model.GPT4
            })!);
            _channelList[command.Channel.Id].AppendSystemMessage(DefaultPrompt);
        }
        _channelList[command.Channel.Id].AppendUserInput("こんにちは");
        var response = await _channelList[command.Channel.Id].GetResponseFromChatbotAsync();
        await command.FollowupAsync(response);
    }

    private async void DisableTalkInChannel(SocketInteraction command)
    {
        if (!_channelList.ContainsKey(command.Channel.Id))
        {
            await command.FollowupAsync("このチャンネルにStella-Chanは居なかったみたいです。");
            return;
        }
        _channelList.Remove(command.Channel.Id);
        await command.FollowupAsync("Stella-Chanは立ち去りました。");
    }

    private async void ResetConversation(SocketInteraction command)
    {
        _channelList[command.Channel.Id] = _api?.Chat.CreateConversation()!;
        _channelList[command.Channel.Id].AppendSystemMessage(DefaultPrompt);
        _channelList[command.Channel.Id].AppendUserInput("こんにちは");
        var response = await _channelList[command.Channel.Id].GetResponseFromChatbotAsync();
        await command.Channel.SendMessageAsync(response);
    }
    private async Task Client_Ready()
    {
        //resetコマンド
        var  resetCommand = new SlashCommandBuilder();
        resetCommand.WithName("reset");
        resetCommand.WithDescription("Stella-Chanの記憶を消します。若干性格が変わります。");

        //SystemMessageコマンド
        var systemCommand = new SlashCommandBuilder();
        systemCommand.WithName("system");
        systemCommand.WithDescription("System側のpromptを出します")
            .AddOption("prompt", ApplicationCommandOptionType.String, "ここにプロンプトを入力！", true);
        
        //enableCommand
        var enableCommand = new SlashCommandBuilder();
        enableCommand.WithName("enable");
        enableCommand.WithDescription("このチャンネルにStella-Chanを呼びます");
        
        //disableCommand
        var disableCommand = new SlashCommandBuilder();
        disableCommand.WithName("disable");
        disableCommand.WithDescription("このチャンネルのStella-Chanが居なくなります");
        //TweetEnable
        var tweetEnable = new SlashCommandBuilder();
        tweetEnable.WithName("tweet-enable");
        tweetEnable.WithDescription("TwitterのURLを投稿したときにステラちゃんが読みやすくしてくれます。");

        var tweetDisable = new SlashCommandBuilder();
        tweetDisable.WithName("tweet-disable");
        tweetDisable.WithDescription("ステラちゃんがTwitterを使うのをやめます。");
        

        var versionCommand = new SlashCommandBuilder();
        versionCommand.WithName("version");
        versionCommand.WithDescription("Stella-Chanのバージョンを表示します。");
        try
        {
            await _client?.CreateGlobalApplicationCommandAsync(resetCommand.Build())!;
            await _client?.CreateGlobalApplicationCommandAsync(systemCommand.Build())!;
            await _client?.CreateGlobalApplicationCommandAsync(enableCommand.Build())!;
            await _client?.CreateGlobalApplicationCommandAsync(disableCommand.Build())!;
            await _client?.CreateGlobalApplicationCommandAsync(versionCommand.Build())!;
            await _client?.CreateGlobalApplicationCommandAsync(tweetEnable.Build())!;
            await _client?.CreateGlobalApplicationCommandAsync(tweetDisable.Build())!;
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
                    //Task.Run(SetUpChatGpt);
#pragma warning restore CS4014
                    ResetConversation(command);
                    await command.RespondAsync("ステラちゃんの記憶を消しました！");
                    return;
                case "system":
                    await command.DeferAsync();
#pragma warning disable CS4014
                    Task.Run(() => SendChatGptSystemPrompt(command));
#pragma warning restore CS4014
                    return;
                case "enable":
                    //有効化するやつ
                    await command.DeferAsync();
                    EnableTalkInChannel(command);
                    return;
                case "disable":
                    //無効化するやつ
                    await command.DeferAsync();
                    DisableTalkInChannel(command);
                    return;
                case "version":
                    await command.RespondAsync(Version);
                    return;
                case "tweet-enable":
                    if(!_tweetChannelList.Contains(command.Channel.Id))
                        _tweetChannelList.Add(command.Channel.Id);
                    await command.RespondAsync("ステラちゃんがツイートを見せてくれるよ！");
                    break;
                case "tweet-disable":
                    if (_tweetChannelList.Contains(command.Channel.Id))
                        _tweetChannelList.RemoveAll(value => value == command.Channel.Id);
                    break;
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