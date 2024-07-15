using Discord;
using Discord.WebSocket;

namespace Stella_OpenAI.Discord;

public class DiscordEventHandler
{
    public static async Task SendMessageFromGptAsync(SocketMessage message, DiscordSocketClient client, CancellationToken token = default)
    {
        var response = "";
        var emote = Emote.Parse("<a:working:1085848442468827146>");
        var badReaction = Emote.Parse("<:zofinka:761499334654689300>");

        await message.AddReactionAsync(emote);
        try
        {
            response = await ChatGptClass.SendChatGptPromptAsync(message.Content, message.Channel.Id, token);
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
}