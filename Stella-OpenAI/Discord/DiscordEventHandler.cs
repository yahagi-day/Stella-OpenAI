using Discord;
using Discord.WebSocket;
using OpenAI.Chat;

namespace Stella_OpenAI.Discord;

public static class DiscordEventHandler
{
    public static readonly Dictionary<ulong, ChatGptClass> GptClasses = new();
    private static ChatGptClass _gptClass = new();
    public static async Task SendMessageFromGptAsync(SocketMessage message, DiscordSocketClient client, CancellationToken token = default)
    {
        string response;
        var emote = Emote.Parse("<a:working:1085848442468827146>");
        var badReaction = Emote.Parse("<:zofinka:761499334654689300>");

        await message.AddReactionAsync(emote);
        try
        {
            var list = new List<ChatMessage> { new UserChatMessage(message.Content) };
            response = await GptClasses[message.Channel.Id].SendChatGptPromptAsync(list, token);
        }
        catch (Exception)
        {
            await message.RemoveReactionAsync(emote, client.CurrentUser);
            await message.AddReactionAsync(badReaction);
            return;
        }

        await message.Channel.SendMessageAsync(response, messageReference: new MessageReference(message.Id));
        await message.RemoveReactionAsync(emote, client.CurrentUser);
    }

    public static async Task SendCreateImageFromModal(SocketModal modal, CancellationToken token = default)
    {
        await modal.DeferAsync();
        var components = modal.Data.Components.ToList();
        var prompt = components.First(x => x.CustomId == "Prompt").Value;
        try
        {
            var bytes = await _gptClass.CreateImageDataAsync(prompt, token: token);
            var file = new List<FileAttachment> { new(new MemoryStream(bytes), $"image_{prompt}.webp") };
            await modal.FollowupWithFilesAsync(file, text: prompt);
        }
        catch (Exception)
        {
            await modal.FollowupAsync("ふわ～>< よくわかんないや…");
        }
    }
}