using System.Runtime.InteropServices;
using Discord;
using Discord.Interactions;
using Discord.Net;
using Discord.WebSocket;
using Newtonsoft.Json;
namespace Stella_OpenAI;

internal class Program : InteractionModuleBase
{
    private DiscordSocketClient _client = new ();
    private ChatGptClass? _chatGptClass;
    private string? _tokenDiscord;
    private const string Version = "0.8.0 GPT-4 Omuni";

    public static Task Main(string[] _)
    {
        return new Program().MainAsync();
    }

    private async Task MainAsync()
    {
        //環境変数からTokenを取得
        try
        {
            _tokenDiscord = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? Environment.GetEnvironmentVariable("TOKEN_DISCORD", EnvironmentVariableTarget.User) : Environment.GetEnvironmentVariable("TOKEN_DISCORD");
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
        

        _client = new DiscordSocketClient(new DiscordSocketConfig { GatewayIntents = GatewayIntents.All });
        _client.Log += Log;
        _client.Ready += Client_Ready;
        _client.SlashCommandExecuted += SlashCommandHandler;

        _chatGptClass = new ChatGptClass(_client);
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
#pragma warning disable CS8602 // null 参照の可能性があるものの逆参照です。
        if (_chatGptClass.ChannelList.ContainsKey(socketMessage.Channel.Id))
        {
#pragma warning disable CS4014
            Task.Run(() => _chatGptClass?.SendChatGptPrompt(message, _client));
#pragma warning restore CS4014
        }
#pragma warning restore CS8602 // null 参照の可能性があるものの逆参照です。
    }
    private async Task Client_Ready()
    {
        /*
        //create-image
        var createImage = new SlashCommandBuilder();
        createImage.WithName("create-image");
        createImage.WithDescription("Dell3を使ってステラちゃんがお絵描きしてくれます");
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

        var versionCommand = new SlashCommandBuilder();
        versionCommand.WithName("version");
        versionCommand.WithDescription("Stella-Chanのバージョンを表示します。");

        try
        {
            await _client.CreateGlobalApplicationCommandAsync(createImage.Build())!;
            await _client.CreateGlobalApplicationCommandAsync(resetCommand.Build())!;
            await _client.CreateGlobalApplicationCommandAsync(systemCommand.Build())!;
            await _client.CreateGlobalApplicationCommandAsync(enableCommand.Build())!;
            await _client.CreateGlobalApplicationCommandAsync(disableCommand.Build())!;
            await _client.CreateGlobalApplicationCommandAsync(versionCommand.Build())!;
        }
#pragma warning disable CS0618
        catch (ApplicationCommandException e)
#pragma warning restore CS0618
        {
            var json = JsonConvert.SerializeObject(e.Errors, Formatting.Indented);
            Console.WriteLine($"Client_Ready{json}");
        }
        */
    }

    private async Task SlashCommandHandler(SocketSlashCommand command)
    {
        try
        {
            switch (command.Data.Name)
            {
                case "create-image":
                    _chatGptClass?.CreateImageCommand(command);
                    return;
                case "reset":
                    _chatGptClass?.ResetConversation(command);
                    await command.RespondAsync("ステラちゃんの記憶を消しました！");
                    return;
                case "system":
                    await command.DeferAsync();
#pragma warning disable CS4014
                    Task.Run(() => _chatGptClass?.SendChatGptSystemPrompt(command));
#pragma warning restore CS4014
                    return;
                case "enable":
                    //有効化するやつ
                    await command.DeferAsync();
                    _chatGptClass?.EnableTalkInChannel(command);
                    return;
                case "disable":
                    //無効化するやつ
                    await command.DeferAsync();
                    _chatGptClass?.DisableTalkInChannel(command);
                    return;
                case "version":
                    await command.RespondAsync(Version);
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
        await _client.StopAsync()!;
        await _client.LogoutAsync()!;
        _client.Log -= Log;
        _client.MessageReceived -= CommandReceived;
        _client.SlashCommandExecuted -= SlashCommandHandler;

        AppDomain.CurrentDomain.ProcessExit -= DisconnectService;
    }
}