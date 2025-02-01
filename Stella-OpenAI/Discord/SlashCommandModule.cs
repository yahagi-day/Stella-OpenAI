using Discord.Interactions;

namespace Stella_OpenAI.Discord;

public class SlashCommandModule : InteractionModuleBase<SocketInteractionContext>
{
    private const string Version = "0.10.0 o3-mini";

    [SlashCommand("version", "Stella-Chanのバージョンを表示します")]
    public async Task VersionRespond()
        => await RespondAsync(Version);
}