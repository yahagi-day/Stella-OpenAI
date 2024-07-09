using System.Reflection;
using System.Runtime.InteropServices;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;

namespace Stella_OpenAI;

public class Program
{
    private static IServiceProvider _serviceProvider;
    
    private DiscordSocketClient _client = new ();
    private string? _tokenDiscord;
    private InteractionService? _interactionService;


    public InteractionService InteractionService;

    private static IServiceProvider CreateProvider()
    {
        var config = new DiscordSocketConfig { GatewayIntents = GatewayIntents.All };
        var collection = new ServiceCollection()
            .AddSingleton(config)
            .AddSingleton<DiscordSocketClient>();
        return collection.BuildServiceProvider();
    }
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
        _client.ModalSubmitted += ChatGptClass.CreateImageModalResponse;

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
        _interactionService = new InteractionService(_client);
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
        _client.ModalSubmitted -= ChatGptClass.CreateImageModalResponse;

        AppDomain.CurrentDomain.ProcessExit -= DisconnectService;
    }
}
