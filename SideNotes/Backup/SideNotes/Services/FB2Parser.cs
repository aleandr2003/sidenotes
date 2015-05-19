using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SideNotes.Services.Abstract;
using System.IO;
using System.Xml.Linq;
using System.Xml.XPath;
using System.Xml;
using SideNotes.Models;
using System.Text;
using System.Drawing;
using SideNotes.Extensions;

namespace SideNotes.Services
{
    public class FB2Parser:IEBookParser
    {
        private IBookBuilder bookBuilder;
        private IImageKeeper imageKeeper;
        private XmlNamespaceManager nsManager;
        private string currentBookTitle = "";
        private Dictionary<string, Photo> images;
        private XNamespace xlink;
        private XDocument xdoc;


        public FB2Parser(IBookBuilder builder, IImageKeeper ikeeper)
        {
            bookBuilder = builder;
            imageKeeper = ikeeper;
            nsManager = new XmlNamespaceManager(new NameTable());
            nsManager.AddNamespace("x", "http://www.gribuser.ru/xml/fictionbook/2.0");
            xlink = "http://www.w3.org/1999/xlink";
            images = new Dictionary<string, Photo>();
        }
        public void Parse(Stream input)
        {
            StreamReader reader = new StreamReader(input);
            string fileContent = reader.ReadToEnd();
            xdoc = XDocument.Parse(fileContent);
            ParseDocument();
        }

        private void ParseDocument()
        {
            XElement descriptionNode = xdoc.XPathSelectElement("/x:FictionBook/x:description", nsManager);
            XElement titleInfo = descriptionNode.XPathSelectElement("x:title-info", nsManager);
            string title = titleInfo.XPathSelectElement("x:book-title", nsManager).Value;

            string author = String.Join(", ", 
                titleInfo.XPathSelectElements("x:author", nsManager)
                .Select(a => GetAuthorName(a))
             );

            var annotationElement = titleInfo.XPathSelectElement("x:annotation", nsManager);
            string annotation = annotationElement == null ? "" : annotationElement.Value;
            var isbnElement = descriptionNode.XPathSelectElement("x:publish-info/x:isbn", nsManager);
            string isbn = isbnElement == null ? "" : isbnElement.Value;
            bookBuilder.StartBook(title, author, annotation, isbn);
            currentBookTitle = title;
            ParseImages(xdoc.XPathSelectElements("/x:FictionBook/x:binary[contains(@content-type,'image/')]", nsManager));
            ParseBody(xdoc.XPathSelectElement("/x:FictionBook/x:body", nsManager));
            UseCoverImage(titleInfo.XPathSelectElement("x:coverpage/x:image", nsManager));

        }

        private void UseCoverImage(XElement cover)
        {
            if (cover == null) return;
            string Id = cover.Attribute(xlink + "href").Value;
            if (Id.StartsWith("#")) Id = Id.Substring(1);
            if (File.Exists(images[Id].Location))
            {
                bookBuilder.SetAvatar(File.OpenRead(images[Id].Location));
            }
        }

        private void ParseImages(IEnumerable<XElement> imageElements)
        {
            using (var context = new SideNotesEntities())
            {
                foreach (var element in imageElements)
                {
                    string Id = element.Attribute("id").Value;
                    string filename = DateTime.Now.ToString("yyyyMMdd-HHmm")+ "_" + Id.Substring(0, Id.LastIndexOf("."));
                    byte[] byteArray = Convert.FromBase64String(element.Value);
                    MemoryStream stream = new MemoryStream(byteArray);
                    Photo newPhoto = imageKeeper.Upload(new Bitmap(stream), filename);
                    images.Add(Id, newPhoto);
                    context.Photos.AddObject(newPhoto);
                }
                context.SaveChanges();
            }
        }

        private void ParseBody(XElement body)
        {
            var titleElement = body.XPathSelectElement("x:title", nsManager);
            string title = titleElement != null ? titleElement.XPathSelectElement("x:p", nsManager).Value : currentBookTitle;
            bookBuilder.StartChapter(title);
            foreach (var section in body.Elements().Where(e => e.Name.LocalName == "section"))
            {
                ParseSection(section);
            }
            bookBuilder.CloseChapter();
        }

        private void ParseSection(XElement section)
        {
            var titleElement = section.XPathSelectElement("x:title", nsManager);
            IEnumerable<XElement> elements = null;
            bool chapterStarted = false;
            if (titleElement != null)
            {
                string title = titleElement.Value;
                bookBuilder.StartChapter(title);
                chapterStarted = true;
                ParseTitle(titleElement);
                elements = titleElement.ElementsAfterSelf();
            }
            else
            {
                elements = section.Elements();
            }
            foreach (var element in elements)
            {
                if (element.Name.LocalName == "section")
                {
                    ParseSection(element);
                }
                else
                {
                    ParseElement(element);
                }
            }
            if (chapterStarted) bookBuilder.CloseChapter();
        }

        private void ParseTitle(XElement title)
        {
            if (title == null) return;
            foreach (var child in title.Elements())
            {
                if (child.Name.LocalName == "empty-line")
                {
                    bookBuilder.AddEmptyLine();
                }
                else
                {
                    AddParagraph(child, FormatType.Title);
                }
            }
        }

        private void ParseImage(XElement element)
        {
            string Id = element.Attribute(xlink + "href").Value;
            if (Id.StartsWith("#")) Id = Id.Substring(1);
            bookBuilder.AddImage(images[Id]);
        }

