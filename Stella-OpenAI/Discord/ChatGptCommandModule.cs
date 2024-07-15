using Discord.Interactions;

namespace Stella_OpenAI.Discord;

public class ChatGptCommandModule : InteractionModuleBase<SocketInteractionContext>
{

    [SlashCommand("create-image", "Dell3を使ってステラちゃんがお絵描きしてくれます")]
    public async Task CreateImageWithDell3()
        => throw new NotImplementedException();

    [SlashCommand("reset", "Stella-Chanの記憶を消します")]
    public async Task ResetConversation()
        => await ChatGptClass.ResetConversationAsync(Context.Channel.Id);

    [SlashCommand("enable", "このチャンネルにStella-Chanを呼びます")]
    public async Task EnableConversation()
        => await ChatGptClass.CreateConversationAsync(Context.Channel.Id);

    [SlashCommand("disable", "このチャンネルのStella-Chanが居なくなります")]
    public void DisableConversation()
        => ChatGptClass.DeleteConversation(Context.Channel.Id);
}