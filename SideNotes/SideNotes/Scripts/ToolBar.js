var toolbar_shown = false;
var toolbar_moving = false;
var toolbar_width = 200;
var toolbar_left = -150;
if (navigator.userAgent.match(/iPad/) && screen.width <= 1024) {
    var toolbar_disabled = true;
}

function show_toolbar() {
    if (toolbar_shown || toolbar_disabled) return;
    toolbar_shown = true;
    if (toolbar_moving) return;
    toolbar_moving = true;
    $('#toolbar').animate({ left: 0 }, 400, 'swing', function () {
        $('.toolbar_outer').show();
        toolbar_moving = false;
        if (!toolbar_shown) {
            toolbar_shown = true;
            hide_toolbar();
        }
    });
}
function hide_toolbar() {
    if (!toolbar_shown) return;
    toolbar_shown = false;
    if (toolbar_moving) return;
    toolbar_moving = true;
    $('#toolbar').animate({ left: toolbar_left }, 400, 'swing', function () {
        toolbar_moving = false;
        if (toolbar_shown) {
            toolbar_shown = false;
            show_toolbar();
        }
    });
}

function setup_toolbar_width() {
    toolbar_width = $('.toolbar_outer').width();
    //if (toolbar_width < 200) { toolbar_width = 200; }
    //$('.toolbar_outer').css({ width: '' + toolbar_width + 'px' });
}

function disable_toolbar() {
    toolbar_disabled = true;
    hide_toolbar();
}

var toolbar_height;
$(document).ready(function () {
    setup_toolbar_width();
    toolbar_height = $('#toolbar').height();
    toolbar_left = 20 - toolbar_width;
    $('#toolbar').css('left', toolbar_left);
});