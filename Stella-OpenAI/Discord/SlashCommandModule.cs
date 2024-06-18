using Discord;
using Discord.Interactions;

namespace Stella_OpenAI.Discord;

public class SlashCommandModule : InteractionModuleBase<SocketInteractionContext>
{
    [SlashCommand("ping", "Get a pong")]
    public async Task PongCommand()
        => await RespondAsync("Pong!");
}