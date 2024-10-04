using Discord;
using Discord.Interactions;
using OpenAI.Chat;

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
        var contain = DiscordEventHandler.GptClasses.ContainsKey(Context.Channel.Id);
        if (!contain)
        {
            await Context.Interaction.FollowupAsync("ここにステラちゃんは居ないみたい");
        }
        var status = DiscordEventHandler.GptClasses.TryAdd(Context.Channel.Id, new ChatGptClass());
        if (status)
        {
            var message = new UserChatMessage( "こんにちは");
            var response = await DiscordEventHandler.GptClasses[Context.Channel.Id].SendChatGptPromptAsync(new []{message});
            await Context.Interaction.FollowupAsync(response);
        }
        await Context.Interaction.FollowupAsync("ふわー>< なんか壊れちゃった");
    }

    [SlashCommand("enable", "このチャンネルにStella-Chanを呼びます")]
    public async Task EnableConversation()
    {
        await Context.Interaction.DeferAsync();
        var status = DiscordEventHandler.GptClasses.TryAdd(Context.Channel.Id, new ChatGptClass());
        if (status)
        {
            var message = new UserChatMessage( "こんにちは");
            var response = await DiscordEventHandler.GptClasses[Context.Channel.Id].SendChatGptPromptAsync(new []{message});
            await Context.Interaction.FollowupAsync(response);
        }

    }

    [SlashCommand("disable", "このチャンネルのStella-Chanが居なくなります")]
    public async Task DisableConversation()
    {
        var contain = DiscordEventHandler.GptClasses.ContainsKey(Context.Channel.Id);
        if (!contain)
        {
            await Context.Interaction.RespondAsync("ここにステラちゃんは居ないみたい");
        }
        var response = DiscordEventHandler.GptClasses.Remove(Context.Channel.Id);
        if (response)
        {
            await Context.Interaction.RespondAsync("ステラちゃんはご飯を食べに行きました");
        }
        else
        {
            await Context.Interaction.RespondAsync("ふわー>< なんか壊れちゃった");
        }
    }
}