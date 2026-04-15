using System.Collections.Generic;
using System.Linq;

namespace ITStockM.WebApi.Services
{
    /// <summary>
    /// Scoped service to maintain chat conversation history across page navigations.
    /// </summary>
    public class GeminiChatStateService
    {
        private readonly List<ChatMessage> _messages = new();

        public IReadOnlyList<ChatMessage> Messages => _messages.AsReadOnly();

        public void AddMessage(ChatMessage message)
        {
            _messages.Add(message);
        }

        public void AddMessages(params ChatMessage[] messages)
        {
            _messages.AddRange(messages);
        }

        public void ClearHistory()
        {
            _messages.Clear();
        }

        public int MessageCount => _messages.Count;
    }

    public class ChatMessage
    {
        public ChatMessage(string sender, string content)
        {
            Sender = sender;
            Content = content;
            Timestamp = DateTime.UtcNow;
        }

        public string Sender { get; set; }
        public string Content { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
