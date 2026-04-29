using Microsoft.AspNetCore.Mvc;

namespace BackEndAPI.TemplateHtml
{
    public static class TemplateHelper
    {
        public static string GetEmailTemplate(string templatePath, Dictionary<string, string> placeholders)
        {
            var template = File.ReadAllText(templatePath);
            foreach (var placeholder in placeholders)
            {
                template = template.Replace(placeholder.Key, placeholder.Value);
            }
            return template;
        }
    }
}
