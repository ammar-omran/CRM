namespace Modules.Customers.Features.Templates;

/// <summary>
/// Provides HTML email templates used in customer-related notifications.
/// Follows the same table-based, inline-styled pattern as <c>OrganizationTemplates</c>
/// in the UserManagement service to ensure consistent email presentation.
/// </summary>
public static class CustomerEmailTemplates
{
    /// <summary>
    /// Builds the HTML body for the login announcement email sent to a customer
    /// immediately after each successful login.
    /// </summary>
    /// <param name="customerName">The full name of the customer who logged in.</param>
    /// <param name="loginDateTime">The UTC date and time of the login event.</param>
    /// <param name="deviceInfo">
    /// The User-Agent string from the HTTP request, representing the device / browser
    /// used to log in. Displayed as-is in the email body.
    /// </param>
    /// <param name="supportEmail">Support contact email shown at the bottom of the email.</param>
    /// <param name="companyName">The company name shown in the email footer.</param>
    /// <returns>A fully-rendered, inline-styled HTML string ready to send via SMTP.</returns>
    public static string LoginNotificationHtmlBody(
        string customerName,
        DateTime loginDateTime,
        string deviceInfo,
        string supportEmail = "codehunters2024@gmail.com",
        string companyName = "Azka CRM")
        => $"""
            <!DOCTYPE html>
            <html lang="en">
            <head>
                <meta charset="UTF-8" />
                <meta name="viewport" content="width=device-width, initial-scale=1.0" />
                <title>New Login to Your Account</title>
            </head>
            <body style="margin:0;padding:0;font-family:Arial,sans-serif;background-color:#f0f4f8;">

                <table width="100%" cellpadding="0" cellspacing="0"
                       style="background-color:#f0f4f8;padding:30px 0;">
                    <tr>
                        <td align="center">

                            <!-- Outer card -->
                            <table width="600" cellpadding="0" cellspacing="0"
                                   style="background-color:#ffffff;border-radius:8px;
                                          box-shadow:0 2px 8px rgba(0,0,0,0.08);
                                          overflow:hidden;">

                                <!-- ── Header banner ── -->
                                <tr>
                                    <td style="background-color:#1a73e8;padding:28px 32px;">
                                        <h1 style="margin:0;color:#ffffff;font-size:22px;
                                                   font-weight:700;letter-spacing:0.3px;">
                                            🔐 New Login Detected
                                        </h1>
                                        <p style="margin:6px 0 0;color:#cce0ff;font-size:13px;">
                                            A sign-in to your {companyName} account was just recorded.
                                        </p>
                                    </td>
                                </tr>

                                <!-- ── Greeting ── -->
                                <tr>
                                    <td style="padding:28px 32px 0;">
                                        <p style="margin:0;color:#333333;font-size:15px;line-height:1.6;">
                                            Dear <strong>{customerName}</strong>,
                                        </p>
                                        <p style="margin:12px 0 0;color:#555555;font-size:14px;line-height:1.7;">
                                            We detected a new login to your account. Here are the details:
                                        </p>
                                    </td>
                                </tr>

                                <!-- ── Login Details card ── -->
                                <tr>
                                    <td style="padding:20px 32px;">
                                        <table width="100%" cellpadding="0" cellspacing="0"
                                               style="background-color:#f7faff;border:1px solid #d0e4ff;
                                                      border-radius:6px;padding:0;">
                                            <tr>
                                                <td style="padding:16px 20px;
                                                           border-bottom:1px solid #e0ecff;">
                                                    <table width="100%" cellpadding="0" cellspacing="0">
                                                        <tr>
                                                            <td style="color:#888888;font-size:12px;
                                                                       text-transform:uppercase;
                                                                       letter-spacing:0.8px;
                                                                       width:40%;">
                                                                Date &amp; Time
                                                            </td>
                                                            <td style="color:#222222;font-size:14px;
                                                                       font-weight:600;">
                                                                {loginDateTime:dddd, MMMM dd, yyyy} at {loginDateTime:hh:mm tt} UTC
                                                            </td>
                                                        </tr>
                                                    </table>
                                                </td>
                                            </tr>
                                            <tr>
                                                <td style="padding:16px 20px;">
                                                    <table width="100%" cellpadding="0" cellspacing="0">
                                                        <tr>
                                                            <td style="color:#888888;font-size:12px;
                                                                       text-transform:uppercase;
                                                                       letter-spacing:0.8px;
                                                                       width:40%;
                                                                       vertical-align:top;">
                                                                Device / Browser
                                                            </td>
                                                            <td style="color:#222222;font-size:13px;
                                                                       word-break:break-word;">
                                                                {deviceInfo}
                                                            </td>
                                                        </tr>
                                                    </table>
                                                </td>
                                            </tr>
                                        </table>
                                    </td>
                                </tr>

                                <!-- ── "Was this you?" section ── -->
                                <tr>
                                    <td style="padding:4px 32px 24px;">

                                        <!-- Was this you? -->
                                        <table width="100%" cellpadding="0" cellspacing="0"
                                               style="background-color:#e8f5e9;border-left:4px solid #43a047;
                                                      border-radius:4px;margin-bottom:14px;">
                                            <tr>
                                                <td style="padding:14px 18px;color:#2e7d32;font-size:14px;
                                                           line-height:1.6;">
                                                    ✅ <strong>If this was you</strong>, no further action is required.
                                                    You can safely ignore this email.
                                                </td>
                                            </tr>
                                        </table>

                                        <!-- Was NOT you? -->
                                        <table width="100%" cellpadding="0" cellspacing="0"
                                               style="background-color:#fff8e1;border-left:4px solid #ffa726;
                                                      border-radius:4px;">
                                            <tr>
                                                <td style="padding:14px 18px;color:#e65100;font-size:14px;
                                                           line-height:1.6;">
                                                    ⚠️ <strong>If you do not recognize this activity</strong>,
                                                    we strongly recommend that you
                                                    <strong>reset your password immediately</strong>
                                                    and contact our support team.
                                                </td>
                                            </tr>
                                        </table>
                                    </td>
                                </tr>

                                <!-- ── Divider ── -->
                                <tr>
                                    <td style="padding:0 32px;">
                                        <hr style="border:none;border-top:1px solid #eeeeee;margin:0;" />
                                    </td>
                                </tr>

                                <!-- ── Footer ── -->
                                <tr>
                                    <td style="padding:22px 32px;background-color:#fafafa;
                                               border-radius:0 0 8px 8px;">
                                        <p style="margin:0;color:#777777;font-size:13px;line-height:1.7;">
                                            Your account security is important to us.<br />
                                            Best regards,<br />
                                            <strong style="color:#333333;">{companyName}</strong><br />
                                            Support:
                                            <a href="mailto:{supportEmail}"
                                               style="color:#1a73e8;text-decoration:none;">
                                                {supportEmail}
                                            </a>
                                        </p>
                                    </td>
                                </tr>

                            </table>
                            <!-- /Outer card -->

                        </td>
                    </tr>
                </table>

            </body>
            </html>
            """;

    /// <summary>
    /// Returns the standard subject line for a login notification email.
    /// </summary>
    public static string LoginNotificationSubject => "New Login to Your Account";
}
