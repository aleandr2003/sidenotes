function BestComment(container, data) {
    var bookBlockWidth = 250;
    var templateStr = '<div class="bestComment_container">\
    <div class="bookReference">\
        <a href="" class="book_avatar_link">\
	        <img src="" alt="" class="book_avatar" title="">\
    	</a>\
        <div class="bookCaption">\
	        <a href="" class="bestComment_bookTitle"></a>\
            <a href="" class="bestComment_bookAuthor"></a>\
        </div>\
        <div class="separator"> </div>\
    </div>\
    <div class="paragraphBlock">\
            <div class="bestCommentQuote">\
                 <img src="../../Content/img/quote.png" />\
            </div>\
        <a href="" class="paragraph_content"></a>\
        <div class="separator">&nbsp;</div>\
    </div>\
    <a href="" class="userblock_avatar_link"><img src="" alt="" class="" title=""/></a>\
    <a href="" class="userblock_name"></a>\
    <a href="" class="comment_content"></a>\
    <div class="commentCounter">еще <a href="" class="commentCounter_counter"></a> к этой книге</div>\
    </div>';

    var self = this;
    var _container = container;
    var _data = data;
    var div = $(templateStr);

    this.Display = function () {
        div.hide().appendTo(_container);
        self.Fill();
        div.show();
    };
    this.Fill = function () {
        div.find('.book_avatar_link').attr('href', _data.BookReadUrl);
        div.find('.book_avatar').attr('src', _data.BookAvatarUrl).attr('alt', _data.BookTitle).attr('title', _data.BookTitle);
        div.find('.bestComment_bookTitle').attr('href', _data.BookReadUrl).html(_data.BookTitle);
        div.find('.bestComment_bookAuthor').attr('href', _data.BookReadUrl).html(_data.BookAuthor);
        var bookBlock = div.find('.paragraph_content').attr('href', _data.ParagraphUrl);
        fillBookBlock(bookBlock, { Content: _data.ParagraphContent, FormatType: _data.ParagraphFormatType });
        div.find('.userblock_avatar_link img').attr('src', _data.UserAvatarUrl)
            .attr('alt', _data.UserName).attr('title', _data.UserName);
        div.find('.userblock_avatar_link').attr('href', _data.UserProfileUrl);
        div.find('.userblock_name').attr('href', _data.UserProfileUrl).html(_data.UserName);
        div.find('.comment_content').attr('href', _data.ParagraphUrl).html(_data.CommentText);
        if (_data.UserCommentsCount > 1) {
            div.find('.commentCounter_counter').attr('href', _data.CommentsByUserUrl).html(_data.UserCommentsCount - 1);
        } else {
            div.find('.commentCounter').hide();
        }
    }

    function fillBookBlock(bookBlock, paragraph) {
        if (paragraph.FormatType == FormatType.Image) {
            bookBlock.empty();
            var img = $(paragraph.Content).appendTo(bookBlock);
            if (parseInt(img.attr('width')) > bookBlockWidth - 10) {
                //alert(bookBlockWidth - 10);
                img.attr('width', '' + (bookBlockWidth - 10) + 'px');
                img.removeAttr('height');
            }
        } else {
            bookBlock.html(paragraph.Content);
        }
    }
}

BestComment.prototype.constructor = BestComment;
