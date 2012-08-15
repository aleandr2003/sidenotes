function ShareBlock(container, content, url, title, picture) {
    Observable.call(this);
    var self = this;
    var _container = container;
    var _content = $(content).text();
    var _url = url;
    var _title = title;
    var _picture = picture;

    var tweetLimit = 110;
    var facebookLimit = 490;
    var vkontakteLimit = 340;
    var LJLimit = 340;

    var setUpTwitterShare = function (hashtag) {
        var content = _content;
        if (content != undefined && content.length > tweetLimit) {
            content = content.substr(0, tweetLimit) + '...';
        }
        var button = $('<a href="http://twitter.com/share" class="twitter-share-button" data-count="none">Tweet</a>')
        .attr('data-url', _url).attr('data-text', content);
        if (hashtag != undefined && hashtag.length > 0) {
            button.attr('data-hashtags', hashtag);
        }
        var div = $('<div class="shareButtonContainer"></div>').appendTo(_container);
        button.appendTo(div);
        twttr.widgets.load();
    }

    var setUpFacebookShare = function () {
        var content = _content;
        if (content != undefined && content.length > facebookLimit) {
            content = content.substr(0, facebookLimit) + '...';
        }
        var dialogUrl = 'http://www.facebook.com/dialog/feed?app_id=' + facebook_app_Id +
        '&link=' + _url +
        '&picture=' + _picture +
        '&name=' + _title +
        '&caption=' + _url +
        '&description=' + content +
        '&redirect_uri=' + siteRootUrl +
        '&display=popup';
        var button = $('<a href="' + dialogUrl + '" class="facebook-share-button" target="_blank"><span>Рассказать</span></a>');
        button.click(function (e) {
            var url = $(this).attr('href');
            e.preventDefault();
            MyWindow = window.open(url, 'MyWindow', 'toolbar=no,location=no,directories=no,status=no, menubar=no,scrollbars=no,resizable=no,width=580,height=400');
            return false;
        });
        var div = $('<div class="shareButtonContainer"></div>').appendTo(_container);
        button.appendTo(div);
    }

    var setUpVkontakteShare = function () {
        var content = _content;
        if (content != undefined && content.length > vkontakteLimit) {
            content = content.substr(0, vkontakteLimit) + '...';
        }
        var contentSettings = {
            url: _url,
            title: _title,
            description: content,
            image: _picture,
            noparse: true
        };

        var button = VK.Share.button(contentSettings, { type: 'round_nocount', text: 'Рассказать' });
        var div = $('<div class="shareButtonContainer"></div>').appendTo(_container);
        $(button).attr('class', 'vkontakte-share-button').appendTo(div);
    }

    var setUpLjShare = function () {
        var content = _content;
        if (content != undefined && content.length > LJLimit) {
            content = content.substr(0, LJLimit) + '...';
        }
        var ljSnippet = encodeURIComponent('<table cellspacing="0" style="width: 525px; font-family: arial; border: solid 1px #000; background-color: #fff;"><tr valign="top"><td style="padding: 13px; width:150px;"><img src="' + _picture + '" width="123" height="93" alt="" style="border: none;" /></td><td style="padding: 9px 9px 12px 1px"><div><a href="' + _url + '" style="text-decoration: none; color: #7B7B7B; font-size: 16px; line-height: 16px; font-weight:bold">' + _title + '</a></div><div style="margin-top:8px"><a href="' + _url + '" style="text-decoration: none; color: #000000;"><span style="font-size: 15px; line-height: 16px; color: #7B7B7B; font-style:italic">' + content + '</span></a></div><div style="margin-top:8px"><a href="http://sidenotes.ru" style="color: #000"><img src="' + siteLogoUrl + '"/><span style="color: #000; font-size: 11px; font-weight:bold">Sidenotes.ru</span></a></div></td></tr></table>');
        var linkUrl = 'http://www.livejournal.com/update.bml?event=' + ljSnippet;
        var button = $('<a href="' + linkUrl + '" class="lj-share-button" target="_blank"><span>Написать</span></a>');
        var div = $('<div class="shareButtonContainer"></div>').appendTo(_container);
        button.appendTo(div);
    }
    this.init = function () {
        setUpTwitterShare('');
        setUpFacebookShare();
        setUpLjShare();
        setUpVkontakteShare();
    }
}

ShareBlock.prototype = new Observable();
ShareBlock.prototype.constructor = ShareBlock;
