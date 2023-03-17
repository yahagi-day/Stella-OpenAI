using Discord;
using Discord.Commands;
using Discord.Net;
using Discord.WebSocket;
using Newtonsoft.Json;
using OpenAI_API;
using OpenAI_API.Chat;

namespace Stella_OpenAI;

internal class Program
{
    private DiscordSocketClient _client;
    public static CommandService _commands;
    public static IServiceProvider _services;
    private string _tokenDiscord;
    private string _tokenOpenAi;

    public static Task Main(string[] args) => new Program().MainAsync();

    private async Task MainAsync()
    {
        try
        {
            _tokenDiscord = Environment.GetEnvironmentVariable("TOKEN_DISCORD", EnvironmentVariableTarget.User);
            _tokenOpenAi = Environment.GetEnvironmentVariable("TOKEN_OPENAI", EnvironmentVariableTarget.User);
        }
        catch (Exception ex)
        {
            Environment.Exit(0);
        }
        //環境変数からTokenを設定
        

        _client = new DiscordSocketClient(new DiscordSocketConfig() { GatewayIntents = GatewayIntents.All });
        _client.Log += Log;
        _client.Ready += Client_Ready;
        _client.SlashCommandExecuted += SlashCommandHandler;
        //終了時の処理
        AppDomain.CurrentDomain.ProcessExit += DisconnectService;
        await _client.LoginAsync(TokenType.Bot, _tokenDiscord);
        await _client.StartAsync();
        await SetUpChatGPT();
        _client.MessageReceived += CommandRecieved;
        await Task.Delay(-1);
    }

    private static Task Log(LogMessage message)
    {
        Console.WriteLine(message.ToString());
        return Task.CompletedTask;
    }

    private async Task CommandRecieved(SocketMessage socketMessage)
    {
        var message = socketMessage as SocketUserMessage;

        Console.WriteLine($"{message.Channel.Name}:{message.Author.Username}:{message.Content}");
        if (message is null)
            return;
        if (message.Author.IsBot || message.Author.IsWebhook)
            return;
        if (message.Channel.Id != 1085891579585249351)
            return;

        Task.Run(() => SendChatGptPrompt(message));
    }

    OpenAIAPI api;
    Conversation conversation;

    private async Task SetUpChatGPT()
    {
        api = new OpenAIAPI(_tokenOpenAi);
        conversation = api.Chat.CreateConversation();
        try
        {
            conversation.AppendSystemMessage("あなたは架空のキャラクター「ステラちゃん」です。ステラちゃんになりきって質問に回答してください。一人称は私です。おとなしめな性格です。あなたは魔法を使うことができます。相手が対価を払ったときに気まぐれで魔法を使ってあげることがあります。");
            conversation.AppendUserInput("こんにちは！");
            var response = await conversation.GetResponseFromChatbot();
            var channel = _client.GetChannel(1085891579585249351) as IMessageChannel;
            channel.SendMessageAsync(response);
        }
        catch (Exception _)
        {
            Console.WriteLine($"Invalid token");
            Environment.Exit(0);
        }

    }

    private async Task SendChatGptSystemPrompt(SocketSlashCommand command)
    {
        try
        {
            conversation.AppendSystemMessage(command.Data.Options.First().Value.ToString());
            var response = await conversation.GetResponseFromChatbot();
            command.FollowupAsync($"更新しました");
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
            conversation.AppendUserInput(prompt);
            var response = await conversation.GetResponseFromChatbot();
            await message.Channel.SendMessageAsync(response, messageReference: new MessageReference(messageId: message.Id));
            await message.RemoveReactionAsync(emote, _client.CurrentUser);
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
        var SystemCommand = new SlashCommandBuilder();
        SystemCommand.WithName("system");
        SystemCommand.WithDescription("System側のpromptを出します")
            .AddOption("prompt", ApplicationCommandOptionType.String, "ここにプロンプトを入力！", true);

        try
        {
            await _client.CreateGlobalApplicationCommandAsync(resetCommand.Build());
            await _client.CreateGlobalApplicationCommandAsync(SystemCommand.Build());
        }
        catch (ApplicationCommandException e)
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
                    Task.Run(() => SetUpChatGPT());
                    await command.RespondAsync($"ステラちゃんの記憶を消しました！");
                    return;
                case "system":
                    await command.DeferAsync();
                    Task.Run(() => SendChatGptSystemPrompt(command));
                    return;
            }
        }
        catch (Exception e)
        {
            Console.WriteLine($"error:{e.Message}");
        }
    }

    private async void DisconnectService(object sender, EventArgs e)
    {
        //Discord
        await _client.StopAsync();
        await _client.LogoutAsync();
        _client.Log -= Log;
        _client.MessageReceived -= CommandRecieved;
        _client.SlashCommandExecuted -= SlashCommandHandler;

        AppDomain.CurrentDomain.ProcessExit -= DisconnectService;
    }
}
