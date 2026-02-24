namespace ImmoPilot.Infrastructure.Options;

public class ApiOptions
{
    public const string SectionName = "Api";
    public string HudApiKey { get; set; } = string.Empty;
    public string RedfinBaseUrl { get; set; } = "https://www.redfin.com";
    public string HudBaseUrl { get; set; } = "https://www.huduser.gov";
}
