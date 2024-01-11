namespace Stella_OpenAI;

public abstract class ChatGptClass
{
    [Serializable]
    public class ChatGPTMessageModel
    {
        public string role;
        public string? content;
    }

//ChatGPT APIにRequestを送るためのJSON用クラス
    [Serializable]
    public class ChatGPTCompletionRequestModel
    {
        public string model;
        public List<ChatGPTMessageModel?> messages;
    }

//ChatGPT APIからのResponseを受け取るためのクラス
    [System.Serializable]
    public class ChatGPTResponseModel
    {
        public string id;
        public string @object;
        public int created;
        public Choice[] choices;
        public Usage usage;

        [System.Serializable]
        public class Choice
        {
            public int index;
            public ChatGPTMessageModel message;
            public string finish_reason;
        }

        [System.Serializable]
        public class Usage
        {
            public int prompt_tokens;
            public int completion_tokens;
            public int total_tokens;
        }
    }

}