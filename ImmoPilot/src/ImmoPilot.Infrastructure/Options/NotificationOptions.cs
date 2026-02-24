namespace ImmoPilot.Infrastructure.Options;

public class NotificationOptions
{
    public const string SectionName = "Notification";
    public string TwilioAccountSid { get; set; } = string.Empty;
    public string TwilioAuthToken { get; set; } = string.Empty;
    public string FromWhatsApp { get; set; } = string.Empty;
    public string ToWhatsApp { get; set; } = string.Empty;
}
