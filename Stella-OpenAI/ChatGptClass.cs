// ReSharper disable InconsistentNaming
// ReSharper disable UnassignedField.Global
namespace Stella_OpenAI;

public abstract class ChatGptClass
{
    [Serializable]
    public class ChatGptResponseMessageModel
    {
        public string? role;
        public string? content;
    }
    
    public class ChatGptMessageModel
    {
        public string? role;
        public List<ChatGptMessageModelContent> content = null!;
    }

    [Serializable]
    public class ChatGptMessageModelContent
    {
        public string? type;
        public string? text;
        public string? image_url;
    }
//ChatGPT APIにRequestを送るためのJSON用クラス
    [Serializable]
    public class ChatGptCompletionRequestModel
    {
        public string? model;
        public List<ChatGptMessageModel?>? messages;
        public int max_tokens;
    }

//ChatGPT APIからのResponseを受け取るためのクラス
    [Serializable]
    public class ChatGptResponseModel
    {
        public string? id;
        public string? @object;
        public int created;
        public string? model;
        public Usage? usage;
        public Choice[]? choices;

        [Serializable]
        public class Choice
        {
            public ChatGptResponseMessageModel? message;
            public FinishDetails? finish_details;
            public int index;
        }

        [Serializable]
        public class FinishDetails
        {
            public string? type;
            public string? stop;
        }
        [Serializable]
        public class Usage
        {
            public int prompt_tokens;
            public int completion_tokens;
            public int total_tokens;
        }
    }

}