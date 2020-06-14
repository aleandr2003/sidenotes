function CommentInfo(container, sandbox, paragraphId, feedUrl, commentatorsUrl) {
    Observable.call(this);
    this.paragraphId = paragraphId;

    var self = this;
    var _container = container;
    var _sandbox = sandbox;
    var _feedUrl = feedUrl;
    var _commentatorsUrl = commentatorsUrl;
    var _counter = $('<div class="commentCounter" style="display:none;"></div>');
    var _counterLink = $('<a href="' + setParameter(_feedUrl, 'paragraphId', self.paragraphId) + '" class="commentCounter_counter allCommentsLink"  title = "' + ResourceStrings.CommentsTotalCount+ '"></a>');
    var _countInt = 0;

    this.Display = function () {
        _container.empty();
        $('<span class="floatDummy">&nbsp;</span>').appendTo(_container);
        _counter.appendTo(_container);
        _counterLink.appendTo(_counter);
        //self.update();
        _counterLink.click(function (e) {
            e.preventDefault();
            self.linkPressed();
        });
    };

    this.update = function () {
        updateCommentsCount();
    };

    var updateCommentators = function () {
        postData = { EntityId: self.paragraphId, take: 1 };
        $.ajax({
            'url': _commentatorsUrl,
            'type': 'POST',
            'dataType': 'json',
            'data': postData,
            'success': self.updateCommentatorsSuccess,
            'error': function () { },
            'beforeSubmit': function () {
                //показываем крутилку
            },
            'complete': function () {

            }
        });
    }

    this.updateCommentatorsSuccess = function (data) {
        _container.find('.userblock_avatar_link').remove();
        for (var i in data.userList) {
            var user = data.userList[i];
            var userLink = $('<a href="' + user.ProfileUrl + '" class="userblock_avatar_link"></a>').appendTo(_container);
            var avatar = $('<img class="userblock_avatar" src="' + user.AvatarUrl + '" title="' + user.Name + '">').appendTo(userLink);
        }
    }

    var updateCommentsCount = function () {
        var settingDummy = jQuery.ajaxSettings.traditional;
        jQuery.ajaxSettings.traditional = true;
        var UserIds = _sandbox.getUserIds();
        var postData = { Id: self.paragraphId, UserIds: UserIds };
        $.ajax({
            'url': commentCountUrl,
            'type': 'POST',
            'data': postData,
            'dataType': 'json',
            'success': function (data) { self.updateCommentsCountSuccess(data); if (data.count > 0) { updateCommentators(); } },
            'error': function () { },
            'beforeSubmit': function () {
                //показываем крутилку
            },
            'complete': function () {

            }
        });
        jQuery.ajaxSettings.traditional = settingDummy;
    };

    this.updateCommentsCountSuccess = function (data) {
        _counterLink.html(data.count);
        _counterLink.attr('title', ResourceStrings.CommentsTotalCount2.format(data.count));
        _container.find('.userblock_avatar_link').remove();
        if (!data.count || data.count == 0) {
            _counter.hide();
        } else {
            _counter.show();
        }
    };
}

CommentInfo.prototype = new Observable();
CommentInfo.prototype.constructor = CommentInfo;

CommentInfo.prototype.linkPressed = function (func) {
    if (!func) this.dispatch({ type: "linkPressed", object: this });
    else if (typeof func == "function") {
        this.addObserver("linkPressed", func);
    }
};