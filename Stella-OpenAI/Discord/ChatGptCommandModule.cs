using Discord;
using Discord.Interactions;

namespace Stella_OpenAI.Discord;

public class ChatGptCommandModule : InteractionModuleBase<SocketInteractionContext>
{

    public static SlashCommandModule Instance { get; } = new();


    [SlashCommand("create-image", "Dell3を使ってステラちゃんがお絵描きしてくれます")]
    public async Task CreateImageWithDell3()
        => await ChatGptClass.CreateImageCommand(Instance);

    [SlashCommand("reset", "Stella-Chanの記憶を消します")]
    public async Task ResetConversation()
        => throw new NotImplementedException();

    [SlashCommand("enable", "このチャンネルにStella-Chanを呼びます")]
    public async Task EnableConversation()
        => throw new NotImplementedException();

    [SlashCommand("disable", "このチャンネルのStella-Chanが居なくなります")]
    public async Task DisableConversation()
        => throw new NotImplementedException();
}