using System.Net;
using System.Net.Mail;
using CRM.SharedKernel.Infrastructure.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CRM.SharedKernel.Infrastructure.Services;

internal sealed class SmtpEmailSender(
	IOptions<EmailOptions> options,
	ILogger<SmtpEmailSender> logger) : IEmailSender
{
	public async Task SendAsync(string to, string subject, string htmlBody, CancellationToken ct = default)
	{
		if (string.IsNullOrWhiteSpace(to))
		{
			logger.LogWarning("Skipping email: empty recipient for subject {Subject}", subject);
			return;
		}

		if (!System.Net.Mail.MailAddress.TryCreate(to, out _))
		{
			logger.LogWarning("Skipping email: invalid To address {To} for subject {Subject}", to, subject);
			return;
		}

		var cfg = options.Value;

		if (string.IsNullOrWhiteSpace(cfg.Host))
		{
			logger.LogInformation("Skipping email to {To}: no SMTP host configured (MailServer not set)", to);
			return;
		}

		var fromAddress = cfg.FromAddress;
		if (string.IsNullOrWhiteSpace(fromAddress))
		{
			if (!string.IsNullOrWhiteSpace(cfg.Username) && cfg.Username.Contains('@') && System.Net.Mail.MailAddress.TryCreate(cfg.Username, out _))
				fromAddress = cfg.Username;
			else
				fromAddress = "noreply@crm.local";
			logger.LogDebug("MailServer:FromAddress not configured, using fallback {From}", fromAddress);
		}

		if (!System.Net.Mail.MailAddress.TryCreate(fromAddress, out _))
		{
			logger.LogWarning("Skipping email to {To}: invalid From address {From}", to, fromAddress);
			return;
		}

		var fromName = string.IsNullOrWhiteSpace(cfg.FromName) ? fromAddress : cfg.FromName;

		using var client = new SmtpClient(cfg.Host, cfg.Port)
		{
			Credentials = string.IsNullOrWhiteSpace(cfg.Username) ? null : new NetworkCredential(cfg.Username, cfg.Password),
			EnableSsl = cfg.UseSsl,
		};

		using var message = new MailMessage
		{
			From = new MailAddress(fromAddress, fromName),
			Subject = subject,
			Body = htmlBody,
			IsBodyHtml = true,
		};

		message.To.Add(to);

		logger.LogInformation("Sending email to {To} with subject {Subject}", to, subject);
		try
		{
			await client.SendMailAsync(message, ct);
			logger.LogInformation("Email sent to {To}", to);
		}
		catch (Exception ex) when (ex is SmtpException or InvalidOperationException)
		{
			logger.LogWarning(ex, "Failed to send email to {To} with subject {Subject}", to, subject);
		}
	}
}
