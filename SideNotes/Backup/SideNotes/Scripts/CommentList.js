function CommentList(container, sandbox, commentCountUrl, commentListUrl, repliesCountUrl) {
    Observable.call(this);
    var commentLengthLimit = 200;

    var self = this;
    var _container = container;
    var _sandbox = sandbox;
    var _commentCountUrl = commentCountUrl;
    var _commentListUrl = commentListUrl;
    var _repliesCountUrl = repliesCountUrl;

    var _firstComment;
    var _lastComment;
    this.paragraphId;
    var _paragraphText;
    var _url;

    var _firstCommentBlock;
    var _commentList;
    var _extraCommentsButton;
    var _extraCommentsLink;
    var _hideCommentsLink;

    var _lastCommentBlock;

    var _commentBlockTemplate = $('.commentBlockTemplate');
    var _addCommentForm;

    this.init = function (firstComment, lastComment, paragraphId, paragraphText, url) {
        _firstComment = firstComment;
        _lastComment = lastComment;
        self.paragraphId = paragraphId;
        _paragraphText = paragraphText;
        _url = url;
    };

    this.initNoComments = function (paragraphId, paragraphText, url) {
        self.paragraphId = paragraphId;
        _paragraphText = paragraphText;
        _url = url;
    };



    this.Display = function () {
        _container.empty();
        _firstCommentBlock = $('<div class="commentBlockFirst"></div>').appendTo(_container);

        _commentList = $('<div class="commentBlockList" style="display:none;"></div>').appendTo(_container);
        _extraCommentsButton = $('<div class="extraComments" style="display:none;"></div>').appendTo(_container);
        _extraCommentsLink = $('<a href="javascript:void(0)" class="extraCommentsLink"></a>').appendTo(_extraCommentsButton);
        _hideCommentsLink = $('<a href="javascript:void(0)" class="hideCommentsLink" style="display:none;">Скрыть</a>').appendTo(_extraCommentsButton);
        _lastCommentBlock = $('<div class="commentBlockLast"></div>').appendTo(_container);
        _addCommentForm = createNewCommentForm().appendTo(_container).show();

        var commentBlock = getCommentBlock().appendTo(_firstCommentBlock);
        fillCommentBlock(commentBlock, _firstComment, self.paragraphId);

        if ((_firstComment && _lastComment) && _firstComment.Id != _lastComment.Id) {
            commentBlock = getCommentBlock().appendTo(_lastCommentBlock);
            fillCommentBlock(commentBlock, _lastComment, self.paragraphId);
        }
        _extraCommentsLink.click(this.showCommentsList);
        _hideCommentsLink.click(this.hideCommentsList);
        self.getCommentsCount();
    };

    this.showCommentsList = function () {
        self.getComments();
        _extraCommentsLink.hide();
        _hideCommentsLink.show();
    }

    this.hideCommentsList = function () {
        _commentList.hide();
        _commentList.empty();
        _firstCommentBlock.show();
        _lastCommentBlock.show();
        _extraCommentsLink.show();
        _hideCommentsLink.hide();
    }

    this.getComments = function() {
        var settingDummy = jQuery.ajaxSettings.traditional;
        jQuery.ajaxSettings.traditional = true;
        var UserIds = _sandbox.getUserIds();
        var postData = { Id: self.paragraphId, UserIds: UserIds };
        $.ajax({
            'url': _commentListUrl,
            'type': 'POST',
            'dataType': 'json',
            'data': postData,
            'success': getCommentsSuccess,
            'error': function () { },
            'beforeSubmit': function () {
                //показываем крутилку
            },
            'complete': function () {

            }
        });
        jQuery.ajaxSettings.traditional = settingDummy;
    }
    var getCommentsSuccess = function (data) {
        _commentList.empty();
        _firstCommentBlock.empty();
        _lastCommentBlock.empty();
        for (var i in data.commentList) {
            var comment = data.commentList[i];

            var commentBlock = getCommentBlock().appendTo(_commentList);
            fillCommentBlock(commentBlock, comment, data.paragraphId);
        }
        if (data.commentList && data.commentList.length > 0) {
            var comment = data.commentList[0];
            var commentBlock = getCommentBlock().appendTo(_firstCommentBlock);
            fillCommentBlock(commentBlock, comment, data.paragraphId);
        }
        if (data.commentList && data.commentList.length > 1) {
            var comment = data.commentList[data.commentList.length - 1];
            var commentBlock = getCommentBlock().appendTo(_lastCommentBlock);
            fillCommentBlock(commentBlock, comment, data.paragraphId);
        }

        _commentList.show();
        _firstCommentBlock.hide();
        _lastCommentBlock.hide();
    };
    var createNewCommentForm = function () {
        var form = $('.addHeadCommentFormTemplate').clone().removeClass('addHeadCommentFormTemplate').addClass('addHeadCommentForm');
        form.find('input[name=entityId]').val(self.paragraphId);
        form.find('input[name=entityType]').val(EntityType.Paragraph);
        var formName = 'addHeadCommentForm_' + self.paragraphId;
        form.attr('name', formName);
        if (!IsAuthenticated) {
            form.find('input[type=submit]').click(function (event) {
                event.preventDefault();
                $(this).hide();
                $(this).closest('.addHeadCommentForm').find('.SNLoginBox').show();
            });
        }
        form.find('.newCommentTextArea').focus(function () {
            $('.newCommentTextAreaWide').removeClass('newCommentTextAreaWide').addClass('newCommentTextArea');
            $(this).removeClass('newCommentTextArea').addClass('newCommentTextAreaWide');
        });

        form.find('.commentBoxBottomLine .socialNetworkLogo').click(function (event) {
            event.preventDefault();
            //var form = $(this).closest('.addHeadCommentForm');
            form.attr('action', $(this).attr('action'));

            var callbackInput = $('<input type="hidden" name="callbackUri"/>');
            callbackInput.attr('value', window.location);
            callbackInput.appendTo(form);
            //form.submit();
            document.forms[formName].submit();
        });

        form[0].success = function (data) {
            if (data != null && data.ErrorMessage != null) {
                $('#lightbox_shadow').click();
                alert("Ой. Ошибочка: " + data.ErrorMessage);
            } else {
                $(form).find('[name=commentText]').val('');
                self.showCommentsList();
                self.getCommentsCount();
                reloadBookCommentsCount();
                $('#lightbox_shadow').click();
                self.commentAdded();
            }
        };

        initAjaxForms(form);
        initSubmitButtons(form.find('input[type=submit]'));
        
        return form;
    }
    var getCommentBlock = function () {
        return _commentBlockTemplate.clone().removeClass('commentBlockTemplate').addClass('commentBlock');
    };

    var fillCommentBlock = function (commentBlock, Comment, paragraphId) {
        if (!Comment || !commentBlock) return;
        
        commentBlock.attr('headCommentId', Comment.Id);
        var commentContainer = commentBlock.find('.comment_container');
        
        commentContainer.find('.commentDateCreated').html(Comment.date);
        commentContainer.find('.commentText').html(Comment.Text).shorten({ limit: commentLengthLimit });
        
        /*fillCommentText(commentContainer, Comment.Text);*/
        fillUserBlock(commentContainer.find('.userblock'), Comment.ProfileUrl, Comment.AvatarUrl, Comment.AuthorName);
        
        var repliesCounter = commentBlock.find('.repliesCounter_counter');
        var repliesLink = commentBlock.find('.threadLink');
        repliesLink.attr('href', Comment.RepliesLink);
        if (Comment.ChildCommentsCount > 0) {
            repliesCounter.html(Comment.ChildCommentsCount);
        } else {
            commentBlock.find('.threadLink').hide();
        }
        //initReplyLinks(commentBlock.find('.replyLink'));
        //initAddHeadLinks(commentBlock.find('.replyLink'));
        initAjaxForms(commentBlock.find('.ajaxForm'));
        initDeleteForm(commentBlock.find('.removeCommentForm'), Comment, paragraphId);
        
        commentBlock.show();
    };

    var initAddHeadLinks = function (links) {
        initLightBoxLinks(links);
        links.click(function (e) {
            e.preventDefault();
            initAddHeadBox(_paragraphText, self.paragraphId, EntityType.Paragraph,
                _url, bookTitle + ' (' + bookAuthor + ')', bookAvatarMedium);
        });
    };

    var initDeleteForm = function (form, comment, paragraphId) {
        form.hide();
        if (currentUserId == comment.AuthorId) {
            form.find('input[name=Id]').val(comment.Id);
            form.show();
        } else {
            form.hide();
        }
        form[0].success = function () {
            hideComment(comment.Id);
            self.showCommentsList();
            self.getCommentsCount();
            reloadBookCommentsCount();
            $('#lightbox_shadow').click();
            self.commentDeleted();
        };
    };
    var hideComment = function (Id) {
        $('.commentBlock[headCommentId=' + Id + ']').remove();
    };

    var initReplyLinks = function (links) {
        initLightBoxLinks(links);
        links.click(function () {
            var Id = $(this).parents('.commentBlock').attr('headCommentId');
            var box = $('#replyBox');
            box.find('input[name=headCommentId]').val(Id);
            box.find('input[name=entityId]').val(self.paragraphId);
        });
    };

    this.getRepliesCount = function (Id) {
        var postData = { Id: Id };
        $.ajax({
            'url': _repliesCountUrl,
            'type': 'POST',
            'data': postData,
            'dataType': 'json',
            'success': getRepliesCountSuccess,
            'error': function () { },
            'beforeSubmit': function () {
                //показываем крутилку
            },
            'complete': function () {

            }
        });
    };

    var getRepliesCountSuccess = function (data) {
        if (data.count > 0) {
            var commentBlock = $('[headCommentId=' + data.Id + ']');
            commentBlock.find('.repliesCounter_counter').html(data.count).show();
            commentBlock.find('.threadLink').show();
        } else {
            $('[headCommentId=' + data.Id + ']').find('.threadLink').hide();
        }
    };

    this.getCommentsCount = function () {
        var settingDummy = jQuery.ajaxSettings.traditional;
        jQuery.ajaxSettings.traditional = true;
        var UserIds = _sandbox.getUserIds();
        var postData = { Id: self.paragraphId, UserIds: UserIds };
        $.ajax({
            'url': _commentCountUrl,
            'type': 'POST',
            'data': postData,
            'dataType': 'json',
            'success': getCommentsCountSuccess,
            'error': function () { },
            'beforeSubmit': function () {
                //показываем крутилку
            },
            'complete': function () {

            }
        });
        jQuery.ajaxSettings.traditional = settingDummy;
    };

    var getCommentsCountSuccess = function (data) {
        if (data.count > 2) {
            _extraCommentsLink.html('ещё ' + (data.count - 2) + ' ' + getInclinationNominative(data.count - 2));
            _extraCommentsButton.show();
        } else {
            _extraCommentsButton.hide();
        }
    };

    var getInclinationNominative = function (intgr) {
        if (intgr % 10 == 1 && intgr % 100 != 11) {
            return 'комментарий';
        } else if (intgr % 10 < 5 && (intgr % 100 < 11 || intgr % 100 > 14)) {
            return 'комментария';
        } else {
            return 'комментариев';
        }
    };
}

CommentList.prototype = new Observable();
CommentList.prototype.constructor = CommentList;

CommentList.prototype.commentAdded = function (func) {
    if (!func) this.dispatch({ type: "commentAdded", object: this });
    else if (typeof func == "function") {
        this.addObserver("commentAdded", func);
    }
};

CommentList.prototype.commentDeleted = function (func) {
    if (!func) this.dispatch({ type: "commentDeleted", object: this });
    else if (typeof func == "function") {
        this.addObserver("commentDeleted", func);
    }
};