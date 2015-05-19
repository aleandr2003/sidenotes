using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SideNotes.Services.Abstract;
using SideNotes.Models;
using System.IO;

namespace SideNotes.Services
{
    public class BookBuilder : IBookBuilder, IDisposable
    {
        private Stack<Chapter> parentChapters;
        private int chapterCounter = 0;
        private int paragraphCounter = 0;
        private Book currentBook;
        private SideNotesEntities dataContext;
        private bool topParagraphFilled = false;
        private AvatarService avatarService;

        public BookBuilder(BookAvatarService avatarService)
        {
            this.avatarService = avatarService;
            parentChapters = new Stack<Chapter>();
            dataContext = new SideNotesEntities();
        }

        public void StartBook(string title, string author, string annotation, string ISBN)
        {
            currentBook = new Book();
            currentBook.Title = title;
            currentBook.Author = author;
            currentBook.Annotation = annotation;
            currentBook.ISBN = ISBN;
            dataContext.Books.AddObject(currentBook);
            dataContext.SaveChanges();
        }

        public void StartChapter(string title)
        {
            if (currentBook == null) throw new InvalidOperationException("Сначала создайте книгу");
            var newChapter = new Chapter();
            if (parentChapters.Count > 0)
            {
                newChapter.ParentChapter = parentChapters.Peek();
            }
            newChapter.OrderNumber = ++chapterCounter;
            newChapter.Title = title;
            newChapter.Book = currentBook;
            parentChapters.Push(newChapter);
            dataContext.Chapters.AddObject(newChapter);
            dataContext.SaveChanges();
            topParagraphFilled = false;
        }

        public void CloseChapter()
        {
            parentChapters.Pop();
        }

        public void AddEmptyLine()
        {
            AddParagraph(null, new EmptyLine());
        }

        public void AddImage(Photo photo)
        {
            string img = "<img src=\"" + VirtualPathUtility.ToAbsolute(photo.Url) + "\" width=\"" + photo.width+"\" height=\""+photo.height+"\"/>";
            AddParagraph(img, new ImageParagraph());
        }

        public void AddParagraph(string content)
        {
            AddParagraph(content, new Paragraph());
        }
        public void AddParagraph(string content, FormatType type)
        {
            switch (type)
            {
                case FormatType.Common:
                    AddParagraph(content, new Paragraph());
                    break;
                case FormatType.Title:
                    AddParagraph(content, new Title());
                    break;
                case FormatType.Epigraph:
                    AddParagraph(content, new Epigraph());
                    break;
                case FormatType.EpigraphAuthor:
                    AddParagraph(content, new EpigraphAuthor());
                    break;
                case FormatType.Cite:
                    AddParagraph(content, new Cite());
                    break;
                case FormatType.CiteAuthor:
                    AddParagraph(content, new CiteAuthor());
                    break;
                case FormatType.Poem:
                    AddParagraph(content, new Poem());
                    break;
                case FormatType.PoemAuthor:
                    AddParagraph(content, new PoemAuthor());
                    break;
                case FormatType.Image:
                    AddParagraph(content, new ImageParagraph());
                    break;
                case FormatType.FootNote:
                    AddParagraph(content, new FootNote());
                    break;
            }
        }
        public void AddParagraph(string content, Paragraph paragraph)
        {
            if (currentBook == null) throw new InvalidOperationException("Сначала создайте книгу");
            paragraph.Book = currentBook;
            paragraph.Content = content;
            paragraph.OrderNumber = ++paragraphCounter;
            paragraph.ChapterId = parentChapters.Peek().Id;
            dataContext.Paragraphs.AddObject(paragraph);
            dataContext.SaveChanges();
            if (!topParagraphFilled)
            {
                parentChapters.Where(c => c.TopParagraph_Id == null)
                    .ToList().ForEach(c => c.TopParagraph_Id = paragraph.Id);
                //parentChapters.Peek().TopParagraph_Id = paragraph.Id;
                dataContext.SaveChanges();
            }
            
            topParagraphFilled = true;
        }

        public void SetAvatar(Stream avatarStream)
        {
            avatarService.UploadNew(currentBook.Id, avatarStream);
        }

        public void Dispose()
        {
            dataContext.Dispose();
        }
        
    }
}