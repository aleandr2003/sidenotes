function Feed() {
    Observable.call(this);
    var self = this;
    this.dropdown = new CommentatorsDropdown();
    this.commentLists = [];

    var getParagraphs = function (url, UserIds) {
        var settingDummy = jQuery.ajaxSettings.traditional;
        jQuery.ajaxSettings.traditional = true;
        var postData = { UserIds: UserIds, sortType: currentSortType, page: currentPage };
        downloadParagraphsStarted();
        $.ajax({
            'url': url,
            'type': 'POST',
            'dataType': 'json',
            'data': postData,
            'success': getParagraphsSuccess,
            'error': function () { },
            'beforeSubmit': function () {
                //показываем крутилку
            },
            'complete': function () {

            }
        });
        jQuery.ajaxSettings.traditional = settingDummy;
        
    };

    var getParagraphsSuccess = function (data) {
        for (var i in data) {
            var paragraph = data[i];
            var paragraphContainer = $('#paragraphTemplate').clone().appendTo('#commentList_container');
            paragraphContainer.removeAttr('Id');
            paragraphContainer.attr('paragraphId', paragraph.Id);
            paragraphContainer.attr('BookId', paragraph.BookId);
            paragraphContainer.attr('OrderNumber', paragraph.OrderNumber);
            paragraphContainer.attr('FormatType', paragraph.FormatType);
            paragraphContainer.attr('externalLink', paragraph.BookTextUrl);
            paragraphContainer.find('.toTextButtonLink').attr('href', paragraph.BookTextUrl);

            var bookBlock = paragraphContainer.find('.bookBlock_inner');
            fillBookBlock(bookBlock, paragraph);
            
            var commentList = new CommentList(paragraphContainer.find('.feedBlockRight'), self, commentCountUrl, commentsUrl, repliesCountUrl);
            
            commentList.init(paragraph.FirstComment, paragraph.LastComment, paragraph.Id, paragraph.Content, paragraph.BookTextUrl);
            
            commentList.Display();
            
            self.commentLists[''+paragraph.Id] = commentList;
            
            paragraphContainer.show();

            //initAddHeadLinks(paragraphContainer.find('.addHeadLink'));
        }
        downloadParagraphsFinished();
        if (data.length == 0) noParagraphsReterned();
        
    };

    var fillBookBlock = function (bookBlock, paragraph) {
        if (paragraph.FormatType == FormatType.Image) {
            bookBlock.empty();
            var img = $(paragraph.Content).appendTo(bookBlock);
            if (parseInt(img.attr('width')) > bookBlockWidth - 10) {
                img.attr('width', '' + (bookBlockWidth - 10) + 'px');
                img.removeAttr('height');
            }
        } else {
            bookBlock.html(paragraph.Content).shorten({ limit: paragraphLengthLimit });
        }
    };

    var getSingleParagraphs = function (url, Id) {
        var postData = { Id: Id };
        $.ajax({
            'url': url,
            'type': 'POST',
            'dataType': 'json',
            'data': postData,
            'success': getSingleParagraphsSuccess,
            'error': function () { },
            'beforeSubmit': function () {
                //показываем крутилку
            },
            'complete': function () {

            }
        });
    };

    var getSingleParagraphsSuccess = function (data) {
        var paragraph = data;
        var paragraphContainer = $('#paragraphTemplate').clone().appendTo('#commentList_container');
        paragraphContainer.removeAttr('Id');
        paragraphContainer.attr('paragraphId', paragraph.Id);
        paragraphContainer.attr('BookId', paragraph.BookId);
        paragraphContainer.attr('OrderNumber', paragraph.OrderNumber);
        paragraphContainer.attr('FormatType', paragraph.FormatType);
        paragraphContainer.find('.toTextButtonLink').attr('href', paragraph.BookTextUrl);

        var bookBlock = paragraphContainer.find('.bookBlock_inner');
        fillBookBlock(bookBlock, paragraph);

        var commentList = new CommentList(paragraphContainer.find('.feedBlockRight'), self, commentCountUrl, commentsUrl, repliesCountUrl);
        commentList.init(paragraph.FirstComment, paragraph.LastComment, paragraph.Id, paragraph.Content, paragraph.BookTextUrl);
        commentList.Display();
        self.commentLists[' ' + paragraph.Id] = commentList;

        paragraphContainer.show();
        //initAddHeadLinks(paragraphContainer.find('.addHeadLink'));
        self.dropdown.all();
    };

    //function fillCommentText(commentContainer, text) {
    //    var commentChunk = commentContainer.find('span.commentChunk');
    //    var commentWhole = commentContainer.find('span.commentWhole');
    //    var readMore = commentContainer.find('.comment_readmore').attr('href', 'javascript:void(0)');
    //    if (text.length <= commentLengthLimit) {
    //        readMore.hide();
    //        commentChunk.html(text);
    //        commentWhole.hide();
    //    } else {
    //        commentChunk.html(text.substr(0, commentLengthLimit));
    //        commentWhole.html(text).hide();
    //        readMore.click(function () {
    //            $(this).parents('.comment_container').find('span.commentChunk').hide();
    //            $(this).parents('.comment_container').find('span.commentWhole').show();
    //            $(this).hide();
    //        });
    //        readMore.show();
    //    }
    //}

    var clearPage = function () {
        currentPage = 1;
        $('#commentList_container').empty();
        $('#navigateDownButton').show();
    };

    var downloadParagraphsStarted = function () {
        $('#navigateDownButton').addClass('navigateDownButtonPressed');
    };
    var downloadParagraphsFinished = function () {
        $('#navigateDownButton').removeClass('navigateDownButtonPressed');
    };
    var noParagraphsReterned = function () {
        $('#navigateDownButton').hide();
    };

    this.getUserIds = function () {
        return self.dropdown.getUserIds();
    };

//    this.UpdateCommentstors = function(){
//        self.dropdown.getCommentators(commentatorsUrl);
//    }

    this.initBookFeed = function () {

        self.dropdown.loaded(function () {
            clearPage();
            getParagraphs(paragraphsUrl, self.getUserIds());
        });
        self.dropdown.changed(function () {
            clearPage();
            getParagraphs(paragraphsUrl, self.getUserIds());
        });


        var replyForm = $('#replyForm')[0];
        replyForm.success = function (data) {
            if (data != null && data.ErrorMessage != null) {
                $('#lightbox_shadow').click();
                alert(ResourceStrings.OopsError + ": " + data.ErrorMessage);
            } else {
                $(replyForm).find('[name=commentText]').val('');
                var Id = $(replyForm).find('[name=headCommentId]').val();
                var paragraphId = $(replyForm).find('[name=entityId]').val();
                var commentList = self.commentLists[''+paragraphId];
                commentList.getRepliesCount(Id);
                $('#lightbox_shadow').click();
            }
        };
        var addForm = $('#addHeadCommentBoxForm')[0];
        addForm.success = function (data) {
            if (data != null && data.ErrorMessage != null) {
                $('#lightbox_shadow').click();
                alert(ResourceStrings.OopsError + ": " + data.ErrorMessage);
            } else {
                $(addForm).find('[name=commentText]').val('');
                var Id = $(addForm).find('[name=entityId]').val();
                //self.dropdown.getCommentators(commentatorsUrl);
                var commentList = self.commentLists[''+Id];
                commentList.showCommentsList();
                commentList.getCommentsCount();
                reloadBookCommentsCount();
                $('#lightbox_shadow').click();
            }
        };

        $('#feedSortByDate').attr('sortType', SortType_ByRecentComments);
        $('#feedSortByText').attr('sortType', SortType_ByOrderNumber);

        $('.sortTypeTab').removeClass('sortSelect_item_current');
        $('.sortTypeTab[sortType=' + currentSortType + ']').addClass('sortSelect_item_current');
        $('.sortTypeTab').click(function () {
            $('.sortTypeTab').removeClass('sortSelect_item_current');
            $(this).addClass('sortSelect_item_current');
            currentSortType = $(this).attr('sortType');
            clearPage();
            getParagraphs(paragraphsUrl, self.getUserIds());
        });
        $('#navigateDownButton').click(function () {
            if ($(this).hasClass('navigateDownButtonPressed')) return;
            currentPage += 1;
            getParagraphs(paragraphsUrl, self.getUserIds());
        });
        $(window).scroll(function () {
            if ($(document).height() - $(window).height() <= $(window).scrollTop() + 50) {
                $('#navigateDownButton:visible').click();
            }
        });

        self.dropdown.getCommentators(commentatorsUrl);
    }

    this.initParagraphFeed = function (Id) {
        self.dropdown.changed(function () {
            self.commentLists['' + Id].showCommentsList();
        });

        self.dropdown.loaded(function () {
            getSingleParagraphs(singleParagraphUrl, Id);
        });

        var replyForm = $('#replyForm')[0];
        replyForm.success = function (data) {
            if (data != null && data.ErrorMessage != null) {
                $('#lightbox_shadow').click();
                alert(ResourceStrings.OopsError + ": " + data.ErrorMessage);
            } else {
                $(replyForm).find('[name=commentText]').val('');
                var Id = $(replyForm).find('[name=headCommentId]').val();
                var paragraphId = $(replyForm).find('[name=entityId]').val();
                var commentList = self.commentLists[' ' + paragraphId];
                commentList.getRepliesCount(Id);
                $('#lightbox_shadow').click();
            }
        };
        var addForm = $('#addHeadCommentBoxForm')[0];
        addForm.success = function (data) {
            if (data != null && data.ErrorMessage != null) {
                $('#lightbox_shadow').click();
                alert(ResourceStrings.OopsError + ": " + data.ErrorMessage);
            } else {
                $(addForm).find('[name=commentText]').val('');
                var Id = $(addForm).find('[name=entityId]').val();
                //self.dropdown.getCommentators(commentatorsUrl);
                var commentList = self.commentLists[' ' + Id];
                commentList.showCommentsList();
                commentList.getCommentsCount();
                reloadBookCommentsCount();
                //getCommentsCount(Id, commentCountUrl, filter);
                $('#lightbox_shadow').click();
            }
        };

        $('#sortSelect_list').hide();
        $('#navigateDownButton').hide();

        self.dropdown.getCommentators(commentatorsUrl);
    }
}

Feed.prototype = new Observable();
Feed.prototype.constructor = Feed;