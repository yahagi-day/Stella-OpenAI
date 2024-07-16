using Discord;
using Discord.Interactions;

namespace Stella_OpenAI.Discord;

public class ChatGptCommandModule : InteractionModuleBase<SocketInteractionContext>
{

    [SlashCommand("create-image", "Dell3を使ってステラちゃんがお絵描きしてくれます")]
    public async Task CreateImageWithDell3()
    {
        var mb = new ModalBuilder()
            .WithTitle("ステラちゃんにお絵描きしてもらおう!")
            .WithCustomId("CreateImage")
            .AddTextInput("何を描いてもらう？", "Prompt",TextInputStyle.Paragraph, "好きなものを書いてね！");
        await Context.Interaction.RespondWithModalAsync(mb.Build());
    }

    [SlashCommand("reset", "Stella-Chanの記憶を消します")]
    public async Task ResetConversation()
    {
        await Context.Interaction.DeferAsync();
        var response = await ChatGptClass.ResetConversationAsync(Context.Channel.Id);
        await Context.Interaction.FollowupAsync(response);
    }

    [SlashCommand("enable", "このチャンネルにStella-Chanを呼びます")]
    public async Task EnableConversation()
    {
        await Context.Interaction.DeferAsync();
        var response = await ChatGptClass.CreateConversationAsync(Context.Channel.Id);
        await Context.Interaction.FollowupAsync(response);
    }

    [SlashCommand("disable", "このチャンネルのStella-Chanが居なくなります")]
    public async Task DisableConversation()
    {
        var response = ChatGptClass.DeleteConversation(Context.Channel.Id);
        await Context.Interaction.RespondAsync(response);
    }
}