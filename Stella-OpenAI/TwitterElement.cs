using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace Stella_OpenAI;

public class TwitterElement
{
    private readonly string? _bearToken = Environment.GetEnvironmentVariable("TOKEN_TWITTER");

    public async Task<Tweet?> GetTweetFromUriAsync(string text, CancellationToken cancellationToken)
    {
        if (!CheckTweetUrl(text))
        {
            return null;
        }
        //ツイートの内容を取得する
        var uri = new Uri(text);
        var path = uri.AbsolutePath;
        var segment = path.Split('/');
        if (segment.Length < 4)
            return null;
        try
        {
            var tweet = await GetTweetAsync(segment[3], cancellationToken);
            return tweet;
        }
        catch(Exception)
        {
            throw new Exception();
        }
    }
    //貼られてテキストがTwitterのURLかチェックする
    public static bool CheckTweetUrl(string text)
    {
        if (Uri.IsWellFormedUriString(text, UriKind.Absolute))
        {
            var isMatch = Regex.IsMatch(text, "^(https://(twitter)|(x).com)");
            return isMatch;
        }

        return false;
    }

    public async Task<Tweet> GetTweetAsync(string id, CancellationToken cancellationToken)
    {
        if (_bearToken == null)
        {
            cancellationToken.ThrowIfCancellationRequested();
            throw new OperationCanceledException();
        }
        var client = new HttpClient();
        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {_bearToken}");
        try
        {
            var response = await client.GetAsync($"https://api.twitter.com/2/tweets?ids={id}",
                cancellationToken: cancellationToken);
            var tweet = JsonConvert.DeserializeObject<TweetResponse>(response.Content.ToString() ?? throw new NullReferenceException());
            return tweet!.data.First();
        }
        catch(OperationCanceledException e)
        {
            Console.WriteLine(e);
            throw;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }

    }
}

public class TweetResponse
{
    public List<Tweet> data;
}

public abstract class Tweet
{
    public string author_id;
    public string created_at;
    public string id;
    public string text;
}