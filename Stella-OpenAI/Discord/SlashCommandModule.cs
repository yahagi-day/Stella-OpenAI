using Discord;
using Discord.Interactions;

namespace Stella_OpenAI.Discord;

public class SlashCommandModule : InteractionModuleBase<SocketInteractionContext>
{
    public static SlashCommandModule Instance { get; } = new();

    public const string Version = "0.8.1 GPT-4 Omni";

    [SlashCommand("ping", "Get a pong")]
    public async Task PongCommand()
        => await RespondAsync("Pong!");

    [SlashCommand("version", "Stella-Chanのバージョンを表示します")]
    public async Task VersionRespond()
        => await RespondAsync(Version);
}