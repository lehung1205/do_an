namespace JobPortal.Web.Models;

public class N8nChatbotOptions
{
    public const string SectionName = "N8nChatbot";

    public string WebhookUrl { get; set; } = "http://localhost:5678/workflow/XWYnx5BV7J7CLmOS";

    public int TimeoutSeconds { get; set; } = 120;

    public bool Enabled { get; set; } = true;
}
