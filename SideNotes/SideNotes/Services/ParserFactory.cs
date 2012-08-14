using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SideNotes.Services.Abstract;

namespace SideNotes.Services
{
    public class ParserFactory
    {
        private IImageKeeper imageKeeper;
        private IBookBuilder bookBuilder;
        public ParserFactory(IImageKeeper ikeeper, IBookBuilder bookBuilder)
        {
            this.bookBuilder = bookBuilder;
            imageKeeper = ikeeper;
        }
        public IEBookParser CreateParser(ParserType type)
        {
            if (type == ParserType.FB2Parser)
            {
                return new FB2Parser(this.bookBuilder, imageKeeper);
            }
            throw new ArgumentException("Неподдерживаемый формат книги");
        }
    }

    public enum ParserType
    {
        Unknown,
        FB2Parser
    }
}