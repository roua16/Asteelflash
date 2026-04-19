namespace ITStockM.WebApi.Services
{
    /// <summary>
    /// Service contract for Gemini AI communication.
    /// </summary>
    public interface IGeminiService
    {
        /// <summary>
        /// Send a message to Gemini AI and receive a response.
        /// </summary>
        /// <param name="userMessage">The user's message to send to the model</param>
        /// <param name="conversationHistory">Optional conversation history for context</param>
        /// <returns>The AI response text</returns>
        Task<string> SendMessageAsync(string userMessage, IEnumerable<ChatMessage>? conversationHistory = null);
    }
}
