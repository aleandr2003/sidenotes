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

            var template = t.LoadEmailTemplate("correct", "ru");

            Assert.IsNotNull(template);
            Assert.IsNotNull(template.Body);
            Assert.IsNotNull(template.Subject);
        }

        [TestMethod]
        public void ShouldNotLoadFromIncorrectPath()
        {
            var t = new TemplateLoader(".\\TestTemplates\\MissingFolder");

            var template = t.LoadEmailTemplate("correct", "ru");

            Assert.IsNull(template);
        }

        [TestMethod]
        public void ShouldNotLoadMissingTemplate()
        {
            var t = new TemplateLoader(".\\TestTemplates");

            var template = t.LoadEmailTemplate("missingtemplate", "ru");

            Assert.IsNull(template);
        }

        [TestMethod]
        public void ShouldLoadSelectedCulture()
        {
            var t = new TemplateLoader(".\\TestTemplates");

            var template = t.LoadEmailTemplate("correct", "en");

            Assert.IsNotNull(template);
            Assert.IsNotNull(template.Body);
            Assert.IsNotNull(template.Subject);
        }

        [TestMethod]
        public void ShouldNotLoadBrokenTemplate()
        {
            var t = new TemplateLoader(".\\TestTemplates");

            var template = t.LoadEmailTemplate("broken", "ru");

            Assert.IsNull(template);
        }

        [TestMethod]
        public void ShouldLoadTemplateWithiMissingField()
        {
            var t = new TemplateLoader(".\\TestTemplates");

            var template = t.LoadEmailTemplate("missingsubject", "ru");

            Assert.IsNotNull(template);
            Assert.IsNotNull(template.Body);
            Assert.IsNull(template.Subject);
        }
    }
}
