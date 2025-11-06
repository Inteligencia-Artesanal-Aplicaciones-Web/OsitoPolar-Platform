using System.Text;
using OsitoPolarPlatform.API.Notifications.Domain.Model.ValueObjects;
using OsitoPolarPlatform.API.Notifications.Domain.Services;

namespace OsitoPolarPlatform.API.Notifications.Infrastructure.Services;

/// <summary>
/// Template renderer implementation using simple string replacement
/// Replaces {{placeholder}} with actual values
/// </summary>
public class TemplateRenderer : ITemplateRenderer
{
    private readonly string _templatesBasePath;
    private readonly ILogger<TemplateRenderer> _logger;

    public TemplateRenderer(IWebHostEnvironment environment, ILogger<TemplateRenderer> logger)
    {
        _templatesBasePath = Path.Combine(
            environment.ContentRootPath,
            "Notifications",
            "Infrastructure",
            "Templates"
        );
        _logger = logger;
    }

    public async Task<string> RenderTemplateAsync(EmailTemplate template, Dictionary<string, object> data)
    {
        try
        {
            // Get template file path
            var templateFileName = GetTemplateFileName(template);
            var templatePath = Path.Combine(_templatesBasePath, templateFileName);

            if (!File.Exists(templatePath))
            {
                _logger.LogError("Template file not found: {TemplatePath}", templatePath);
                throw new FileNotFoundException($"Template file not found: {templatePath}");
            }

            // Read template content
            var templateContent = await File.ReadAllTextAsync(templatePath);

            // Replace placeholders with actual values
            var renderedContent = RenderTemplate(templateContent, data);

            return renderedContent;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rendering template: {Template}", template);
            throw;
        }
    }

    private string RenderTemplate(string template, Dictionary<string, object> data)
    {
        var result = template;

        foreach (var kvp in data)
        {
            var placeholder = $"{{{{{kvp.Key}}}}}"; // {{key}}
            var value = kvp.Value?.ToString() ?? string.Empty;
            result = result.Replace(placeholder, value);
        }

        return result;
    }

    private static string GetTemplateFileName(EmailTemplate template)
    {
        return template switch
        {
            EmailTemplate.Welcome => "Welcome.html",
            EmailTemplate.PasswordReset => "PasswordReset.html",
            EmailTemplate.CredentialDelivery => "CredentialDelivery.html",
            EmailTemplate.TransactionConfirmation => "TransactionConfirmation.html",
            EmailTemplate.TwoFactorSetup => "TwoFactorSetup.html",
            _ => throw new ArgumentException($"Unknown template: {template}")
        };
    }
}
