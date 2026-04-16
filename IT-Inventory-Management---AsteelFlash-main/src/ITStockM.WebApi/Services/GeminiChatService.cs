using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ITStockM.WebApi.Services
{
    public class GeminiChatService : IGeminiService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<GeminiChatService> _logger;
        private readonly string _apiKey;

        // Safer model name
        private const string Model = "models/gemini-2.5-flash-lite";

        public GeminiChatService(
            HttpClient httpClient,
            IConfiguration configuration,
            ILogger<GeminiChatService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;

            // Read API key from appsettings or environment variable
            _apiKey =
                configuration["GeminiApi:ApiKey"] ??
                Environment.GetEnvironmentVariable("GEMINI_API_KEY") ??
                throw new InvalidOperationException(
                    "Gemini API key not found. Configure GeminiApi:ApiKey or GEMINI_API_KEY.");
        }

        public async Task<string> SendMessageAsync(
            string userMessage,
            IEnumerable<ChatMessage>? conversationHistory = null)
        {
            if (string.IsNullOrWhiteSpace(userMessage))
                throw new ArgumentException("Message cannot be empty.");

            var request = BuildRequest(userMessage, conversationHistory);

            var url = $"https://generativelanguage.googleapis.com/v1beta/{Model}:generateContent?key={_apiKey}";
            try
            {
                var response = await _httpClient.PostAsJsonAsync(url, request);

                var responseContent = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError(
                        "Gemini API failed. Status: {Status}. Body: {Body}",
                        (int)response.StatusCode,
                        responseContent);

                    throw new Exception(
                        $"Gemini API error {(int)response.StatusCode}: {responseContent}");
                }

                var result = JsonSerializer.Deserialize<GeminiResponse>(
                    responseContent,
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                var text = result?
                    .Candidates?
                    .FirstOrDefault()?
                    .Content?
                    .Parts?
                    .FirstOrDefault()?
                    .Text;

                if (string.IsNullOrWhiteSpace(text))
                {
                    _logger.LogError("Gemini returned empty response: {Body}", responseContent);
                    throw new Exception("Gemini returned an empty response.");
                }

                return text;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while calling Gemini API");
                throw;
            }
        }

        private static GeminiRequest BuildRequest(
            string userMessage,
            IEnumerable<ChatMessage>? conversationHistory)
        {
            var contents = new List<GeminiContent>();

            if (conversationHistory != null)
            {
                foreach (var message in conversationHistory)
                {
                    if (string.IsNullOrWhiteSpace(message.Content))
                        continue;

                    contents.Add(new GeminiContent
                    {
                        Role = message.Sender.Equals("user", StringComparison.OrdinalIgnoreCase)
                            ? "user"
                            : "model",
                        Parts = new List<GeminiPart>
                        {
                            new GeminiPart
                            {
                                Text = message.Content
                            }
                        }
                    });
                }
            }

            contents.Add(new GeminiContent
            {
                Role = "user",
                Parts = new List<GeminiPart>
                {
                    new GeminiPart
                    {
                        Text = userMessage
                    }
                }
            });

            return new GeminiRequest
            {
                Contents = contents
            };
        }
    }

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
        public List<GeminiPart> Parts { get; set; } = new();
    }

    public class GeminiPart
    {
        [JsonPropertyName("text")]
        public string Text { get; set; } = string.Empty;
    }

    public class GeminiResponse
    {
        [JsonPropertyName("candidates")]
        public List<GeminiCandidate>? Candidates { get; set; }
    }

    public class GeminiCandidate
    {
        [JsonPropertyName("content")]
        public GeminiContent? Content { get; set; }
    }
}