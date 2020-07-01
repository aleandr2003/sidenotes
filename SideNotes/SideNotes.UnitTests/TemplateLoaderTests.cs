using Microsoft.VisualStudio.TestTools.UnitTesting;
using SideNotes.Services.Templates;

namespace SideNotes.UnitTests
{
    [TestClass]
    public class TemplateLoaderTests
    {
        [TestMethod]
        public void ShouldLoadCorrectTemplate()
        {
            var t = new TemplateLoader(".\\TestTemplates");

            var template = t.GetEmailTemplate("correct", "ru");

            Assert.IsNotNull(template);
            Assert.IsNotNull(template.Body);
            Assert.IsNotNull(template.Subject);
        }

        [TestMethod]
        public void ShouldNotLoadFromIncorrectPath()
        {
            var t = new TemplateLoader(".\\TestTemplates\\MissingFolder");

            var template = t.GetEmailTemplate("correct", "ru");

            Assert.IsNull(template);
        }

        [TestMethod]
        public void ShouldNotLoadMissingTemplate()
        {
            var t = new TemplateLoader(".\\TestTemplates");

            var template = t.GetEmailTemplate("missingtemplate", "ru");

            Assert.IsNull(template);
        }

        [TestMethod]
        public void ShouldLoadSelectedCulture()
        {
            var t = new TemplateLoader(".\\TestTemplates");

            var template = t.GetEmailTemplate("correct", "en");

            Assert.IsNotNull(template);
            Assert.IsNotNull(template.Body);
            Assert.IsNotNull(template.Subject);
        }

        [TestMethod]
        public void ShouldNotLoadBrokenTemplate()
        {
            var t = new TemplateLoader(".\\TestTemplates");

            var template = t.GetEmailTemplate("broken", "ru");

            Assert.IsNull(template);
        }

        [TestMethod]
        public void ShouldLoadTemplateWithiMissingField()
        {
            var t = new TemplateLoader(".\\TestTemplates");

            var template = t.GetEmailTemplate("missingsubject", "ru");

            Assert.IsNotNull(template);
            Assert.IsNotNull(template.Body);
            Assert.IsNull(template.Subject);
        }
    }
}
