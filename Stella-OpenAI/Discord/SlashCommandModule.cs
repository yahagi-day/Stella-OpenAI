using Discord;
using Discord.Interactions;

namespace Stella_OpenAI.Discord;

public class SlashCommandModule : InteractionModuleBase<SocketInteractionContext>
{
    public const string Version = "0.8.1 GPT-4 Omni";

    [SlashCommand("ping", "Get a pong")]
    public async Task PongCommand()
        => await RespondAsync("Pong!");

    [SlashCommand("create-image", "Dell3を使ってステラちゃんがお絵描きしてくれます")]
    public async Task CreateImageWithDell3()
        => throw new NotImplementedException();

    [SlashCommand("reset", "Stella-Chanの記憶を消します")]
    public async Task ResetConversation()
        => throw new NotImplementedException();

    [SlashCommand("enable", "このチャンネルにStella-Chanを呼びます")]
    public async Task EnableConversation()
        => throw new NotImplementedException();

    [SlashCommand("disable", "このチャンネルのStella-Chanが居なくなります")]
    public async Task DisableConversation()
        => throw new NotImplementedException();

    [SlashCommand("version", "Stella-Chanのバージョンを表示します")]
    public async Task VersionRespond()
        => await RespondAsync(Version);
}