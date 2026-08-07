namespace Modules.Users.Features.Organization.CreateOrganization.Events;

public static class OrganizationTemplates
{
    public static string AgentAddedToOrganizationEmailContent(
    string agentName,
    string agentEmail,
    string tempPassword,
    string OrgName,
    string agentRole,
    string systemURL)
    => $"""
            <!DOCTYPE html>
            <html>
            <head>
                <meta charset="UTF-8" />
                <title>Account Created</title>
            </head>
            <body
                style="
                margin: 0;
                padding: 0;
                font-family: Arial, sans-serif;
                background-color: #f4f6f8;
                "
            >
                <table
                width="100%"
                cellpadding="0"
                cellspacing="0"
                style="background-color: #f4f6f8; padding: 20px"
                >
                <tr>
                    <td align="center">
                    <table
                        width="600"
                        cellpadding="0"
                        cellspacing="0"
                        style="background-color: #ffffff; border-radius: 6px; padding: 20px"
                    >
                        <!-- Header -->
                        <tr>
                        <td style="padding-bottom: 20px; text-align: left">
                            <h2 style="margin: 0; color: #333333">Welcome, {agentName}</h2>
                        </td>
                        </tr>
                        <!-- Body -->
                        <tr>
                        <td style="color: #555555; font-size: 14px; line-height: 1.6">
                            <p>
                            Your agent account has been successfully created in the
                            Ticketing System.
                            </p>
                            <p>You can now log in using the following credentials:</p>
                            <table cellpadding="0" cellspacing="0" style="margin: 15px 0">
                            <tr>
                                <td style="padding: 5px 0"><strong>Login URL:</strong></td>
                                <td style="padding: 5px 10px">
                                <a
                                    href="{systemURL}"
                                    style="color: #1a73e8; text-decoration: none"
                                    >{systemURL}</a
                                >
                                </td>
                            </tr>
                            <tr>
                                <td style="padding: 5px 0"><strong>Username:</strong></td>
                                <td style="padding: 5px 10px">{agentEmail}</td>
                            </tr>
                            <tr>
                                <td style="padding: 5px 0">
                                <strong>Temporary Password:</strong>
                                </td>
                                <td style="padding: 5px 10px">{tempPassword}</td>
                            </tr>
                            </table>
                            <p>You have been assigned to the following organization:</p>
                            <table cellpadding="0" cellspacing="0" style="margin: 15px 0">
                            <tr>
                                <td style="padding: 5px 0">
                                <strong>Organization Name:</strong>
                                </td>
                                <td style="padding: 5px 10px">{OrgName}</td>
                            </tr>
                            <tr>
                                <td style="padding: 5px 0"><strong>Role:</strong></td>
                                <td style="padding: 5px 10px">{agentRole}</td>
                            </tr>
                            </table>
                            <p>
                            Based on your role, you will be responsible for managing and
                            responding to tickets related to this organization.
                            </p>
                            <p
                            style="
                                margin-top: 20px;
                                padding: 10px;
                                background-color: #fff4e5;
                                border-left: 4px solid #ffa726;
                            "
                            >
                            For security reasons, please log in and change your password
                            as soon as possible.
                            </p>
                            <p>
                            If you have any questions or need assistance, please contact
                            the System Administrator.
                            </p>
                            <p style="margin-top: 30px">
                            Best regards,<br /><strong>System Administration Team</strong>
                            </p>
                        </td>
                        </tr>
                    </table>
                    </td>
                </tr>
                </table>
            </body>
            </html>
            """;

}
