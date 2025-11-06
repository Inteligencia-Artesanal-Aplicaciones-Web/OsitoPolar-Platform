using OsitoPolarPlatform.API.Notifications.Domain.Model.ValueObjects;

namespace OsitoPolarPlatform.API.Notifications.Domain.Services;

/// <summary>
/// Service for rendering email templates with dynamic data
/// </summary>
public interface ITemplateRenderer
{
    /// <summary>
    /// Render a template with the provided data
    /// </summary>
    /// <param name="template">Template type to render</param>
    /// <param name="data">Dictionary of placeholder keys and values</param>
    /// <returns>Rendered HTML content</returns>
    Task<string> RenderTemplateAsync(EmailTemplate template, Dictionary<string, object> data);
}
