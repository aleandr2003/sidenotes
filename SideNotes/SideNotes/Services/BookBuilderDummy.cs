using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SideNotes.Services.Abstract;
using SideNotes.Models;
using System.IO;

namespace SideNotes.Services
{
    public class BookBuilderDummy : IBookBuilder
    {
        public void StartBook(string Title, string Author, string Annotation, string ISBN){}
        public void StartChapter(string chapterTitle){}
        public void CloseChapter(){}
        public void AddParagraph(string content){}
        public void AddParagraph(string content, FormatType type) { }
        public void AddParagraph(string content, Paragraph paragraph){}
        public void AddEmptyLine(){}
        public void AddImage(Photo photo){}
        public void SetAvatar(Stream avatarStream){}

        public void Dispose()
        {
        }

    }
}