using OpenAI_API.Chat;
using OpenAI_API.Models;
using OpenAI_API;
using OpenAI_API.Images;
using System.Runtime.InteropServices;

namespace Stella_OpenAI;

public static class ChatGptClass
{
    private static readonly OpenAIAPI Api;
    public static readonly Queue<string> ModalQueue = new();
    public static readonly Dictionary<ulong, Conversation> ChannelList = new();
    
    private const string DefaultPrompt =
        "ステラちゃんと呼ばれる女性型AIとの会話シミュレーションを行います。" +
        "また、彼女はUnity,Maya,VR,C#などのIT技術とVTuberに関して詳しいようです。またFPSゲームが好きで毎日たくさんプレイしています。" +
        "また音楽について詳しくDAWを使った楽曲作成やDTM、Vocaloidにも精通しています。" +
        "またアルゴリズムについて非常に詳しく様々な計算方法に精通しています。いつも人に助言しています。" +
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

    static ChatGptClass()
    {
        string? tokenOpenAi;
        //環境変数からTokenを取得
        try
        {
            tokenOpenAi = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? Environment.GetEnvironmentVariable("TOKEN_OPENAI", EnvironmentVariableTarget.User) : Environment.GetEnvironmentVariable("TOKEN_OPENAI");
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
        Api = new OpenAIAPI(new APIAuthentication(tokenOpenAi));
    }

    public static async Task<string> SendChatGptPromptAsync(string message, ulong id, CancellationToken token = default)
    {
        ChannelList[id].AppendUserInput(message);
        var cts = new CancellationTokenSource();
        var response = await Task.Run(() => ChannelList[id].GetResponseFromChatbotAsync(),
            cts.Token);

        return response;
    }

    public static async Task<string> CreateConversationAsync(ulong id, CancellationToken token = default)
    {
        if (!ChannelList.TryGetValue(id, out var value))
        {
            value = Api.Chat.CreateConversation(new ChatRequest()
            {
                Model = "gpt-4o"
            })!;
            ChannelList.Add(id, value);
            ChannelList[id].AppendSystemMessage(DefaultPrompt);
        }

        value.AppendUserInput("こんにちは");

        var response = await value.GetResponseFromChatbotAsync();
        token.ThrowIfCancellationRequested();
        return response;
    }

    public static string DeleteConversation(ulong id)
    {
        if (!ChannelList.ContainsKey(id))
        {
            return "このチャンネルにStella-Chanは居なかったみたいです。";
        }
        ChannelList.Remove(id);
        return "ステラちゃんはどこかに行ってしまったようです";
    }

    public static async Task<string> ResetConversationAsync(ulong id, CancellationToken token = default)
    {
        ChannelList[id] = Api.Chat.CreateConversation()!;
        ChannelList[id].AppendSystemMessage(DefaultPrompt);
        ChannelList[id].AppendUserInput("こんにちは");
        var response = await ChannelList[id].GetResponseFromChatbotAsync();
        token.ThrowIfCancellationRequested();
        return response;
    }

    public static async Task<byte[]> CreateImageDataAsync(string prompt, CancellationToken token = default)
    {
        try
        {
            var result = await Api.ImageGenerations.CreateImageAsync(new ImageGenerationRequest(prompt, Model.DALLE3,
                ImageSize._1024, responseFormat: ImageResponseFormat.B64_json));
            token.ThrowIfCancellationRequested();
            //base64 imageを画像にする
            var bytes = Convert.FromBase64String(result.Data[0].Base64Data);
            return bytes;
        }
        catch (OperationCanceledException)
        {
            throw new OperationCanceledException();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw new Exception();
        }
    }
}
