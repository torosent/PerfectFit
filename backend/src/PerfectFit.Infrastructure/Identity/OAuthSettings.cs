namespace PerfectFit.Infrastructure.Identity;

/// <summary>
/// OAuth provider configuration settings.
/// </summary>
public class OAuthSettings
{
    public const string SectionName = "OAuth";

    public GoogleSettings Google { get; set; } = new();
    public MicrosoftSettings Microsoft { get; set; } = new();
    public AppleSettings Apple { get; set; } = new();
}

public class GoogleSettings
{
    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
}

public class MicrosoftSettings
{
    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
}

public class AppleSettings
{
    public string ClientId { get; set; } = string.Empty;
    public string TeamId { get; set; } = string.Empty;
    public string KeyId { get; set; } = string.Empty;
    public string PrivateKey { get; set; } = string.Empty;
}
