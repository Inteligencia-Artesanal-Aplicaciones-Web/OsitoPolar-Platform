using Microsoft.AspNetCore.Mvc;
using OsitoPolarPlatform.API.Notifications.Application.Internal.CommandServices;
using OsitoPolarPlatform.API.Notifications.Domain.Model.Commands;
using OsitoPolarPlatform.API.Notifications.Domain.Model.ValueObjects;
using OsitoPolarPlatform.API.Notifications.Interfaces.REST.Resources;

namespace OsitoPolarPlatform.API.Notifications.Interfaces.REST.Controllers;

/// <summary>
/// Test controller for sending test emails
/// </summary>
[ApiController]
[Route("api/v1/notifications/test")]
public class NotificationsTestController : ControllerBase
{
    private readonly IEmailCommandService _emailCommandService;
    private readonly ILogger<NotificationsTestController> _logger;

    public NotificationsTestController(
        IEmailCommandService emailCommandService,
        ILogger<NotificationsTestController> logger)
    {
        _emailCommandService = emailCommandService;
        _logger = logger;
    }

    /// <summary>
    /// Send a test credential delivery email
    /// </summary>
    [HttpPost("credential-delivery")]
    public async Task<IActionResult> SendTestCredentialDeliveryEmail([FromBody] SendTestEmailResource resource)
    {
        try
        {
            _logger.LogInformation("Sending test credential delivery email to {Email}", resource.ToEmail);

            var command = new SendEmailCommand
            {
                To = resource.ToEmail,
                ToName = resource.ToName,
                Template = EmailTemplate.CredentialDelivery,
                TemplateData = new Dictionary<string, object>
                {
                    { "email", resource.ToEmail },
                    { "temporaryPassword", "TestPassword123!" },
                    { "loginUrl", "https://app.ositopolar.com/login" }
                }
            };

            var success = await _emailCommandService.SendTemplatedEmailAsync(command);

            if (success)
            {
                return Ok(new { message = "Test email sent successfully!", recipient = resource.ToEmail });
            }

            return BadRequest(new { message = "Failed to send test email" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending test email");
            return StatusCode(500, new { message = "Internal server error", error = ex.Message });
        }
    }

    /// <summary>
    /// Send a test welcome email
    /// </summary>
    [HttpPost("welcome")]
    public async Task<IActionResult> SendTestWelcomeEmail([FromBody] SendTestEmailResource resource)
    {
        try
        {
            _logger.LogInformation("Sending test welcome email to {Email}", resource.ToEmail);

            var command = new SendEmailCommand
            {
                To = resource.ToEmail,
                ToName = resource.ToName,
                Template = EmailTemplate.Welcome,
                TemplateData = new Dictionary<string, object>
                {
                    { "name", resource.ToName ?? "User" },
                    { "planName", "Polar Bear Plan" },
                    { "dashboardUrl", "https://app.ositopolar.com/dashboard" }
                }
            };

            var success = await _emailCommandService.SendTemplatedEmailAsync(command);

            if (success)
            {
                return Ok(new { message = "Test welcome email sent successfully!", recipient = resource.ToEmail });
            }

            return BadRequest(new { message = "Failed to send test email" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending test email");
            return StatusCode(500, new { message = "Internal server error", error = ex.Message });
        }
    }

    /// <summary>
    /// Send a test password reset email
    /// </summary>
    [HttpPost("password-reset")]
    public async Task<IActionResult> SendTestPasswordResetEmail([FromBody] SendTestEmailResource resource)
    {
        try
        {
            _logger.LogInformation("Sending test password reset email to {Email}", resource.ToEmail);

            var command = new SendEmailCommand
            {
                To = resource.ToEmail,
                ToName = resource.ToName,
                Template = EmailTemplate.PasswordReset,
                TemplateData = new Dictionary<string, object>
                {
                    { "resetUrl", "https://app.ositopolar.com/reset-password?token=test-token-123" }
                }
            };

            var success = await _emailCommandService.SendTemplatedEmailAsync(command);

            if (success)
            {
                return Ok(new { message = "Test password reset email sent successfully!", recipient = resource.ToEmail });
            }

            return BadRequest(new { message = "Failed to send test email" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending test email");
            return StatusCode(500, new { message = "Internal server error", error = ex.Message });
        }
    }

    /// <summary>
    /// Send a test transaction confirmation email
    /// </summary>
    [HttpPost("transaction-confirmation")]
    public async Task<IActionResult> SendTestTransactionConfirmationEmail([FromBody] SendTestEmailResource resource)
    {
        try
        {
            _logger.LogInformation("Sending test transaction confirmation email to {Email}", resource.ToEmail);

            var command = new SendEmailCommand
            {
                To = resource.ToEmail,
                ToName = resource.ToName,
                Template = EmailTemplate.TransactionConfirmation,
                TemplateData = new Dictionary<string, object>
                {
                    { "transactionId", "TXN-123456789" },
                    { "amount", "18.99" },
                    { "currency", "USD" },
                    { "planName", "Polar Bear Plan" },
                    { "date", DateTime.UtcNow.ToString("MMMM dd, yyyy") }
                }
            };

            var success = await _emailCommandService.SendTemplatedEmailAsync(command);

            if (success)
            {
                return Ok(new { message = "Test transaction confirmation email sent successfully!", recipient = resource.ToEmail });
            }

            return BadRequest(new { message = "Failed to send test email" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending test email");
            return StatusCode(500, new { message = "Internal server error", error = ex.Message });
        }
    }

    /// <summary>
    /// Send a raw HTML email for testing
    /// </summary>
    [HttpPost("raw")]
    public async Task<IActionResult> SendTestRawEmail([FromBody] SendTestEmailResource resource)
    {
        try
        {
            _logger.LogInformation("Sending test raw email to {Email}", resource.ToEmail);

            var htmlBody = @"
                <!DOCTYPE html>
                <html>
                <head>
                    <style>
                        body { font-family: Arial, sans-serif; padding: 20px; }
                        .container { max-width: 600px; margin: 0 auto; background: #f5f5f5; padding: 30px; border-radius: 8px; }
                        h1 { color: #4A90E2; }
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <h1>🐻‍❄️ Test Email from OsitoPolar</h1>
                        <p>This is a test email sent from the Notifications system.</p>
                        <p><strong>Sent to:</strong> " + resource.ToEmail + @"</p>
                        <p><strong>Time:</strong> " + DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss UTC") + @"</p>
                    </div>
                </body>
                </html>
            ";

            var success = await _emailCommandService.SendRawEmailAsync(
                resource.ToEmail,
                resource.ToName,
                "Test Email from OsitoPolar 🐻‍❄️",
                htmlBody
            );

            if (success)
            {
                return Ok(new { message = "Test raw email sent successfully!", recipient = resource.ToEmail });
            }

            return BadRequest(new { message = "Failed to send test email" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending test email");
            return StatusCode(500, new { message = "Internal server error", error = ex.Message });
        }
    }
}
