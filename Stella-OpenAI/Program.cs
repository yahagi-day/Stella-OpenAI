using System.Reflection;
using System.Runtime.InteropServices;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Stella_OpenAI.Discord;

namespace Stella_OpenAI;

public class Program
{
#pragma warning disable CS8618
    private static IServiceProvider _serviceProvider;
#pragma warning restore CS8618

    private DiscordSocketClient _client = new();
    private string? _tokenDiscord;
    private InteractionService? _interactionService;
    private static ChannelStateManager _channelStateManager = new();

    private static IServiceProvider CreateProvider()
    {
        var config = new DiscordSocketConfig { GatewayIntents = GatewayIntents.All };
        var collection = new ServiceCollection()
            .AddSingleton(config)
            .AddSingleton<DiscordSocketClient>()
            .AddSingleton(_channelStateManager);
        return collection.BuildServiceProvider();
    }

    public static ChannelStateManager GetChannelStateManager() => _channelStateManager;
    public static Task Main(string[] _)
    {
        _serviceProvider = CreateProvider();
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


        _client = _serviceProvider.GetRequiredService<DiscordSocketClient>();
        _client.Log += Log;
        _client.Ready += Client_Ready;
        _client.MessageReceived += MessageReceivedEventAsync;
        //Modalを使っているのがCreateImageしかないからこれでいいがそのうち汎用化する
        _client.ModalSubmitted += ModalSubmittedEventAsync;

        //終了時の処理
        AppDomain.CurrentDomain.ProcessExit += DisconnectService;

        await _client.LoginAsync(TokenType.Bot, _tokenDiscord);
        await _client.StartAsync();

        await Task.Delay(-1);
    }

    private static Task Log(LogMessage message)
    {
        Console.WriteLine(message.ToString());
        return Task.CompletedTask;
    }

    private async Task Client_Ready()
    {
        // チャンネル状態を復元
        await _channelStateManager.LoadStateAsync();

        // 保存されたチャンネルにChatGptClassを復元
        var enabledChannels = _channelStateManager.GetEnabledChannels();
        foreach (var channelId in enabledChannels)
        {
            if (!DiscordEventHandler.GptClasses.ContainsKey(channelId))
            {
                DiscordEventHandler.GptClasses.TryAdd(channelId, new ChatGptClass());
            }
        }

        Console.WriteLine($"復元されたチャンネル数: {enabledChannels.Length}");

        _interactionService = new InteractionService(_client);
        //InteractionModuleBaseを参照しているクラスを取得して登録している
        await _interactionService.AddModulesAsync(Assembly.GetEntryAssembly(), _serviceProvider);
        await _interactionService.RegisterCommandsGloballyAsync();

        _client.InteractionCreated += async (x) =>
        {
            var ctx = new SocketInteractionContext(_client, x);
            await _interactionService.ExecuteCommandAsync(ctx, _serviceProvider);
        };
    }

    private async void DisconnectService(object? sender, EventArgs e)
    {
        //Discord
        await _client.StopAsync()!;
        await _client.LogoutAsync()!;
        _client.Log -= Log;
        _client.MessageReceived -= MessageReceivedEventAsync;
        //_client.ModalSubmitted -= ChatGptClass.CreateImageModalResponse;
        AppDomain.CurrentDomain.ProcessExit -= DisconnectService;
    }

    private Task MessageReceivedEventAsync(SocketMessage message)
    {
        if (DiscordEventHandler.GptClasses.ContainsKey(message.Channel.Id) && !message.Author.IsBot && !message.Author.IsWebhook)
        {
#pragma warning disable CS4014
            DiscordEventHandler.SendMessageFromGptAsync(message, _client);
#pragma warning restore CS4014
        }

        return Task.CompletedTask;
    }

    private Task ModalSubmittedEventAsync(SocketModal modal)
    {
        if (modal.Data.CustomId == "CreateImage")
        {
#pragma warning disable CS4014
            DiscordEventHandler.SendCreateImageFromModal(modal);
#pragma warning restore CS4014
        }
        return Task.CompletedTask;
    }
}
