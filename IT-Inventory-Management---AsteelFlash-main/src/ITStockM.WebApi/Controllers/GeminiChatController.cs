using ITStockM.WebApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json.Serialization;

namespace ITStockM.WebApi.Controllers
{
    /// <summary>
    /// Controller for Gemini AI chat functionality.
    /// Handles message sending and chat history management.
    /// </summary>
    [AllowAnonymous]
    [ApiController]
    [Route("api/[controller]")]
    public class GeminiChatController : ControllerBase
    {
        private readonly IGeminiService _geminiService;
        private readonly GeminiChatStateService _chatStateService;
        private readonly ILogger<GeminiChatController> _logger;

        public GeminiChatController(
            IGeminiService geminiService,
            GeminiChatStateService chatStateService,
            ILogger<GeminiChatController> logger)
        {
            _geminiService = geminiService ?? throw new ArgumentNullException(nameof(geminiService));
            _chatStateService = chatStateService ?? throw new ArgumentNullException(nameof(chatStateService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Send a message to Gemini AI and get a response.
        /// </summary>
        /// <param name="request">The chat request containing the user message</param>
        /// <returns>The AI response with message count</returns>
        /// <response code="200">Successfully received response from Gemini</response>
        /// <response code="400">Invalid request (empty message)</response>
        /// <response code="500">Server error (API not configured or request failed)</response>
        /// <response code="503">Gemini API service unavailable</response>
        [HttpPost("send")]
        [ProducesResponseType(typeof(ChatResponseModel), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        [ProducesResponseType(503)]
        public async Task<IActionResult> SendMessage([FromBody] ChatRequestModel request)
        {
            if (string.IsNullOrWhiteSpace(request?.Message))
            {
                _logger.LogWarning("Attempt to send empty message to Gemini");
                return BadRequest(new { error = "Message cannot be empty." });
            }

            try
            {
                _logger.LogInformation("User sending message to Gemini (message length: {MessageLength})",
                    request.Message.Length);

                // Add user message to state
                _chatStateService.AddMessage(new ChatMessage("user", request.Message));

                // Get response from Gemini service
                var response = await _geminiService.SendMessageAsync(request.Message, _chatStateService.Messages);

                // Validate response
                if (string.IsNullOrWhiteSpace(response))
                {
                    _logger.LogWarning("Gemini returned empty response");
                    return StatusCode(500, new { error = "Received empty response from Gemini API." });
                }

                // Add assistant message to state
                _chatStateService.AddMessage(new ChatMessage("assistant", response));

                _logger.LogInformation("Successfully processed Gemini response. Total messages: {MessageCount}",
                    _chatStateService.MessageCount);

                return Ok(new ChatResponseModel
                {
                    Success = true,
                    Message = response,
                    MessageCount = _chatStateService.MessageCount
                });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex, "Configuration error in Gemini chat");
                return StatusCode(500, new
                {
                    error = "Service is not properly configured.",
                    details = ex.Message
                });
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP error calling Gemini API");
                return StatusCode(503, new
                {
                    error = "Failed to reach Gemini API. Please try again later.",
                    details = ex.Message
                });
            }
            catch (TaskCanceledException ex)
            {
                _logger.LogError(ex, "Request to Gemini API timed out");
                return StatusCode(504, new
                {
                    error = "Request to Gemini API timed out. Please try again.",
                    details = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in Gemini chat");
                return StatusCode(500, new
                {
                    error = "An unexpected error occurred. Please try again.",
                    details = ex.Message
                });
            }
        }

        /// <summary>
        /// Clear chat history.
        /// </summary>
        /// <response code="200">Chat history cleared successfully</response>
        [HttpPost("clear")]
        [ProducesResponseType(200)]
        [ProducesResponseType(500)]
        public IActionResult ClearHistory()
        {
            try
            {
                _chatStateService.ClearHistory();
                _logger.LogInformation("Chat history cleared by user");
                return Ok(new { success = true, message = "Chat history cleared." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error clearing chat history");
                return StatusCode(500, new { error = "Failed to clear chat history." });
            }
        }

        /// <summary>
        /// Get current message count and status.
        /// </summary>
        /// <response code="200">Returns current message count</response>
        [HttpGet("status")]
        [ProducesResponseType(typeof(ChatStatusModel), 200)]
        public IActionResult GetStatus()
        {
            return Ok(new ChatStatusModel
            {
                MessageCount = _chatStateService.MessageCount,
                IsConfigured = true
            });
        }
    }

    /// <summary>
    /// Request model for sending message to Gemini.
    /// </summary>
    public class ChatRequestModel
    {
        [JsonPropertyName("message")]
        public string? Message { get; set; }
    }

    /// <summary>
    /// Response model from Gemini chat endpoint.
    /// </summary>
    public class ChatResponseModel
    {
        [JsonPropertyName("success")]
        public bool Success { get; set; }

        [JsonPropertyName("message")]
        public string? Message { get; set; }

        [JsonPropertyName("messageCount")]
        public int MessageCount { get; set; }

        [JsonPropertyName("error")]
        public string? Error { get; set; }
    }

    /// <summary>
    /// Status response for chat service.
    /// </summary>
    public class ChatStatusModel
    {
        [JsonPropertyName("messageCount")]
        public int MessageCount { get; set; }

        [JsonPropertyName("isConfigured")]
        public bool IsConfigured { get; set; }
    }
}
