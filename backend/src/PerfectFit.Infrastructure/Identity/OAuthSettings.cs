namespace PerfectFit.Infrastructure.Identity;

/// <summary>
/// OAuth provider configuration settings.
/// </summary>
public class OAuthSettings
{
    public const string SectionName = "OAuth";

    public MicrosoftSettings Microsoft { get; set; } = new();
}

public class MicrosoftSettings
{
    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
}
