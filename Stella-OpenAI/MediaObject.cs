// ReSharper disable InconsistentNaming
namespace Stella_OpenAI;

public class MediaObject
{
    public string? media_key { get; set; }
    public string? type { get; set; }
    public string? url { get; set; }
    public int? duration_ms { get; set; }
    public int? height { get; set; }
    public object? non_public_metrics { get; set; }
    public object? organic_metrics { get; set; }
    public string? preview_image_url { get; set; }
    public object? promoted_metrics { get; set; }
    public object? public_metrics { get; set; }
    public int? width { get; set; }
    public string? alt_text { get; set; }
    public List<object>? variants { get; set; }
}