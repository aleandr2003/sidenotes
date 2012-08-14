//testing git
var tweetLimit = 110;
var facebookLimit = 490;
var vkontakteLimit = 340;
var LJLimit = 340;
function initAddHeadBox(content, entityId, entityType, url, title, picture, successFunc) {
    var lightBox = $('#addHeadCommentBox');
    if (entityId != undefined && entityType != undefined) {
        lightBox.find('.inputEntityId').val(entityId);
        lightBox.find('.inputEntityType').val(entityType);
    }
    if (content != undefined) {
        var bookBlock = lightBox.find('.bookBlock');
        bookBlock.html(content);
        bookBlock.shorten({ limit: 300 });
    }
    var text = $(content).text();
    var shareBlock = $('#addHeadCommentBox .shareBlock').empty();
    setUpTwitterShare(text, url, '', shareBlock);
    setUpFacebookShare(text, url, title, picture, shareBlock);
    setUpLjShare(text, url, title, picture, shareBlock);
    setUpVkontakteShare(text, url, title, picture, shareBlock);
    

    var form = lightBox.find('#addHeadCommentBoxForm')[0];
    form.success = function () {
        $('#addHeadCommentBoxForm textarea').text('');
        $('#lightbox_shadow').click();
        successFunc();
    };
}

function setUpTwitterShare(content, url, hashtag, block) {
    if (content != undefined && content.length > tweetLimit) {
        content = content.substr(0, tweetLimit) + '...';
    }
    var button = $('<a href="http://twitter.com/share" class="twitter-share-button" data-count="none">Tweet</a>')
        .attr('data-url', url).attr('data-text', content);
    if (hashtag != undefined && hashtag.length > 0) {
        button.attr('data-hashtags', hashtag);
    }
    button.appendTo(block);
    twttr.widgets.load();
}

function setUpFacebookShare(content, url, title, picture, block) {
    if (content != undefined && content.length > facebookLimit) {
        content = content.substr(0, facebookLimit) + '...';
    }
    var dialogUrl = 'http://www.facebook.com/dialog/feed?app_id=' + facebook_app_Id +
        '&link=' + url +
        '&picture=' + picture +
        '&name=' + title +
        '&caption=' + url +
        '&description=' + content +
        '&redirect_uri=' + siteRootUrl+
        '&display=popup';
    var button = $('<a href="'+dialogUrl+'" class="facebook-share-button" target="_blank"><span>Рассказать</span></a>');
    button.click(function (e) {
        var url = $(this).attr('href');
        e.preventDefault();
        MyWindow = window.open(url, 'MyWindow', 'toolbar=no,location=no,directories=no,status=no, menubar=no,scrollbars=no,resizable=no,width=580,height=400');
        return false;
    });
    
    button.appendTo(block);
}

function setUpVkontakteShare(content, url, title, picture, block) {
    if (content != undefined && content.length > vkontakteLimit) {
        content = content.substr(0, vkontakteLimit) + '...';
    }
    var contentSettings = {
        url: url,
        title: title,
        description: content,
        image: picture,
        noparse: true
    };

    var button = VK.Share.button(contentSettings, { type: 'round_nocount', text: 'Рассказать' });
    $(button).attr('class', 'vkontakte-share-button').appendTo(block);
}

function setUpLjShare(content, url, title, picture, block) {
    if (content != undefined && content.length > LJLimit) {
        content = content.substr(0, LJLimit) + '...';
    }
    var ljSnippet = encodeURIComponent('<table cellspacing="0" style="width: 525px; font-family: arial; border: solid 1px #000; background-color: #fff;"><tr valign="top"><td style="padding: 13px; width:150px;"><img src="' + picture + '" width="123" height="93" alt="" style="border: none;" /></td><td style="padding: 9px 9px 12px 1px"><div><a href="' + url + '" style="text-decoration: none; color: #7B7B7B; font-size: 16px; line-height: 16px; font-weight:bold">' + title + '</a></div><div style="margin-top:8px"><a href="' + url + '" style="text-decoration: none; color: #000000;"><span style="font-size: 15px; line-height: 16px; color: #7B7B7B; font-style:italic">' + content + '</span></a></div><div style="margin-top:8px"><a href="http://sidenotes.ru" style="color: #000"><img src="' + siteLogoUrl + '"/><span style="color: #000; font-size: 11px; font-weight:bold">Sidenotes.ru</span></a></div></td></tr></table>');
    var linkUrl = 'http://www.livejournal.com/update.bml?event=' + ljSnippet;
    var button = $('<a href="' + linkUrl + '" class="lj-share-button" target="_blank"><span>Написать</span></a>');

    button.appendTo(block);
}