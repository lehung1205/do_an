namespace JobPortal.API.Helpers;

public static class JobDescriptionPreview
{
    public const int MaxLength = 200;

    public static string Create(string? description)
    {
        if (string.IsNullOrWhiteSpace(description))
        {
            return string.Empty;
        }

        var text = description.Trim();
        if (text.Length <= MaxLength)
        {
            return text;
        }

        return text[..MaxLength].TrimEnd() + "…";
    }
}
