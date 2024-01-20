namespace Stella_OpenAI;

public class UserObject
{
    public string? id { get; set; }
    public string? name { get; set; }
    public string? username { get; set; }
    public List<string>? connection_status { get; set; }
    public DateTime? created_at { get; set; }
    public string? description { get; set; }
    public object? entities { get; set; }
    public string? location { get; set; }
    public string? pinned_tweet_id { get; set; }
    public string? profile_image_url { get; set; }
    public bool? @protected { get; set; }
    public object? public_metrics { get; set; }
    public string? url { get; set; }
    public bool? verified { get; set; }
    public object? withheld { get; set; }
}