using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace Stella_OpenAI;

public class VoiceChannel
{
    public async Task JoinChannel(SocketSlashCommand command)
    {
        var channel = (command.User as IGuildUser)?.VoiceChannel;
        if(channel == null)
        {
            await command.RespondAsync("You must be in a voice channel to use this command.");
            return;
        }
        var audioClient = await channel.ConnectAsync();
    }
}