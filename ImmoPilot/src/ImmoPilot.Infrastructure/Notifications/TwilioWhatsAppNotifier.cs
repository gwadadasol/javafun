using ImmoPilot.Application.Interfaces;
using ImmoPilot.Domain.Common;
using ImmoPilot.Domain.Entities;
using ImmoPilot.Infrastructure.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace ImmoPilot.Infrastructure.Notifications;

public class TwilioWhatsAppNotifier(
    IOptions<NotificationOptions> notificationOptions,
    ILogger<TwilioWhatsAppNotifier> logger) : IWhatsAppNotifier
{
    private readonly NotificationOptions _options = notificationOptions.Value;

    public async Task<Result<bool>> SendDigestAsync(
        IEnumerable<Property> properties, string runId, CancellationToken cancellationToken = default)
    {
        try
        {
            TwilioClient.Init(_options.TwilioAccountSid, _options.TwilioAuthToken);

            var body = BuildDigestMessage(properties, runId);

            var message = await MessageResource.CreateAsync(
                body: body,
                from: new PhoneNumber($"whatsapp:{_options.FromWhatsApp}"),
                to: new PhoneNumber($"whatsapp:{_options.ToWhatsApp}"));

            logger.LogInformation("WhatsApp digest sent: {Sid}", message.Sid);
            return Result<bool>.Ok(true);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to send WhatsApp digest for run {RunId}", runId);
            return Result<bool>.Fail($"TWILIO_ERROR: {ex.Message}");
        }
    }

    private static string BuildDigestMessage(IEnumerable<Property> properties, string runId)
    {
        var props = properties.ToList();
        var lines = new List<string>
        {
            $"*ImmoPilot Digest* — {runId}",
            $"_{props.Count} propert{(props.Count == 1 ? "y" : "ies")} qualified today_",
            ""
        };

        foreach (var p in props)
        {
            lines.Add($"*{p.Address}, {p.City}*");
            lines.Add($"  Price: ${p.Price:N0}  |  DSCR: {p.DscrRatio:F2}  |  {p.DscrStatus}");
            lines.Add($"  FMR: ${p.FmrRent:N0}/mo  |  MLS: {p.MlsId}");
            lines.Add("");
        }

        return string.Join("\n", lines);
    }
}
