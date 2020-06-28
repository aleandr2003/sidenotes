namespace SideNotes.Services.Templates
{
    public interface ITemplateLoader
    {
        EmailTemplate LoadEmailTemplate(string templateName, string culture = "ru");
    }
}