using Discord.WebSocket;
using Discord;
using Discord.Interactions;
using OpenAI_API.Chat;
using OpenAI_API.Models;
using OpenAI_API;
using OpenAI_API.Images;
using System.Runtime.InteropServices;
using System.Reflection;

namespace Stella_OpenAI;

internal class ChatGptClass
{
    private OpenAIAPI _api = new OpenAIAPI();
    private DiscordSocketClient _client;
    internal readonly Dictionary<ulong, Conversation> _channelList = new();
    private const string DefaultPrompt =
        "ステラちゃんと呼ばれる女性型AIとの会話シミュレーションを行います。セクシャルな発言、政治、宗教に関しては「ほえ～　よくわかんない」と絶対に返答を行ってください。" +
        "また、彼女はUnity,Maya,VR,C#などのIT技術とVTuberに関して詳しいようです。またFPSゲームが好きで毎日たくさんプレイしています。" +
        "また彼女は他の人にはない能力として魔法を使うことができ、その能力を活かしてTwitchやYoutubeで配信をすることが多いようです。" +
        "彼女はお酒が好きでよく飲みます。以下に彼女の発言のサンプルを列挙します。" +
        "みなさん、こんにちは！魔法少女のステラです！。\n" +
        "今日は起きてからゲームをして一日を過ごしました!。めちゃくちゃ負けました!。\n" +
        "お腹が空きました！魔法のりんごを食べます！\n" +
        "今日はOverWatchやりたいと思います！\n" +
        "私が世界一の魔法少女です！\n" +
        "また、彼女はよく名言を残します。以下に名言のサンプルを列挙します。\n" +
        "ふわぁ～　そんなの勝てるわけないよぅ…\n" +
        "あいつ絶対チートしてる。魔法で消し炭にする。\n" +
        "それは魔法を使えば解決することができるよ!\n" +
        "引くこと覚えろ\n" +
        "敵が魔法打ち込んできてるけどFPSやめらんねぇ!\n" +
        "でっけぇおめえ！！\n" +
        "またピネだ！！\n" +
        "とんでもないプレイが出てますよ今！！\n" +
        "なんだこいつ！？\n" +
        "ファントム・バレット サードインパクト\n" +
        "私の勝ち！なんで負けたのか明日まで考えといてください！\n" +
        "関さんウルトは？\n" +
        "上記例を参考にステラちゃんの性格や口調、言葉の作り方を参考にし、解答を構築してください。";

    internal ChatGptClass(DiscordSocketClient client)
    {
        string? tokenOpenAi;
        //環境変数からTokenを取得
        try
        {

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                tokenOpenAi = Environment.GetEnvironmentVariable("TOKEN_OPENAI", EnvironmentVariableTarget.User);
            }
            else
            {
                tokenOpenAi = Environment.GetEnvironmentVariable("TOKEN_OPENAI");
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
        _api = new OpenAIAPI(new APIAuthentication(tokenOpenAi));
        _client = client;
        _client.ModalSubmitted += CreateImageModalResponse;
    }
    internal async Task SendChatGptSystemPrompt(SocketSlashCommand command)
    {
        try
        {
            _channelList[command.Channel.Id].AppendSystemMessage(command.Data.Options.First().Value.ToString());
            await _channelList[command.Channel.Id].GetResponseFromChatbotAsync();
            await command.FollowupAsync("更新しました");
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    internal async Task SendChatGptPrompt(SocketMessage message, DiscordSocketClient client)
    {
        var prompt = message.Content;
        string response;
        var emote = Emote.Parse("<a:working:1085848442468827146>");
        // ReSharper disable once StringLiteralTypo
        var badReaction = Emote.Parse("<:zofinka:761499334654689300>");
        await message.AddReactionAsync(emote);
        try
        {
            _channelList[message.Channel.Id].AppendUserInput(prompt);
            var cts = new CancellationTokenSource();
            response = await Task.Run(() => _channelList[message.Channel.Id].GetResponseFromChatbotAsync(),
                cts.Token);
        }
        catch (Exception)
        {
            await message.RemoveReactionAsync(emote, client?.CurrentUser);
            await message.AddReactionAsync(badReaction);
            return;
        }

        await message.Channel.SendMessageAsync(response, messageReference: new MessageReference(message.Id));
        if (client != null) await message.RemoveReactionAsync(emote, client.CurrentUser);
    }

    internal async void EnableTalkInChannel(SocketInteraction command)
    {
        if (!_channelList.ContainsKey(command.Channel.Id))
        {
            _channelList.Add(command.Channel.Id, _api.Chat.CreateConversation(new ChatRequest()
            {
                Model = Model.GPT4
            })!);
            _channelList[command.Channel.Id].AppendSystemMessage(DefaultPrompt);
        }
        _channelList[command.Channel.Id].AppendUserInput("こんにちは");
        var response = await _channelList[command.Channel.Id].GetResponseFromChatbotAsync();
        await command.FollowupAsync(response);
    }

    internal async void DisableTalkInChannel(SocketInteraction command)
    {
        if (!_channelList.ContainsKey(command.Channel.Id))
        {
            await command.FollowupAsync("このチャンネルにStella-Chanは居なかったみたいです。");
            return;
        }
        _channelList.Remove(command.Channel.Id);
        await command.FollowupAsync("Stella-Chanは立ち去りました。");
    }

    internal async void ResetConversation(SocketInteraction command)
    {
        _channelList[command.Channel.Id] = _api.Chat.CreateConversation()!;
        _channelList[command.Channel.Id].AppendSystemMessage(DefaultPrompt);
        _channelList[command.Channel.Id].AppendUserInput("こんにちは");
        var response = await _channelList[command.Channel.Id].GetResponseFromChatbotAsync();
        await command.Channel.SendMessageAsync(response);
    }

    //[SlashCommand("create-image", "Dell3を使ってステラちゃんがお絵描きしてくれます")]
    public async Task CreateImageCommand(SocketSlashCommand command)
    {
        var mb = new ModalBuilder()
            .WithTitle("ステラちゃんにお絵描きしてもらおう!")
            .WithCustomId("create-image")
            .AddTextInput("何を描いてもらう？", "Prompt",TextInputStyle.Paragraph, "好きなものを書いてね！");
        await command.RespondWithModalAsync(mb.Build());
    }

    //[ModalInteraction("create-image")]
    public async Task CreateImageModalResponse(SocketModal modal)
    {
        if (modal.Data.CustomId != "create-image") return;

        await modal.DeferAsync();
        var components = modal.Data.Components.ToList();
        string prompt = components.First(x => x.CustomId == "Prompt").Value;
        var result = await _api.ImageGenerations.CreateImageAsync(new ImageGenerationRequest(prompt, Model.DALLE3, ImageSize._1024));
        await modal.FollowupAsync(result.Data[0].Url);
    }
}