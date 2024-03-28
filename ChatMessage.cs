namespace IKSAssistApp;

internal class ChatMessage
{
    // Text entered by user
    internal string Text { get; set; }

    // Prompt sent to the LLM
    internal string Prompt { get; set; }
}
