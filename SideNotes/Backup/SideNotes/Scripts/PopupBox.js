$(function () {
    
    $('.popupbox_close').click(function () {
        $(this).closest('.popupbox').hide();
        return false;
    });

    var fadeOutTimeout;
    $('.popupbox').hover(
    function () { clearTimeout(fadeOutTimeout); },
    function () {
        var box = this;
        fadeOutTimeout = setTimeout(function () { $(box).fadeOut(); }, 500);
    });
});

function positionPopupBox(box, link) {
    //var width = $('#lightbox_shadow').outerWidth();
    var width = $(window).width();
    var height = $(window).height();
    var position = $(link).offset();
    var scrolled = $(document).scrollTop();
    
    $('.lightbox').hide();
    var leftMargin = -Math.round(width / 2 - position.left);
    var top = Math.round(position.top - scrolled) + 15;
    var boxHeight = $('#' + box).outerHeight();
    if (top + boxHeight > height) {
        top = height - boxHeight;
    }

    $('#' + box).css({
        'marginLeft': leftMargin + 'px',
        'top': top + 'px',
        'left': '50%'
    });
    if (width / 2 < leftMargin + $('#' + box).outerWidth()) {
        $('#' + box).css({
            'marginLeft': '',
            'left': '',
            'right': '0'
        });
    }
}
function errorReplyPopup(jqXHR, textStatus, errorThrown) {
    alert("Извините, не получилось посмотреть, кто комментировал");
}

function loadPopupBox(box, link, url) {
    $.ajax({
        'url': url,
        'type': 'GET',
        'dataType': 'html',
        'success': function (data) {
            positionPopupBox(box, link);
            $('#' + box + ' .popup_content').html(data);
            $('#' + box).show();
        },
        'error': errorReplyPopup,
        'beforeSubmit': function () {
            //показываем крутилку
        },
        'complete': function () {

        }
    });
    return false;

}


function initPopupBoxLinks(links) {
    var timeout;
    links.hover(function () {
        var box = $(this).attr('rel');
        var popupUrl = $(this).attr('popupUrl');
        var link = this;
        timeout = setTimeout(function () { loadPopupBox(box, link, popupUrl); }, 500);
    },
    function () {
        clearTimeout(timeout);
    });
}