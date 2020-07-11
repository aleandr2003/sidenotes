using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SideNotes.Models;
using System.Xml.Linq;
using System.IO;
using System.Web.Mvc;
using SideNotes.Extensions;

namespace SideNotes.Services
{
    public class FB2Builder
    {
        private SideNotesEntities context;
        private int BookId;
        private UrlHelper urlHelper;

        public FB2Builder(int Id, SideNotesEntities context, UrlHelper urlHelper)
        {
            BookId = Id;
            this.context = context;
            this.urlHelper = urlHelper;
        }

        public string Serialize()
        {
            var fb = new XElement("FictionBook");
            
            //create fictionbook element;
            //add description element;
            
            //add notes
                //add paragraph notes
                //add comment notes
            //add body
                //select chapters ordered by first paragraph
                
            
            //add binaries
                //add cover image
                //add paragraph images
            return fb.ToString();
        }

        private XElement SerializeDescription(Book book)
        {
            var Authors = SerializeAuthors(book.Author);
            var titleinfo = new XElement("title-info",
                    Authors,
                    new XElement("book-title", book.Title));
            if(!String.IsNullOrEmpty(book.Annotation)){
                titleinfo.Add(new XElement("annotation", book.Annotation));
            }
            var description = new XElement("description", titleinfo);
            

            return description;
        }

        private List<XElement> SerializeAuthors(string AuthorsNames)
        {
            if (String.IsNullOrEmpty(AuthorsNames)) return new List<XElement>();
            var authors = AuthorsNames.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries)
                .Select(s => s.Trim()).ToList();
            var authorsElements = new List<XElement>();
            foreach (var author in authors)
            {
                var authorElement = new XElement("author");
                var nameparts = author.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                if (nameparts.Length == 0) continue;
                else if (nameparts.Length == 1)
                {
                    authorElement.Add(new XElement("last-name", nameparts[0]));
                }
                else if (nameparts.Length == 2)
                {
                    authorElement.Add(new XElement("first-name", nameparts[0]));
                    authorElement.Add(new XElement("last-name", nameparts[1]));
                }
                else if (nameparts.Length == 3)
                {
                    authorElement.Add(new XElement("first-name", nameparts[0]));
                    authorElement.Add(new XElement("middle-name", nameparts[1]));
                    authorElement.Add(new XElement("last-name", nameparts[2]));
                }
                else
                {
                    authorElement.Add(new XElement("last-name", String.Join(" ", nameparts)));
                }
                authorsElements.Add(authorElement);
            }
            return authorsElements;
        }

        private XElement SerializeChapter(Chapter chapter, List<Paragraph> paragraphs)
        {
            var _paragraphs = new List<Paragraph>(paragraphs.OrderBy(p => p.OrderNumber).ToList());
            Dictionary<int, XElement> serializedChapters = new Dictionary<int, XElement>();
            if (chapter.ChildChapters != null && chapter.ChildChapters.Any())
            {
                foreach (Chapter child in chapter.ChildChapters.OrderByDescending(c => c.OrderNumber))
                {
                    var topParagraph = _paragraphs.FirstOrDefault(p => p.Id == child.TopParagraph_Id);
                    serializedChapters[child.OrderNumber] = SerializeChapter(child, _paragraphs.Where(p => p.OrderNumber >= topParagraph.OrderNumber).ToList());
                    _paragraphs.RemoveAll(p => p.OrderNumber >= topParagraph.OrderNumber);
                }
            }
            var section = new XElement("section");
            if (_paragraphs.Any())
            {
                foreach (var paragraph in _paragraphs.OrderBy(p => p.OrderNumber))
                {
                    section.Add(SerializeParagraph(paragraph));
                }
            }
            if (serializedChapters.Any())
            {
                foreach (var element in serializedChapters.OrderBy(p => p.Key).Select(kvp => kvp.Value))
                {
                    section.Add(element);
                }
            }
            return section;
        }

        private XElement SerializeParagraph(Paragraph paragraph)
        {
            return new XElement("p", paragraph.Content);
        }

        private XElement SerializeComments(string Id, List<HeadComment> comments, string replyUrl)
        {
            var section = new XElement("section");
            section.SetAttributeValue("id", Id);
            section.Add(new XElement("title", "*"));
            foreach (var comment in comments.Where(c => c.Author_Id != null).OrderBy(c => c.DateCreated))
            {
                section.Add(new XElement("p", String.Format("<strong>{0}</strong>: {1};", comment.Author.Name, comment.Text)));
            }
            var replyLink = new XElement("a", "ответить");
            replyLink.SetAttributeValue("href", replyUrl);
            section.Add(new XElement("p",replyLink));
            return section;
        }

        private XElement SerializeFootNote(FootNote note)
        {
            var section = new XElement("section",
                new XElement("title", note.GetTitle()),
                new XElement("p", note.GetNote()));
            section.SetAttributeValue("id", note.GetNoteId());
            return section;
        }

        private XElement SerializeBinary(string path, string Id, string contentType)
        {
            if (File.Exists(path))
            {
                var bytes = File.ReadAllBytes(path);
                var binary = new XElement("binary", System.Convert.ToBase64String(bytes));
                binary.SetAttributeValue("content-type", contentType);
                binary.SetAttributeValue("id", Id);
                return binary;
            }
            return null;
        }

        private class BinaryElement
        {
            public string Id;
            public string path;

            public BinaryElement(string path)
            {
                if (!File.Exists(path)) throw new InvalidOperationException(String.Format(Resources.ErrorMessages.FileDoesNotExist, path));
                this.path = path;

            }
        }
    }
}