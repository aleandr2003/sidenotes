$(function () {
    $('.submitInUnclosableBox').click(function () {
        $('#unclosablebox_shadow').hide();
        $('.lightbox').hide();
        return true;
    });
});

function showUnclosableBox(boxId) {
    positionCenterBox(boxId);
    $('#unclosablebox_shadow').show();
    $('#' + boxId).show();
}

function positionCenterBox(boxId) {
    var width = $(window).width();
    var height = $(window).height();
    var boxWidth = $('#' + boxId).outerWidth();
    var boxHeight = $('#' + boxId).outerHeight();

    $('.lightbox').hide();
    var leftMargin = -Math.round((width - boxWidth) / 2);
    var top = Math.round((height - boxHeight) / 2);
    
    if (top + boxHeight > height) {
        top = height - boxHeight;
    }

    $('#' + boxId).css({
        'marginLeft': leftMargin + 'px',
        'top': top + 'px',
        'left': '50%'
    });
    if (width / 2 < leftMargin + boxWidth) {
        $('#' + boxId).css({
            'marginLeft': '',
            'left': '',
            'right': '0'
        });
    }
}
