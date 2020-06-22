function BookPage(feedUrl, commentatorsUrl, entityCommentatorsUrl, entityListCommentatorsUrl, commentCountUrl, commentCountListUrl) {
    Observable.call(this);
    var self = this;
    this.dropdown = new CommentatorsDropdown();

    var _feedUrl = feedUrl;
    var _commentatorsUrl = commentatorsUrl;
    var _entityCommentatorsUrl = entityCommentatorsUrl;
    var _entityListCommentatorsUrl = entityListCommentatorsUrl;
    var _commentCountUrl = commentCountUrl;
    var _commentCountListUrl = commentCountListUrl;

    var commentatorNameLimit = 10;
    var UserIds = [];
    this.CommentInfosList = [];

    this.initParagraphs = function (paragraphs) {
        paragraphs.find('.addBookmark').hide();
        paragraphs.find('.addBookmarkLink').show();
        paragraphs.find('.addBookmarkLink').click(function () {
            $('#addBookmarkBox .bookmarkName').val($(this).attr('value'));
            $('#addBookmarkBox .bookmarkParagraphId').val($(this).attr('paragraphId'));
        });
        paragraphs.find('.readCommentsLink, .addCommentLink').attr('href', 'javascript:void(0)')
        paragraphs.find('.readCommentsLink, .addCommentLink').click(function () {
            acivateParagraph($(this).closest('.bookBlock'));
            // initRightFrame('commentsFrame', $(this).attr('frameUrl'));
        });
        paragraphs.find('.opaque').removeClass('opaque').addClass('transparent');
        paragraphs.find('.builtInsList').hide();
        paragraphs.find('.builtInsList').css('color', '#999999');

        initContainerClick(paragraphs.find('.bookBlock_inner'));
        initFootNoteLinks(paragraphs.find('.footNoteLink'));
        initFootNotes(paragraphs.find('.bookParagraphFootNote'));
        initCommentsLinks(paragraphs.find('.addHeadLink'));
        initHideButton(paragraphs.find('.hideCommentListButton'));

        var paragraphIds = [];
        paragraphs.each(function () {
            var paragraph = $(this);
            var paragraphId = paragraph.find('.commentInfo_container').attr('paragraphId');
            paragraphIds.push(paragraphId);
            var container = paragraph.find('.commentInfoBlock');
            var infoBlock = new CommentInfo(container, self, paragraphId, _feedUrl, _entityCommentatorsUrl);
            self.CommentInfosList['' + paragraphId] = infoBlock;
            infoBlock.linkPressed(function (e) {
                openCommentList(e.object.paragraphId);
            });
            infoBlock.Display();
        });
        self.updateCommentsCountList(paragraphIds);
    }

    var initCommentsLinks = function (links) {
        links.click(function (e) {
            e.preventDefault();
            var paragraphId = $(this).closest('.bookBlock').attr('paragraphId');
            openCommentList(paragraphId);
        });
    };

    var openCommentList = function (paragraphId) {
        var bookBlock = $('#book_container .bookBlock[paragraphId=' + paragraphId + ']');
        bookBlock.find('.commentInfo_container .commentInfoBlock, .commentAdd_container .commentCounter').hide();
        bookBlock.find('.hideCommentListButton').show();

        var nextSibling = bookBlock.next();
        var commentsContainer;
        if (nextSibling.hasClass('commentsModule')) {
            nextSibling.empty();
            commentsContainer = nextSibling;
        } else {
            commentsContainer = $('<div class="commentsModule"></div>');
        }
        bookBlock.after(commentsContainer);
        
        var prevUrl = setParameter(prevCommentUrl, 'ParagraphId', bookBlock.attr('paragraphId'));
        var prevButton = $('<div class="prevCommentsButton"><a class="prevCommentsArrow" href="' + prevUrl + '"><img src="/Content/img/blank.png"></a><a href="' + prevUrl + '" class="prevCommentsLink">' + ResourceStrings.PreviousComments + '</a></div>').appendTo(commentsContainer);
        var shareBlockContainer = $('<div class="shareBlockLayout shareBlock"></div>').appendTo(commentsContainer);
        var listContainer = $('<div class="list"></div>').appendTo(commentsContainer);
        var nextUrl = setParameter(nextCommentUrl, 'ParagraphId', bookBlock.attr('paragraphId'));
        var nextButton = $('<div class="nextCommentsButton"><a class="nextCommentsArrow" href="' + nextUrl + '"><img src="/Content/img/blank.png"></a><a href="' + nextUrl + '" class="nextCommentsLink">' + ResourceStrings.NextComments + '</a></div>').appendTo(commentsContainer);

        var _url = bookBlock.attr('externalLink');
        var _paragraphText = bookBlock.find('.bookBlock_inner').html();
        var shareBlock = new ShareBlock(shareBlockContainer, _paragraphText, _url, bookTitle + ' (' + bookAuthor + ')', bookAvatarMedium);
        shareBlock.init();


        commentsContainer.hide();
        var commentList = new CommentList(listContainer, self, _commentCountUrl, commentsUrl, repliesCountUrl);
        var paragraphId = bookBlock.attr('paragraphId');
        commentList.initNoComments(paragraphId, '', '');
        commentList.Display();
        commentList.showCommentsList();
        commentList.commentAdded(function (e) { self.CommentInfosList['' + e.object.paragraphId].update(); });
        commentList.commentDeleted(function (e) { self.CommentInfosList['' + e.object.paragraphId].update(); });
        commentsContainer.slideDown();
    }

    var initHideButton = function (links) {
        links.click(function (e) {
            e.preventDefault();
            var bookBlock = $(this).closest('.bookBlock');
            var nextSibling = bookBlock.next();
            var commentsContainer;
            if (nextSibling.hasClass('commentsModule')) {
                nextSibling.slideUp();
                //nextSibling.empty();
            }
            bookBlock.find('.hideCommentListButton').hide();
            bookBlock.find('.commentInfo_container .commentInfoBlock, .commentAdd_container .commentCounter').show();
        });
    };
    var initAddHeadLinksWithBox = function (links) {
        links.click(function (e) {
            e.preventDefault();
            var entityId = $(this).attr('paragraphId');
            var entityType = EntityType.Paragraph;
            var bookBlock = $('#book_container .bookBlock[paragraphId=' + entityId + ']');
            var url = bookBlock.attr('externalLink');
            var paragraphText = bookBlock.find('.bookBlock_inner').html();
            initAddHeadBox(paragraphText, entityId, entityType, url, bookTitle + ' (' + bookAuthor + ')', bookAvatarMedium, function () {
                var pContainer = $('#book_container .bookBlock[paragraphId=' + entityId + ']');
                reloadParagraph(pContainer);
                reloadBookCommentsCount();
            });
        });
    }

    var getCommentatorsList = function (Ids) {
        var settingDummy = jQuery.ajaxSettings.traditional;
        jQuery.ajaxSettings.traditional = true;
        var UserIds = self.getUserIds();
        postData = { EntityIds: Ids, take: 1, UserIds: UserIds };
        $.ajax({
            'url': _entityListCommentatorsUrl,
            'type': 'POST',
            'dataType': 'json',
            'data': postData,
            'success': getCommentatorsListSuccess,
            'error': function () { },
            'beforeSubmit': function () {
                //показываем крутилку
            },
            'complete': function () {

            }
        });
        jQuery.ajaxSettings.traditional = settingDummy;
    }

    var getCommentatorsListSuccess = function (data) {
        for (var i in data) {
            var singleList = data[i];
            var info = self.CommentInfosList['' + singleList.EntityId];
            info.updateCommentatorsSuccess(singleList);
        }
    }

    this.updateCommentsCountList = function (paragraphIds) {
        var settingDummy = jQuery.ajaxSettings.traditional;
        jQuery.ajaxSettings.traditional = true;
        var UserIds = self.getUserIds();
        var postData = { EntityIds: paragraphIds, UserIds: UserIds };
        $.ajax({
            'url': _commentCountListUrl,
            'type': 'POST',
            'data': postData,
            'dataType': 'json',
            'success': updateCommentsCountListSuccess,
            'error': function () { },
            'beforeSubmit': function () {
                //показываем крутилку
            },
            'complete': function () {

            }
        });
        jQuery.ajaxSettings.traditional = settingDummy;
    };

    var updateCommentsCountListSuccess = function (data) {
        for (var i in data) {
            var singleCount = data[i];
            var info = self.CommentInfosList['' + singleCount.Id];
            info.updateCommentsCountSuccess(singleCount);
        }

        var hasComments = [];
        for (var i in data) {
            if (data[i].count > 0) { hasComments.push(data[i].Id); }
        }
        if (hasComments.length > 0) {
            getCommentatorsList(hasComments);
        }
    };

    var initFootNotes = function (notes) {
        notes.children(':not(.footNoteTitle)').hide();
        notes.find('.footNoteTitle').click(function (e) {
            var content = $(this).closest('.bookParagraphFootNote').children(':not(.footNoteTitle)');
            content.toggle();
            //var parent = $(this).closest('.bookParagraphFootNote').find(':not(.footNoteTitle)').toggle();
            e.stopPropagation();
        });
    }
    var initFootNoteLinks = function(links) {
        links.click(function (e) {
            var noteId = $(this).attr('rel');
            var noteTitle = $('[noteId="' + noteId + '"]').click();
            e.stopPropagation();
        });
    }
    this.reloadParagraphs = function (pContainers) {
        pContainers.each(function () {
            reloadParagraph($(this));
        });
    }

    var reloadParagraph = function (pContainer) {
        var classes = pContainer.attr('class');
        var url = pContainer.attr('reloadLink');
        $.ajax({
            'url': url,
            'type': 'GET',
            'dataType': 'html',
            'success': function (data) {
                var element = $(data);
                element.attr('class', classes);
                pContainer.replaceWith(element);
                self.initParagraphs(element);
            },
            'error': function (jqXHR, textStatus, errorThrown) { alert(ResourceStrings.OopsError); }
        });
    }

    var updateCommentInfos = function () {
        var EntityIds = [];
        for (var i in self.CommentInfosList) {
            EntityIds.push(self.CommentInfosList[i].paragraphId);
        }
        self.updateCommentsCountList(EntityIds);
    }

    this.getUserIds = function () {
        return self.dropdown.getUserIds();
    };
    var acivateParagraph = function(pContainer) {
        self.deactivateAllParagraphs();
        pContainer.addClass('activeParagraph');
    }

    this.deactivateAllParagraphs = function() {
        $('.bookBlock.activeParagraph').removeClass('activeParagraph');
    }

    var initContainerClick = function(containers) {
        containers.click(function () {
            var bookBlock = $(this).closest('.bookBlock');
            //var bookBlock = $(this);
            if (bookBlock.hasClass('activeParagraph')) {
                bookBlock.removeClass('activeParagraph');
                closeRightFrame($('.rightFrameOuter:visible'));
            } else {
                var readCommentLink = bookBlock.find('.readCommentsLink');
                if (readCommentLink.length > 0) {
                    $(readCommentLink[0]).click();
                } else {
                    var addCommentLink = bookBlock.find('.addCommentLink');
                    if (addCommentLink.length > 0) {
                        $(addCommentLink[0]).click();
                    }
                }
            }
        });
    }

    var initNavigateUpButton = function() {
        var skip = parseInt($('#book_container .bookBlock:first').attr('orderNumber')) - 1;
        var pageSize = parseInt($('#book_container').attr('pageSize'));
        var take = pageSize;
        if (skip < pageSize) {
            take = skip;
            skip = 0;
        } else {
            skip = skip - pageSize;
        }
        $('#navigateUp').attr('skip', skip);
        $('#navigateUp').attr('take', take);
    }
    var initNavigateDownButton = function() {
        var skip = parseInt($('#book_container .bookBlock:last').attr('orderNumber'));
        $('#navigateDown').attr('skip', skip);
    }
    $(function () {
        self.dropdown.loaded(function () {
            updateCommentInfos();
            self.commentatorsLoaded();
        });
        self.dropdown.changed(function () {
            updateCommentInfos();
            self.commentatorsChanged();
        });
        initNavigateUpButton();
        initNavigateDownButton();
        $('#navigateUp')[0].success = function (data) {
            var firstOnPage = $('#book_container .bookBlock:first');
            var newElements = $(data).insertBefore(firstOnPage).filter('.bookBlock');
            self.initParagraphs(newElements);
            window.scrollBy(0, firstOnPage.offset().top - bookHeaderHeight - siteHeaderHeight - 15);
            scrollTop = $(window).scrollTop();
            //alert(scrollTop);
            if (newElements.filter('.bookBlock[orderNumber="1"]').length > 0) {
                $('#navigateUp').hide();
            } else {
                initNavigateUpButton();
            }

            var bookBlocks = $('#book_container .bookBlock');
            if (bookBlocks.length > 100) {
                var blocksToDelete = bookBlocks.filter('.bookBlock:gt(50)');
                blocksToDelete.next('.commentsModule').remove();
                blocksToDelete.remove();
                if (!$('#navigateDown').is(':visible')) {
                    $('#navigateUp').show();
                }
                initNavigateDownButton();
            }
        };
        $('#navigateDown')[0].success = function (data) {
            var lastOnPage = $('#book_container .bookBlock:last');
            var newElements = $(data).insertAfter(lastOnPage).filter('.bookBlock');
            self.initParagraphs(newElements);
            if (newElements.length > 0) {
                initNavigateDownButton();
            } else {
                $('#navigateDown').hide();
            }

            var bookBlocks = $('#book_container .bookBlock');
            if (bookBlocks.length > 100) {
                var blocksToDelete = bookBlocks.filter('.bookBlock:lt(50)');
                blocksToDelete.next('.commentsModule').remove();
                blocksToDelete.remove();
                var offset = lastOnPage.offset().top + lastOnPage.height() - $(window).height() + 73;
                if (!$('#navigateUp').is(':visible')) {
                    offset = offset + 30;
                    $('#navigateUp').show();
                }
                initNavigateUpButton();
                window.scroll(0, offset);
                scrollTop = $(window).scrollTop();
            }
        };

        $('#navigateUp, #navigateDown').click(function () {
            var button = this;
            if ($(this).hasClass('ButtonDeactivated')) return;
            $(this)
            //.removeClass('navigationButton')
            .addClass('navigationButtonPressed').addClass('ButtonDeactivated');
            var url = $(this).attr('navigationUrl');
            var skip = $(this).attr('skip');
            if (url.indexOf("?") > -1) { url += "&skip=" + skip; }
            else { url += "?skip=" + skip; }
            var take = $(this).attr('take');
            url += "&take=" + take;
            $.ajax({
                'url': url,
                'type': 'GET',
                'dataType': 'html',
                'success': function (data) {
                    $(button).removeClass('navigationButtonPressed').addClass('navigationButton').removeClass('ButtonDeactivated');
                    button.success(data)
                },
                'error': function (jqXHR, textStatus, errorThrown) {
                    $(button).removeClass('navigationButtonPressed').addClass('navigationButton').removeClass('ButtonDeactivated');
                    alert(ResourceStrings.SorryErrorHappened);
                },
                'beforeSubmit': function () {
                    //показываем крутилку
                },
                'complete': function () {

                }
            });
        });

        if ($('.bookBlock[orderNumber="1"]').length == 0) {
            $('#navigateUp').show();
        }
        $('#navigateDown').show();
        self.dropdown.getCommentators(_commentatorsUrl);
    });
}

BookPage.prototype = new Observable();
BookPage.prototype.constructor = BookPage;

BookPage.prototype.commentatorsLoaded = function (func) {
    if (!func) this.dispatch({ type: "commentatorsLoaded", object: this });
    else if (typeof func == "function") {
        this.addObserver("commentatorsLoaded", func);
    }
};

BookPage.prototype.commentatorsChanged = function (func) {
    if (!func) this.dispatch({ type: "commentatorsChanged", object: this });
    else if (typeof func == "function") {
        this.addObserver("commentatorsChanged", func);
    }
};