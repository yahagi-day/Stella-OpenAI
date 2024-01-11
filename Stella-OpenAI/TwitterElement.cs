using System.Text.RegularExpressions;
using Discord;
using Newtonsoft.Json;

namespace Stella_OpenAI;

public class TwitterElement
{
    // ReSharper disable once UnassignedReadonlyField
#pragma warning disable CA2211
    public static string? BearToken;
#pragma warning restore CA2211

    public async Task<Embed> CreateTweetEmbed(Tweet tweet, CancellationToken cancellationToken)
    {
        //プロフィールを取得する
        var user =  await GetUserDataAsync(tweet.Author_Id, cancellationToken);
        var embed = new EmbedBuilder();
        embed.WithAuthor(user.Username, user.Profile_image_url, $"https://twitter.com/i/web/status/{tweet.Id}");
        embed.WithDescription(tweet.Text);
        return embed.Build();
    }
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
            var isMatch = Regex.IsMatch(text, "^(https://(twitter|x).com)");
            return isMatch;
        }

        return false;
    }
    public async Task<User> GetUserDataAsync(string id, CancellationToken cancellationToken)
    {
        if (BearToken == null)
        {
            cancellationToken.ThrowIfCancellationRequested();
            throw new OperationCanceledException();
        }
        var client = new HttpClient();
        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {BearToken}");
        var response = await client.GetAsync($"https://api.twitter.com/2/users/{id}?user.fields=username,profile_image_url",
            cancellationToken: cancellationToken);
        var responseString = await response.Content.ReadAsStringAsync(cancellationToken);
        var user = JsonConvert.DeserializeObject<UserResponse>(responseString);
        return user!.Data;
    }
    public async Task<Tweet> GetTweetAsync(string id, CancellationToken cancellationToken)
    {
        if (BearToken == null)
        {
            cancellationToken.ThrowIfCancellationRequested();
            throw new OperationCanceledException();
        }
        var client = new HttpClient();
        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {BearToken}");
        
        try
        {
            var response = await client.GetAsync($"https://api.twitter.com/2/tweets?ids={id}&expansions=author_id",
                cancellationToken: cancellationToken);
            var responseString = await response.Content.ReadAsStringAsync(cancellationToken);
            var tweet = JsonConvert.DeserializeObject<TweetResponse>(responseString);
            return tweet!.Data.First();
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
    public List<Tweet> Data { get; set; }
}

public class Tweet
{
    public List<string> EdiHistoryTweetIds { get; set; }
    public string Id { get; set; }
    public string Author_Id { get; set; }
    public string Text { get; set; }
}

public class UserResponse
{
    public User Data { get; set; }
}

public class User
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Username { get; set; }
    public string Profile_image_url { get; set; }
}