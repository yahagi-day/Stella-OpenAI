namespace Stella_OpenAI;

public class TweetObject
{
    public string? id { get; set; }
    public string? text { get; set; }
    public List<string>? edit_history_tweet_ids { get; set; }
    public object? attachments { get; set; }
    public string? author_id { get; set; }
    public object? context_annotations { get; set; }
    public string? conversation_id { get; set; }
    public DateTime? created_at { get; set; }
    public object? edit_controls { get; set; }
    public object? entities { get; set; }
    public string? in_reply_to_user_id { get; set; }
    public string? lang { get; set; }
    public object? non_public_metrics { get; set; }
    public object? organic_metrics { get; set; }
    public bool? possibly_sensitive { get; set; }
    public object? promoted_metrics { get; set; }
    public object? public_metrics { get; set; }
    public List<object>? referenced_tweets { get; set; }
    public string? reply_settings { get; set; }
    public string? source { get; set; }
    public object? withheld { get; set; }
}