function hideLeftFrames() {
    $('.leftFrameOuter').hide();
    $('.leftFrameOuter').each(function () {
        createCookie($(this).attr('id') + "_Opened", 'false', 30);
    });
}
function errorReply(jqXHR, textStatus, errorThrown) {
    alert("Извините, произошла ошибка");
}

function initFrame(frameId) {
    hideLeftFrames();
    var frame = $('#' + frameId)[0];
    var url = $(frame).attr('link');
    var content = $(frame).find('.leftFrameContent');
    content.html('');
    content.css('background-image', 'url(/Content/img/spinner.gif)');
    $.ajax({
        'url': url,
        'type': 'GET',
        'dataType': 'html',
        'success': contentLoadedClosure($(frame)),
        'error': errorReply,
        'beforeSubmit': function () {
            //показываем крутилку
        },
        'complete': function () {

        }
    });
    
    $(frame).show();
}

function contentLoadedClosure(frame) {
    return function contentLoaded(data) {
        createCookie($(frame).attr('id') + "_Opened", 'true', 30);
        //frame.children('.leftFrameContent').html(data);
        var element = frame.children('.leftFrameContent')[0];
        element.style.backgroundImage = "none";
        element.innerHTML = data;
        var ajaxForms = $(element).find('.ajaxForm');
        if (ajaxForms.length > 0) {
            initAjaxForms(ajaxForms);
            initSubmitButtons($(element).find('.submitButton'));
        }
        var scripts = element.getElementsByTagName("script");
        var script;
        for (var i = 0; script = scripts[i]; i++) {
            eval(script.innerHTML);
        }
        frame.show();
    }
}
function initLeftFrameHeight() {
    var frameHeight = $(window).height() - $('#LayoutHeader').height() - 30;
    var contentHeight = frameHeight - $('.leftFrameHeader').height() - 15;
    $('.leftFrameOuter').height(frameHeight);
    $('.leftFrameContent').height(contentHeight);
}
function initLeftFrameWidth() {
    var leftOffset = 15;
    $('.leftFrameOuter').css('left', leftOffset + 'px');
    var frameWidth = $('#main').position().left - leftOffset;
    var contentWidth = frameWidth - 10;
    $('.leftFrameOuter').width(frameWidth);
    $('.leftFrameContent').width(contentWidth);
}
function restoreFrames() {
    $('.leftFrameOuter').each(function () {
        var isOpened = readCookie($(this).attr('id') + "_Opened");
        if (isOpened == 'true') {
            initFrame($(this).attr('id'));
        }
    });
}

$(document).ready(function () {
    $('.leftFrameClose').click(function () {
        var frame = $(this).closest('.leftFrameOuter');
        createCookie($(frame).attr('id') + "_Opened", 'false', 30);
        frame.hide();
        return false;
    });
    //initLeftFrameHeight();
    //initLeftFrameWidth();
    restoreFrames();
});