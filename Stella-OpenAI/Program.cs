using System;
using System.Text.Json;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using OpenAI_API;
using OpenAI_API.Chat;
using OpenAI_API.Models;

class Program
{
    private DiscordSocketClient _client;
    public static CommandService _commands;
    public static IServiceProvider _services;

    public static Task Main(string[] args) => new Program().MainAsync();

    public async Task MainAsync()
    {
        _client = new DiscordSocketClient();
        _client.Log += Log;

        var token = "";

        await _client.LoginAsync(TokenType.Bot, token);
        await _client.StartAsync();
        await SetUpChatGPT();
        _client.MessageReceived += CommandRecieved;
        await Task.Delay(-1);
    }

    private Task Log(LogMessage message)
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
        var context = new CommandContext(_client, message);
        var emote = Emote.Parse("<a:working:1085848442468827146>");
        await message.AddReactionAsync(emote);
        var responseChat =  await SendChatGptPrompt(message.Content);

        await message.Channel.SendMessageAsync(responseChat);

        await message.RemoveReactionAsync(emote, _client.CurrentUser);
    }
    OpenAIAPI api = new OpenAIAPI("");
    Conversation conversation;

    private async Task SetUpChatGPT()
    {
        conversation = api.Chat.CreateConversation();
        conversation.AppendSystemMessage("あなたは架空のキャラクター「ステラちゃん」です。ステラちゃんになりきって質問に回答してください。");
    }
    private async Task<string> SendChatGptPrompt(string prompt)
    {
        conversation.AppendUserInput(prompt);
        var response = await conversation.GetResponseFromChatbot();

        return response;
    }
}