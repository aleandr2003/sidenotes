function initWorkAreaPosition() {
    var leftOffset = ($(window).width() - $('#main').outerWidth()) / 2;
    //alert(leftOffset);
    $('#main').css('left', leftOffset);
}

$(function () {
    //initWorkAreaPosition();
    //$(window).resize(initWorkAreaPosition);
})

function downloadPage(button, success) {
    var page = parseInt($(button).attr('page'));
    page = page + 1;
    $(button).attr('page', page);
    var url = $(button).attr('href');
    var postData = { Page: page };
    $.ajax({
        'url': url,
        'type': 'POST',
        'dataType': 'html',
        'data': postData,
        'success': success,
        'error': errorFunc,
        'beforeSubmit': function () {
            //показываем крутилку
        },
        'complete': function () {

        }
    });
}

function fillUserBlock(userBlock, ProfileUrl, AvatarUrl, Name) {
    var avatarLink = userBlock.find('.userblock_avatar_link');
    var nameLink = userBlock.find('.userblock_name');
    var avatarImg = userBlock.find('.userblock_avatar');
    avatarLink.attr('href', ProfileUrl);
    nameLink.attr('href', ProfileUrl);
    nameLink.html(Name);
    avatarImg.attr('src', AvatarUrl);
}