function BookList(container, catalog) {
    var rowSize = 3;
    var self = this;
    var _container = container;
    var _catalog = catalog;

    _catalog.BooksLoaded(function (event) { self.Display(event.data); });

    this.Display = function (data) {
        $(_container).empty();
        var books = [];
        if (data) {
            books = data.books.concat(data.subfolderBooks);
        }
        if (books.length > 0) {
            
            var table = $('<table class="bookListTable"></table>');
            table.appendTo(_container);

            var row = [];
            for (var i = 0, rowI = 0; i < books.length; i++) {
                row.push(books[i]);
                if (rowI + 1 == rowSize || i + 1 == books.length) {
                    DisplayRow(row, table);
                    rowI = 0;
                    row = [];
                } else {
                    rowI++;
                }
            }
        } else {
            $('<span>' + ResourceStrings.NoBooksMessage + '</span>').appendTo(_container);
        }
    };

    var DisplayRow = function (books, parent) {
        var row = $('<tr></tr>');
        row.appendTo(parent);
        for (var i in books) {
            var book = books[i];
            var cell = $('<td></td>');
            cell.appendTo(row);
            DisplayBook(book, cell);
        }
    };

    var DisplayBook = function (book, parent) {
        $('<div class="bookListCellItem"><a href="' + book.ReadUrl + '"><img src="' + book.AvatarUrl+ '"></a></div>').appendTo(parent);
    };
}

BookList.prototype.constructor = BookList;