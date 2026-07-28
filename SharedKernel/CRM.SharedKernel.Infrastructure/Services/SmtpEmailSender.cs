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
		var cfg = options.Value;

		using var client = new SmtpClient(cfg.Host, cfg.Port)
		{
			Credentials = new NetworkCredential(cfg.Username, cfg.Password),
			EnableSsl = cfg.UseSsl,
		};

		using var message = new MailMessage
		{
			From = new MailAddress(cfg.FromAddress, cfg.FromName),
			Subject = subject,
			Body = htmlBody,
			IsBodyHtml = true,
		};

		message.To.Add(to);

		logger.LogInformation("Sending email to {To} with subject {Subject}", to, subject);
		await client.SendMailAsync(message, ct);
		logger.LogInformation("Email sent to {To}", to);
	}
}
