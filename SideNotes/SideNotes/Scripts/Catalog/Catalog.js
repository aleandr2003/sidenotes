function Catalog(loadCategoriesUrl, loadBooksUrl) {
    Observable.call(this);
    var self = this;
    var books;
    var rootCategories;
    var selectedCategory;

    var _loadCategoriesUrl = loadCategoriesUrl;
    var _loadBooksUrl = loadBooksUrl;
    
    this.LoadCategories = function () {
        $.ajax({
            'url': _loadCategoriesUrl,
            'type': 'POST',
            'success': LoadCategoriesSuccess,
            'error': ajaxErrorFunc
        });
    }

    this.LoadBooks = function (categoryId) {
        var postData = {};
        if (categoryId) {
            postData = { Id: categoryId };
        }
        $.ajax({
            'url': _loadBooksUrl,
            'type': 'POST',
            'data': postData,
            'success': LoadBooksSuccess,
            'error': ajaxErrorFunc
        });
    }

    this.SelectCategory = function (categoryId) {
        self.LoadBooks(categoryId);
    }

    this.GetBookIds = function () {
        if (books && books.length > 0) {
            var Ids = [];
            for (var i in books) {
                var book = books[i];
                Ids.push(book.Id);
            }
            return Ids;
        }
        return [];
    }

    function LoadCategoriesSuccess(data) {
        self.CategoriesLoaded(data);
    }

    function LoadBooksSuccess(data) {
        if(data){
            books = data.books.concat(data.subfolderBooks);
        }
        self.BooksLoaded(data);
    }

    function ajaxErrorFunc(){
        alert('Ошибка при загрузке');
    }
}

Catalog.prototype = new Observable();
Catalog.prototype.constructor = Catalog;

Catalog.prototype.CategoriesLoaded = function (obj) {
    if (typeof obj == "object") this.dispatch({ type: "CategoriesLoaded", data: obj });
    else if (typeof obj == "function") {
        this.addObserver("CategoriesLoaded", obj);
    }
}

Catalog.prototype.CategorySelected = function (obj) {
    if (typeof obj == "object") this.dispatch({ type: "CategorySelected", data: obj });
    else if (typeof obj == "function") {
        this.addObserver("CategorySelected", obj);
    }
}

Catalog.prototype.BooksLoaded = function (obj) {
    if (typeof obj == "object") this.dispatch({ type: "BooksLoaded", data: obj });
    else if (typeof obj == "function") {
        this.addObserver("BooksLoaded", obj);
    }
}

