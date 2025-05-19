namespace WebAccess.DTO
{
    using System.Collections.Generic;
    using System.Text.Json.Serialization; // For [JsonPropertyName]

    public class LLMFullResponse
    {
        [JsonPropertyName("choices")]
        public List<Choice>? Choices { get; set; }

        [JsonPropertyName("object")]
        public string? ObjectType { get; set; } // "object" is a keyword in C#, so renamed or use attribute

        [JsonPropertyName("usage")]
        public UsageStats? Usage { get; set; }

        [JsonPropertyName("created")]
        public long Created { get; set; }

        [JsonPropertyName("system_fingerprint")]
        public string? SystemFingerprint { get; set; }

        [JsonPropertyName("model")]
        public string? Model { get; set; }

        [JsonPropertyName("id")]
        public string? Id { get; set; }
    }

    public class Choice
    {
        [JsonPropertyName("message")]
        public MessageContent? Message { get; set; }

        [JsonPropertyName("finish_reason")]
        public string? FinishReason { get; set; }

        [JsonPropertyName("index")]
        public int Index { get; set; }

        [JsonPropertyName("logprobs")]
        public object? Logprobs { get; set; } // Type could be more specific if known
    }

    public class MessageContent
    {
        [JsonPropertyName("role")]
        public string? Role { get; set; }

        [JsonPropertyName("content")]
        public string? Content { get; set; }
    }

    public class UsageStats
    {
        [JsonPropertyName("prompt_tokens")]
        public int PromptTokens { get; set; }

        [JsonPropertyName("completion_tokens")]
        public int CompletionTokens { get; set; }

        [JsonPropertyName("total_tokens")]
        public int TotalTokens { get; set; }

        [JsonPropertyName("prompt_tokens_details")]
        public PromptTokensDetails? PromptTokensDetails { get; set; }
    }

    public class PromptTokensDetails
    {
        [JsonPropertyName("cached_tokens")]
        public int CachedTokens { get; set; }
    }
}
