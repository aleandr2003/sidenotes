using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SideNotes.Models;
using System.IO;

namespace SideNotes.Services.Abstract
{
    public interface IBookBuilder:IDisposable
    {
        void StartBook(string Title, string Author, string Annotation, string ISBN);
        void StartChapter(string chapterTitle);
        void CloseChapter();
        void AddParagraph(string content);
        void AddParagraph(string content, Paragraph paragraph);
        void AddParagraph(string content, FormatType type);
        void AddEmptyLine();
        void AddImage(Photo photo);
        void SetAvatar(Stream avatarStream);
    }
}