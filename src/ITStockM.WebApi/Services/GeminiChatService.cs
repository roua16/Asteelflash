using System.Net;
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
        private readonly string? _apiKey;
        private readonly string _model;
        private readonly string _baseUrl;

        public GeminiChatService(
            HttpClient httpClient,
            IConfiguration configuration,
            ILogger<GeminiChatService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;

            // Environment variable must override appsettings for safe key rotation.
            _apiKey = FirstNonEmpty(
                Environment.GetEnvironmentVariable("GEMINI_API_KEY"),
                configuration["GeminiApi:ApiKey"]);

            _model = FirstNonEmpty(configuration["GeminiApi:Model"], "gemini-1.5-flash")!;
            _baseUrl = FirstNonEmpty(
                configuration["GeminiApi:BaseUrl"],
                "https://generativelanguage.googleapis.com/v1beta/models")!;
        }

        public async Task<string> SendMessageAsync(
            string userMessage,
            IEnumerable<ChatMessage>? conversationHistory = null)
        {
            if (string.IsNullOrWhiteSpace(userMessage))
                throw new ArgumentException("Message cannot be empty.");

            var request = BuildRequest(userMessage, conversationHistory);
            var apiKey = GetApiKeyOrThrow();
            var modelPath = _model.StartsWith("models/", StringComparison.OrdinalIgnoreCase)
                ? _model["models/".Length..]
                : _model;

            var url = $"{_baseUrl.TrimEnd('/')}/{modelPath}:generateContent?key={Uri.EscapeDataString(apiKey)}";
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

                    throw new GeminiApiException(
                        response.StatusCode,
                        $"Gemini API error {(int)response.StatusCode}: {ExtractApiError(responseContent)}");
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

        private string GetApiKeyOrThrow()
        {
            if (!string.IsNullOrWhiteSpace(_apiKey))
            {
                return _apiKey;
            }

            throw new InvalidOperationException(
                "Gemini API key not configured. Set GEMINI_API_KEY or GeminiApi:ApiKey.");
        }

        private static string? FirstNonEmpty(params string?[] values)
        {
            return values.FirstOrDefault(value => !string.IsNullOrWhiteSpace(value))?.Trim();
        }

        private static string ExtractApiError(string responseContent)
        {
            if (string.IsNullOrWhiteSpace(responseContent))
            {
                return "No error payload received.";
            }

            try
            {
                using var doc = JsonDocument.Parse(responseContent);
                var root = doc.RootElement;
                if (root.TryGetProperty("error", out var errorNode))
                {
                    if (errorNode.ValueKind == JsonValueKind.Object &&
                        errorNode.TryGetProperty("message", out var messageNode) &&
                        messageNode.GetString() is { Length: > 0 } message)
                    {
                        return message;
                    }

                    if (errorNode.ValueKind == JsonValueKind.String &&
                        errorNode.GetString() is { Length: > 0 } errorText)
                    {
                        return errorText;
                    }
                }

                if (root.TryGetProperty("message", out var topMessage) &&
                    topMessage.GetString() is { Length: > 0 } fallbackMessage)
                {
                    return fallbackMessage;
                }
            }
            catch
            {
                // Ignore parser errors and return raw content.
            }

            return responseContent;
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

    public sealed class GeminiApiException : Exception
    {
        public GeminiApiException(HttpStatusCode statusCode, string message)
            : base(message)
        {
            StatusCode = statusCode;
        }

        public HttpStatusCode StatusCode { get; }
    }
}
