namespace Modules.Users.Features.Organizations.Events;

internal static class TicketNotificationTemplates
{
	public static string NewTicketEmail(
		string agentName,
		string organizationName,
		int ticketId,
		string ticketTitle,
		string ticketDescription,
		string agentRole,
		string systemUrl) => $"""
		<!DOCTYPE html>
		<html>
		<head><meta charset="UTF-8" /><title>New Ticket</title></head>
		<body style="margin:0;padding:0;font-family:Arial,sans-serif;background-color:#f4f6f8">
		<table width="100%" cellpadding="0" cellspacing="0" style="background-color:#f4f6f8;padding:20px">
		<tr><td align="center">
		<table width="600" cellpadding="0" cellspacing="0" style="background-color:#ffffff;border-radius:6px;padding:20px">
		<tr><td style="padding-bottom:20px"><h2 style="margin:0;color:#333">Hi {agentName},</h2></td></tr>
		<tr><td style="color:#555;font-size:14px;line-height:1.6">
		<p>A new ticket has been created in your organization <strong>{organizationName}</strong> and requires attention.</p>
		<table cellpadding="0" cellspacing="0" style="margin:15px 0;width:100%">
		<tr><td style="padding:6px 0"><strong>Ticket #:</strong></td><td style="padding:6px 10px">{ticketId}</td></tr>
		<tr><td style="padding:6px 0"><strong>Title:</strong></td><td style="padding:6px 10px">{ticketTitle}</td></tr>
		<tr><td style="padding:6px 0"><strong>Description:</strong></td><td style="padding:6px 10px">{ticketDescription}</td></tr>
		<tr><td style="padding:6px 0"><strong>Your role:</strong></td><td style="padding:6px 10px">{agentRole}</td></tr>
		</table>
		<p>Please open the ticketing portal to review and assign this ticket.</p>
		<p><a href="{systemUrl}" style="display:inline-block;padding:10px 18px;background-color:#1a73e8;color:#ffffff;text-decoration:none;border-radius:4px">Open portal</a></p>
		<p style="margin-top:20px;color:#888;font-size:12px">This is an automated notification — please do not reply.</p>
		<p style="margin-top:10px">Best regards,<br/><strong>CRM Ticketing</strong></p>
		</td></tr>
		</table>
		</td></tr>
		</table>
		</body>
		</html>
		""";
}
