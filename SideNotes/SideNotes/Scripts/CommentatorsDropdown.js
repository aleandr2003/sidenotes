function CommentatorsDropdown() {
    Observable.call(this);
    $('.commentator_container').hide();
    this.commentatorsListLimit = 8;
    var self = this;

    this.initSelectors = function () {
        $('#commentator_type_all').click(function () {
            $('.commentator_type').removeClass('commentator_type_current');
            $(this).addClass('commentator_type_current');
            $('#commentatorType1 li.commentator_item .commentator_checkbox').attr('checked', 'checked');
            self.changed();
        });
        $('#commentator_type_friends').click(function () {
            $('.commentator_type').removeClass('commentator_type_current');
            $(this).addClass('commentator_type_current');
            $('#commentatorType1 li.commentator_item .commentator_checkbox').removeAttr('checked');
            $('#commentatorType1 li.commentator_item[isFriend=true] .commentator_checkbox').attr('checked', 'checked');
            self.changed();
        });
        $('#commentator_type_none').click(function () {
            $('.commentator_type').removeClass('commentator_type_current');
            $(this).addClass('commentator_type_current');
            $('#commentatorType1 li.commentator_item .commentator_checkbox').removeAttr('checked');
            self.changed();
        });
    }

    this.getCommentators = function (url) {
        $.ajax({
            'url': url,
            'type': 'POST',
            'dataType': 'json',
            'success': function (data) { self.getCommentatorsSuccess(data); },
            'error': function () { },
            'beforeSubmit': function () {
                //показываем крутилку
            },
            'complete': function () {

            }
        });
    }
    this.getCommentatorsSuccess = function (data) {
        $('#commentatorType1').empty();
        if (data && data.length > 0) {
            for (var i in data) {
                var commentator = data[i];
                var commentatorItem = $('#commentator_item_template').clone().appendTo('#commentatorType1');
                commentatorItem.removeAttr('Id');
                commentatorItem.attr('UserId', commentator.Id);
                if (commentator.IsFriend) commentatorItem.attr('IsFriend', commentator.IsFriend);
                if (commentator.IsFamous) commentatorItem.attr('IsFamous', commentator.IsFamous);
                var checkbox = commentatorItem.find('.commentator_checkbox');
                var checkBoxId = checkbox.attr('id') + i;
                checkbox.attr('id', checkBoxId);
                var userBlock = commentatorItem.find('div.userblock');
                //userBlock.attr('for', checkBoxId);
                fillUserBlock(userBlock, commentator.ProfileUrl, commentator.AvatarUrl, commentator.Name);
                commentatorItem.show();
            }
            $('.commentator_checkbox').click(function () {
                self.changed();
            });
            $('#commentator_counter_span').html(data.length + ' ' + self.getInclination(data.length));
            self.initSelectors();
            $('.commentator_container').show();
        } else {
            $('.commentator_container').hide();
        }
        self.fillAvatarsLine(data);
        self.loaded();
    }

    this.fillAvatarsLine = function (data) {
        if (data && data.length > 0) {
            var commentators = [];
            for (var i in data) {
                var commentator = data[i];
                if (commentator.IsFriend) {
                    commentators.push(commentator);
                    if (commentators.length >= self.commentatorsListLimit) break;
                }
            }
            if (commentators.length < self.commentatorsListLimit) {
                for (var i in data) {
                    var commentator = data[i];
                    if (commentator.IsFamous) {
                        commentators.push(commentator);
                        if (commentators.length >= self.commentatorsListLimit) break;
                    }
                }
            }
            if (commentators.length < self.commentatorsListLimit) {
                for (var i in data) {
                    var commentator = data[i];
                    if (!commentator.IsFamous && !commentator.IsFriend) {
                        commentators.push(commentator);
                        if (commentators.length >= self.commentatorsListLimit) break;
                    }
                }
            }
            var avatarsLine = $('.commentator_container .avatarsLine');
            for (var i in commentators) {
                var commentator = commentators[i];
                $('<a href="' + commentator.ProfileUrl + '" class="userIcon"><img src="' + commentator.AvatarUrl + '" title="' + commentator.Name + '"/></a>').appendTo(avatarsLine);
            }
        }
    }

    this.getInclination = function (intgr) {
        if (userLanguage === 'ru') {
            if (intgr % 10 == 1 && intgr % 100 != 11) {
                return 'комментатора';
            } else {
                return 'комментаторов';
            }
        } else if (userLanguage === 'en') {
            return intgr === 1 ? 'commentator' : 'commentators';
        }
    }

    var fillUserBlock = function (userBlock, ProfileUrl, AvatarUrl, Name) {
        var avatarLink = userBlock.find('.userblock_avatar_link');
        var nameLink = userBlock.find('.userblock_name');
        var avatarImg = userBlock.find('.userblock_avatar');
        avatarLink.attr('href', ProfileUrl);
        nameLink.attr('href', ProfileUrl);
        nameLink.html(Name);
        avatarImg.attr('src', AvatarUrl);
    }

    this.getUserIds = function () {
        var UserIds = [];
        var selectedItems = $('#commentatorType1 .commentator_checkbox:checked').parents('li.commentator_item');
        selectedItems.each(function () {
            UserIds.push($(this).attr('UserId'));
        });
        return UserIds;
    }
    this.all = function () {
        $('#commentator_type_all').click();
    }

    this.friends = function () {
        $('#commentator_type_friends').click();
    }

    this.none = function () {
        $('#commentator_type_none').click();
    }




}

CommentatorsDropdown.prototype = new Observable();
CommentatorsDropdown.prototype.constructor = CommentatorsDropdown;

CommentatorsDropdown.prototype.loaded = function (func) {
    if (!func) this.dispatch("loaded");
    else if (typeof func == "function") {
        this.addObserver("loaded", func);
    }
};

CommentatorsDropdown.prototype.changed = function (func) {
    if (!func) this.dispatch("changed");
    else if (typeof func == "function") {
        this.addObserver("changed", func);
    }
};