        private void ParseElement(XElement element)
        {
            if (element.Name.LocalName == "empty-line" || 
                (element.Name.LocalName == "p" && String.IsNullOrEmpty(element.Value.Trim())))
            {
                bookBuilder.AddEmptyLine();
            }
            else if (element.Name.LocalName == "poem")
            {
                ParsePoem(element);
            }
            else if (element.Name.LocalName == "epigraph")
            {
                ParseEpigraph(element);
            }
            else if (element.Name.LocalName == "cite")
            {
                ParseCite(element);
            }
            else if (element.Name.LocalName == "image")
            {
                ParseImage(element);
            }
            else //if (element.Name.LocalName == "p")
            {
                AddParagraph(element, FormatType.Common);
            }
            
        }

        private void ParseEpigraph(XElement epigraph)
        {
            foreach (var element in epigraph.Elements())
            {
                if (element.Name.LocalName == "empty-line")
                {
                    bookBuilder.AddEmptyLine();
                }
                else if (element.Name.LocalName == "text-author")
                {
                    AddParagraph(element, FormatType.EpigraphAuthor);
                }
                else if (element.Name.LocalName == "poem")
                {
                    ParsePoem(element);
                }
                else //if (element.Name.LocalName == "p")
                {
                    AddParagraph(element, FormatType.Epigraph);
                }
            }
        }

        private void ParseCite(XElement cite)
        {
            foreach (var element in cite.Elements())
            {
                if (element.Name.LocalName == "empty-line")
                {
                    bookBuilder.AddEmptyLine();
                }
                else if (element.Name.LocalName == "text-author")
                {
                    AddParagraph(element, FormatType.CiteAuthor);
                }
                else if (element.Name.LocalName == "poem")
                {
                    ParsePoem(element);
                }
                else //if (element.Name.LocalName == "p")
                {
                    AddParagraph(element, FormatType.Cite);
                }
            }
        }

        private void ParsePoem(XElement poem)
        {
            foreach (var element in poem.Elements())
            {
                if (element.Name.LocalName == "title")
                {
                    ParseTitle(element);
                }
                else if (element.Name.LocalName == "empty-line")
                {
                    bookBuilder.AddEmptyLine();
                }
                else if (element.Name.LocalName == "text-author")
                {
                    AddParagraph(element, FormatType.PoemAuthor);
                }
                else if (element.Name.LocalName == "stanza")
                {
                    ParseStanza(element);
                }
                else if (element.Name.LocalName == "epigraph")
                {
                    ParseEpigraph(element);
                }
                else //if (element.Name.LocalName == "p")
                {
                    AddParagraph(element, FormatType.Poem);
                }
            }
        }

        private void ParseStanza(XElement stanza)
        {
            var verses = stanza.Elements().Select(e => e.Value);
            AddParagraph(XElement.Parse("<p>" + String.Join("<br/>", verses.ToArray()) + "</p>"), FormatType.Poem);
        }

        private void AddParagraph(XElement element, FormatType formatType)
        {
            var footNoteLinks = fixNoteLinks(element);
            bookBuilder.AddParagraph(element.InnerXml(), formatType);
            AddFootNotes(footNoteLinks);
        }

        private void AddFootNotes(List<XElement> footNoteLinks)
        {
            foreach (var link in footNoteLinks)
            {
                var Id = link.Attribute(xlink + "href").Value.Substring(1);
                var note = xdoc.Descendants().FirstOrDefault(e => e.Attribute("id") != null && e.Attribute("id").Value == Id);
                if (note != null)
                {
                    var newTitle = XElement.Parse("<p class=\"footNoteTitle\" noteId=\"" + Id + "\"></p>");
                    var titleElement = note.XPathSelectElement("x:title", nsManager);
                    if (titleElement != null)
                    {
                        newTitle.Value = titleElement.Value;
                        titleElement.ReplaceWith(newTitle);
                    }
                    else
                    {
                        newTitle.Value = link.Value;
                        note.AddFirst(newTitle);
                    }
                    bookBuilder.AddParagraph(note.InnerXml(), new FootNote());
                }
            }
        }

        private List<XElement> fixNoteLinks(XElement element)
        {
            var footNoteLinks = element.XPathSelectElements("x:a[@type='note']", nsManager)
                .Where(l => l.Attribute(xlink + "href") != null).ToList();
            //var footNoteIds = footNoteLinks.Select(l => l.Attribute(xlink + "href")).Select(a => a.Value.Substring(1));

            //Меняем <a xlink:href="#note_9" type="note">{9}</a> на <a href="javascript:void(0)" rel="note_9">{9}</a>
            footNoteLinks.ForEach(l =>
                l.ReplaceWith(
                    XElement.Parse(
                        "<a href=\"javascript:void(0)\" rel=\""
                            +
                            l.Attribute(xlink + "href").Value.Substring(1)
                            + "\" class=\"footNoteLink\">" + l.Value + "</a>")
                )
            );
            return footNoteLinks;
        }

        private string GetAuthorName(XElement author)
        {
            var firstName = author.XPathSelectElement("x:first-name", nsManager);
            var middleName = author.XPathSelectElement("x:middle-name", nsManager);
            var lastName = author.XPathSelectElement("x:last-name", nsManager);
            return String.Format("{0} {1} {2}", firstName != null ? firstName.Value : "",
                                                middleName != null ? middleName.Value : "",
                                                lastName != null ? lastName.Value : "");
        }
        public void Dispose()
        {
            bookBuilder.Dispose();
        }
    }
}