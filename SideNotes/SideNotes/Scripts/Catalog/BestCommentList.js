function BestCommentList(container, catalog, commentUrl) {
    var listSize = 3;
    var self = this;
    var _container = container;
    var _catalog = catalog;
    var _commentUrl = commentUrl;
    var _bookIds;

    _catalog.BooksLoaded(function (event) { self.LoadCommentsList(_catalog.GetBookIds()); });

    this.LoadCommentsList = function (BookIds) {
        $(_container).empty();
        _bookIds = shuffle(BookIds);
        for (var i = 0; i < listSize; i++) {
            LoadComment();
        }
    };

    var LoadComment = function () {
        if (_bookIds && _bookIds.length > 0) {
            var BookId = _bookIds.pop();
            $.ajax({
                'url': _commentUrl,
                'type': 'POST',
                'data': { 'Id': BookId },
                'success': LoadCommentSuccess,
                'error': function () { }
            });
        }
    }

    var LoadCommentSuccess = function (data) {
        if (!data || data.BookId == undefined) LoadComment();
        else {
            var commentModule = new BestComment(_container, data);
            commentModule.Display();
        }
    }
}

BestCommentList.prototype.constructor = BestCommentList;