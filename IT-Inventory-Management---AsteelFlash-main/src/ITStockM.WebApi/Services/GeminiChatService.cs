using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ITStockM.WebApi.Services
{
    /// <summary>
    /// Service to communicate with Google's Gemini API.
    /// Implements the IGeminiService interface for dependency injection and abstraction.
    /// </summary>
    public class GeminiChatService : IGeminiService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<GeminiChatService> _logger;
        private readonly string? _apiKey;
private const string Model = "gemini-1.5-flash";

        public GeminiChatService(HttpClient httpClient, IConfiguration configuration, ILogger<GeminiChatService> logger)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _logger = logger;

            // Read API key from environment variable or configuration
            // Priority: Environment variable > Configuration > Exception
_apiKey = configuration["GeminiApi:ApiKey"] ?? Environment.GetEnvironmentVariable("GEMINI_API_KEY") ?? Environment.GetEnvironmentVariable("GOOGLE_API_KEY");

            if (string.IsNullOrWhiteSpace(_apiKey))
            {
                _logger.LogWarning("Gemini API key not configured. Set GEMINI_API_KEY environment variable or GeminiApi:ApiKey in configuration.");
            }
        }

        /// <summary>
        /// Send a message to Gemini AI with optional conversation history for context.
        /// </summary>
        public async Task<string> SendMessageAsync(string userMessage, IEnumerable<ChatMessage>? conversationHistory = null)
        {
            if (string.IsNullOrWhiteSpace(_apiKey))
            {
                var errorMsg = "Gemini API key is not configured. Please set GEMINI_API_KEY environment variable.";
                _logger.LogError(errorMsg);
                throw new InvalidOperationException(errorMsg);
            }

            if (string.IsNullOrWhiteSpace(userMessage))
            {
                throw new ArgumentException("Message cannot be empty.", nameof(userMessage));
            }

            try
            {
                _logger.LogInformation("Sending message to Gemini API. Message preview: {MessagePreview}",
                    userMessage.Length > 100 ? userMessage.Substring(0, 100) + "..." : userMessage);

                var request = BuildRequest(userMessage, conversationHistory);
                var url = $"https://generativelanguage.googleapis.com/v1beta/models/{Model}:generateContent?key={_apiKey}";

                _logger.LogDebug("Calling Gemini API endpoint: {Endpoint}", url.Replace(_apiKey, "***"));

                var response = await _httpClient.PostAsJsonAsync(url, request);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Gemini API error: Status={StatusCode}, Response={Content}",
                        response.StatusCode, errorContent.Length > 200 ? errorContent.Substring(0, 200) : errorContent);

                    throw new HttpRequestException($"Gemini API request failed with status {response.StatusCode}: {response.ReasonPhrase}");
                }

                var jsonContent = await response.Content.ReadAsStringAsync();
                var result = System.Text.Json.JsonSerializer.Deserialize<GeminiResponse>(jsonContent);

                if (result?.Candidates?.FirstOrDefault()?.Content?.Parts?.FirstOrDefault()?.Text is string assistantMessage)
                {
                    _logger.LogInformation("Successfully received response from Gemini API");
                    return assistantMessage;
                }

                _logger.LogError("Unexpected response format from Gemini API. Response: {Response}",
                    jsonContent.Length > 200 ? jsonContent.Substring(0, 200) : jsonContent);
                throw new InvalidOperationException("Unexpected response format from Gemini API");
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP error calling Gemini API");
                throw;
            }
            catch (TaskCanceledException ex)
            {
                _logger.LogError(ex, "Request to Gemini API timed out");
                throw new HttpRequestException("Request to Gemini API timed out", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error calling Gemini API");
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

    /// <summary>
    /// Request model for Gemini API generateContent endpoint.
    /// </summary>
    public class GeminiRequest
    {
        [JsonPropertyName("contents")]
        public List<GeminiContent> Contents { get; set; } = new();
    }

    /// <summary>
    /// Content block containing role and message parts.
    /// </summary>
    public class GeminiContent
    {
        [JsonPropertyName("role")]
        public string Role { get; set; } = string.Empty;

        [JsonPropertyName("parts")]
        public GeminiPart[] Parts { get; set; } = Array.Empty<GeminiPart>();
    }

    /// <summary>
    /// Individual part of content (text message).
    /// </summary>
    public class GeminiPart
    {
        [JsonPropertyName("text")]
        public string Text { get; set; } = string.Empty;
    }

    /// <summary>
    /// Response from Gemini API.
    /// </summary>
    public class GeminiResponse
    {
        [JsonPropertyName("candidates")]
        public GeminiCandidate[]? Candidates { get; set; }
    }

    /// <summary>
    /// Candidate response object.
    /// </summary>
    public class GeminiCandidate
    {
        [JsonPropertyName("content")]
        public GeminiContent? Content { get; set; }
    }

    #endregion
}
