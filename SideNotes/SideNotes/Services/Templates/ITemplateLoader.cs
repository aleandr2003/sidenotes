namespace SideNotes.Services.Templates
{
    public interface ITemplateLoader
    {
        EmailTemplate GetEmailTemplate(string templateName, string culture = "ru");
    }
}