using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ITStockM.WebApi.Services
{
    /// <summary>
    /// Service to communicate with Google's Gemini API.
    /// </summary>
    public class GeminiChatService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<GeminiChatService> _logger;
        private readonly string? _apiKey;
        private const string Model = "gemini-2.5-flash";

        public GeminiChatService(HttpClient httpClient, IConfiguration configuration, ILogger<GeminiChatService> logger)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _logger = logger;

            // Read API key from environment variable or configuration
            _apiKey = configuration["GeminiApi:ApiKey"] ?? Environment.GetEnvironmentVariable("GEMINI_API_KEY");
        }

        public async Task<string> SendMessageAsync(string userMessage, IEnumerable<ChatMessage>? conversationHistory = null)
        {
            if (string.IsNullOrWhiteSpace(_apiKey))
            {
                throw new InvalidOperationException("Gemini API key is not configured. Please set GEMINI_API_KEY environment variable.");
            }

            if (string.IsNullOrWhiteSpace(userMessage))
            {
                throw new ArgumentException("Message cannot be empty.", nameof(userMessage));
            }

            try
            {
                var request = BuildRequest(userMessage, conversationHistory);
                var url = $"https://generativelanguage.googleapis.com/v1beta/models/{Model}:generateContent?key={_apiKey}";

                var response = await _httpClient.PostAsJsonAsync(url, request);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Gemini API error: {StatusCode} - {Content}", response.StatusCode, errorContent);
                    throw new HttpRequestException($"Gemini API request failed with status {response.StatusCode}");
                }

                var jsonContent = await response.Content.ReadAsStringAsync();
                var result = System.Text.Json.JsonSerializer.Deserialize<GeminiResponse>(jsonContent);

                if (result?.Candidates?.FirstOrDefault()?.Content?.Parts?.FirstOrDefault()?.Text is string assistantMessage)
                {
                    return assistantMessage;
                }

                throw new InvalidOperationException("Unexpected response format from Gemini API");
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP error calling Gemini API");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling Gemini API");
                throw;
            }
        }

        private GeminiRequest BuildRequest(string userMessage, IEnumerable<ChatMessage>? conversationHistory)
        {
            var contents = new List<GeminiContent>();

            // Add conversation history if provided
            if (conversationHistory != null)
            {
                foreach (var msg in conversationHistory.Where(m => !string.IsNullOrWhiteSpace(m.Content)))
                {
                    contents.Add(new GeminiContent
                    {
                        Role = msg.Sender.Equals("user", StringComparison.OrdinalIgnoreCase) ? "user" : "model",
                        Parts = new[] { new GeminiPart { Text = msg.Content } }
                    });
                }
            }

            // Add current user message
            contents.Add(new GeminiContent
            {
                Role = "user",
                Parts = new[] { new GeminiPart { Text = userMessage } }
            });

            return new GeminiRequest { Contents = contents };
        }
    }

    #region Gemini API Models

    public class GeminiRequest
    {
        [JsonPropertyName("contents")]
        public List<GeminiContent> Contents { get; set; } = new();
    }

    public class GeminiContent
    {
        [JsonPropertyName("role")]
        public string Role { get; set; } = string.Empty;

        [JsonPropertyName("parts")]
        public GeminiPart[] Parts { get; set; } = Array.Empty<GeminiPart>();
    }

    public class GeminiPart
    {
        [JsonPropertyName("text")]
        public string Text { get; set; } = string.Empty;
    }

    public class GeminiResponse
    {
        [JsonPropertyName("candidates")]
        public GeminiCandidate[]? Candidates { get; set; }
    }

    public class GeminiCandidate
    {
        [JsonPropertyName("content")]
        public GeminiContent? Content { get; set; }
    }

    #endregion
}
