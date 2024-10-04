using OpenAI.Chat;
using System.Runtime.InteropServices;
using OpenAI.Images;

namespace Stella_OpenAI;

public class ChatGptClass
{
    private readonly ChatClient _api;
    private readonly ImageClient _imageClient;
    private readonly List<ChatMessage> _messages = [];
    
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

    public ChatGptClass()
    {
        //環境変数からTokenを取得
        try
        {
            var tokenOpenAi = (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? Environment.GetEnvironmentVariable("TOKEN_OPENAI", EnvironmentVariableTarget.User) : Environment.GetEnvironmentVariable("TOKEN_OPENAI")) ?? throw new InvalidOperationException();
            _api = new ChatClient(model: "gpt-4o", apiKey: tokenOpenAi);
            _imageClient = new ImageClient("dall-e-3", tokenOpenAi);
            _messages.Add(new SystemChatMessage(DefaultPrompt));
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    public async Task<string> SendChatGptPromptAsync(IEnumerable<ChatMessage> messages, CancellationToken token = default)
    {
        try
        {
            _messages.AddRange(messages);
            var completion = await _api.CompleteChatAsync(_messages, cancellationToken: token);
            var list = completion.Value.Content.ToList();
            return list[0].Text;
        }
        catch(Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    public async Task<byte[]> CreateImageDataAsync(string prompt, CancellationToken token = default)
    {
        var option = new ImageGenerationOptions
        {
            Quality = GeneratedImageQuality.High,
            Size = GeneratedImageSize.W1024xH1024,
            Style = GeneratedImageStyle.Vivid,
            ResponseFormat = GeneratedImageFormat.Bytes
        };

        try
        {
            var image = await _imageClient.GenerateImageAsync(prompt, option, cancellationToken: token);
            var data = image.Value.ImageBytes;
            return data.ToArray();
        }
        catch(Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
}
