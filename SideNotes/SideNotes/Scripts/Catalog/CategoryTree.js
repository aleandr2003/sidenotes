function CategoryTree(container, catalog, rootUrl) {
    var self = this;
    var _container = container;
    var _catalog = catalog;
    var _rootUrl = rootUrl;

    _catalog.CategoriesLoaded(function (event) { self.Display(event.data); });

    this.Display = function (data) {
        var link = $('<a href="' + _rootUrl + '" class="categoryLink">' + ResourceStrings.All + '</a>')
            .appendTo(_container)
            .click(function (e) { e.preventDefault(); self.SelectCategory($(this)); });
        if (data && data.length > 0) {
            var list = $('<ul style="padding-left:0"></ul>');
            list.appendTo($(_container));
            for (var i in data) {
                var child = data[i];
                var item = $('<li></li>');
                item.appendTo(list);
                DisplayCategory(child, item);
            }
        }
    };

    this.SelectCategoryById = function (Id) {
        $('.categoryLink').removeClass('selectedCategoryItem');
        $('.categoryLink[categoryId='+Id+']').addClass('selectedCategoryItem');
        _catalog.SelectCategory(Id);
    };

    this.SelectCategory = function (category) {
        $('.categoryLink').removeClass('selectedCategoryItem');
        category.addClass('selectedCategoryItem');
        _catalog.SelectCategory(category.attr('categoryId'));
    };

    var DisplayCategory = function (category, parent) {
        var link = $('<a href="' + _rootUrl + category.Id + '" categoryId="' + category.Id + '" class="categoryLink">' + category.Name + '</a>');
        link.appendTo($(parent));
        link.click(function (e) { e.preventDefault(); self.SelectCategory($(this)) });
        if (category.Children != undefined && category.Children.length > 0) {
            var list = $('<ul></ul>');
            list.appendTo($(parent));
            for (var i in category.Children) {
                var child = category.Children[i];
                var item = $('<li></li>');
                item.appendTo(list);
                DisplayCategory(child, item);
            }
        }
    };
}



CategoryTree.prototype.constructor = CategoryTree;