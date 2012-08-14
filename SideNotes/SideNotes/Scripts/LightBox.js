$(document).ready(function () {
    initLightBoxLinks($('.showLightBox_link'));

    $('#lightbox_shadow').click(function () {
        $('.lightbox').hide();
        $(this).hide();
        return false;
    });

    $('.submitInLightBox').click(function () {
        $('#lightbox_shadow').click();
        return true;
    });


    $('.lightbox_close').click(function () {
        $('#lightbox_shadow').click();
        return false;
    });
});
function positionSwitchBox(box, link) {
    //var width = $('#lightbox_shadow').outerWidth();
        
    var parent = $(link).parents('.lightbox');
    $('#' + box).css({
        'marginLeft': parent.css('marginLeft'),
        'top': parent.css('top'),
        'left': parent.css('left'),
        'right': parent.css('right')
    })
    parent.hide();
}

function positionBox(box, link) {
    //var width = $('#lightbox_shadow').outerWidth();
    var width = $(window).width();
    var height = $(window).height();
    var position = $(link).offset();
    var scrolled = $(document).scrollTop();
 
    $('.lightbox').hide();
    var leftMargin = -Math.round(width / 2 - position.left);
    var top = Math.round(position.top - scrolled);
    var boxHeight = $('#' + box).outerHeight();
    if (top + boxHeight > height) {
        top = height - boxHeight - 20;
    }

    $('#' + box).css({
        'marginLeft': leftMargin + 'px',
        'top': top + 'px',
        'left': '50%'
    });
    var outerWidth = $('#' + box).outerWidth();
    if (width / 2 < leftMargin + outerWidth) {
        $('#' + box).css({
            'marginLeft': (width / 2 - outerWidth - 20) + 'px'
        });
    }
}

function positionBoxAbsolute(box, link) {
    //var width = $('#lightbox_shadow').outerWidth()
    var width = $(window).width();
    var height = $(window).height();
    var position = $(link).offset();
    var scrolled = $(document).scrollTop();

    $('.lightbox').hide();
    var leftMargin = -Math.round(width / 2 - position.left);
    var top = Math.round(scrolled);
    var boxHeight = $('#' + box).outerHeight();
    if (boxHeight < height) {
        top += (height - boxHeight) / 2;
    }

    $('#' + box).css({
        'marginLeft': leftMargin + 'px',
        'top': top + 'px',
        'left': '50%'
    });
    var outerWidth = $('#' + box).outerWidth();
    if (width / 2 < leftMargin + $('#' + box).outerWidth()) {
        $('#' + box).css({
            'marginLeft': (width / 2 - outerWidth - 20) + 'px'
        });
    }
}


function initLightBoxLinks(links) {
    links.attr('href', 'javascript:void(0)');
    links.click(function () {/// <reference path="MicrosoftAjax.js" />

        $('#lightbox_shadow').show();
        var box = $(this).attr('rel');
        if ($(this).hasClass('positionBoxAbsolute') == true) {
            positionBoxAbsolute(box, this);
        } else if ($(this).hasClass('switchLightboxLink') == true) {
            positionSwitchBox(box, this);
        } else {
            positionBox(box, this);
        }
        $('#' + box).show();
        return false;
    });
}




