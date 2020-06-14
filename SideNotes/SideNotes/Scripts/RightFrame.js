function hideRightFrames() {
    $('.rightFrameOuter').hide();
    $('.rightFrameOuter').each(function () {
        createCookie($(this).attr('id') + "_Opened", 'false', 30);
    });
}
function errorReplyRight(jqXHR, textStatus, errorThrown) {
    alert(ResourceStrings.SorryErrorHappened);
}

function initRightFrame(frameId, url) {
    hideRightFrames();
    var frame = $('#' + frameId)[0];
    $(frame).attr('url', url);
    var content = $(frame).find('.rightFrameContent');
    content.html('');
    content.css('background-image', 'url(/Content/img/spinner.gif)');
    $.ajax({
        'url': url,
        'type': 'GET',
        'dataType': 'html',
        'success': contentLoadedClosureRight($(frame)),
        'error': errorReplyRight,
        'beforeSubmit': function () {
            //показываем крутилку
        },
        'complete': function () {

        }
    });
   
    $(frame).show();
}

function reloadRightFrame(frameId) {
    var url = $('#' + frameId).attr('url');
    if (url.length > 0) {
        initRightFrame(frameId, url);
    }
}

function contentLoadedClosureRight(frame) {
    return function contentLoaded(data) {
        createCookie($(frame).attr('id') + "_Opened", 'true', 30);
        //frame.children('.rightFrameContent').html(data);
        var element = frame.children('.rightFrameContent')[0];
        element.style.backgroundImage = "none";
        element.innerHTML = data;
        var ajaxForms = $(element).find('.ajaxForm');
        if (ajaxForms.length > 0) {
            initAjaxForms(ajaxForms);
            initSubmitButtons($(element).find('.submitButton'));
        }

        var commentForms = $(element).find('.addHeadCommentForm');
        if (commentForms.length > 0) {
            initCommentForms(commentForms);
        }
        var filterLinks = $(element).find('.commentsFilterLink');
        if (filterLinks.length > 0) {
            initFilterLinks(filterLinks);
        }
        initLightBoxLinks($(element).find('.showLightBox_link'));

        $(element).find('.newCommentTextArea').css('width', '' + (frame.width() - 50) + 'px');
        var scripts = element.getElementsByTagName("script");
        var script;
        for (var i = 0; script = scripts[i]; i++) {
            eval(script.innerHTML);
        }
        var builtInForms = $(element).find('.addBuiltInCommentForm, .deleteBuiltInCommentForm');
        if (builtInForms.length > 0) {
            initBuiltInForms(builtInForms);
        }

        var removeCommentForms = $(element).find('.removeCommentForm');
        if (removeCommentForms.length > 0) {
            fixRemoveCommentForms(removeCommentForms);
        }
        var addCommentForms = $(element).find('.AddNewCommentForm');
        if (addCommentForms.length > 0) {
            fixAddCommentForms(addCommentForms);
        }
        frame.show();
        $('#lightbox_shadow').click();
    }
}

function fixAddCommentForms(forms) {
    forms.each(function () {
        var originalSuccessFunc = this.success || function (data) { };
        var form = this;
        var frame = $(form).closest('.rightFrameOuter')[0];
        this.success = function (data) {
            reloadRightFrame($(frame).attr('Id'));
            var bookPage = new BookPage();
            bookPage.reloadParagraphs($('.activeParagraph'));
            originalSuccessFunc(data);
        };
    });
}

function fixRemoveCommentForms(forms) {
    forms.each(function () {
        var originalSuccessFunc = this.success;
        this.success = function (data) {
            var bookPage = new BookPage();
            bookPage.reloadParagraphs($('.activeParagraph'));
            originalSuccessFunc(data);
        };
    });
}

function initBuiltInForms(forms) {
    forms.each(function () {
        var originalSuccessFunc = this.success || function (data) { };
        this.success = function (data) {
            var bookPage = new BookPage();
            bookPage.reloadParagraphs($('.activeParagraph'));
            originalSuccessFunc(data);
        };
    });
}

function initFilterLinks(links) {
    links.each(function () {
        var url = $(this).attr('href');
        $(this).attr('href', 'javascript:void(0)');
        var frameId = $(this).closest('.rightFrameOuter').attr('id');
        $(this).click(function () {
            initRightFrame(frameId, url);
        });
    });
}

function initCommentForms(forms) {
    //forms.addClass('ajaxForm');
    //var submits = forms.find('[type="submit"]');
    //submits.addClass('submitButton');
    forms.each(function () {
        var form = this;
        //form.dataType = 'html';
        var frame = $(form).closest('.rightFrameOuter')[0];
        //var proccessContentFunc = contentLoadedClosureRight($(frame));
        form.success = function (data) {
            var bookPage = new BookPage();
            bookPage.reloadParagraphs($('.activeParagraph'));
            // proccessContentFunc(data);
            if (data != null && data.RedirectUrl != null) {
                initRightFrame($(frame).attr('Id'), data.RedirectUrl);
            } else {
                reloadRightFrame($(frame).attr('Id'));
            }
        };
    });
    
    //initAjaxForms(forms);
    //initSubmitButtons(submits);
}

function initRightFrameHeight() {
    var frameHeight = $(window).height() - $('#LayoutHeader').height() - 30;
    var contentHeight = frameHeight - $('.rightFrameHeader').height() - 15;
    $('.rightFrameOuter').height(frameHeight);
    $('.rightFrameContent').height(contentHeight);
}
function initRightFrameWidth() {
    var leftOffset = $('#main').outerWidth() + $('#main').position().left;
    var frameWidth = $(window).width() - leftOffset - 25;
    var frameLeft = leftOffset + 15;
    var contentWidth = frameWidth -10;
    $('.rightFrameOuter').css('left', frameLeft);
    $('.rightFrameOuter').width(frameWidth);
    $('.rightFrameContent').width(contentWidth);
}
//function restoreRightFrames() {
//    $('.rightFrameOuter').each(function () {
//        var isOpened = readCookie($(this).attr('id') + "_Opened");
//        if (isOpened == 'true') {
//            initRightFrame($(this).attr('id'));
//        }
//    });
//}

function closeRightFrame(frame) {
    var bookPage = new BookPage();
    bookPage.deactivateAllParagraphs();
    createCookie($(frame).attr('id') + "_Opened", 'false', 30);
    frame.hide();
}

$(document).ready(function () {
    $('.rightFrameClose').click(function () {
        var frame = $(this).closest('.rightFrameOuter');
        closeRightFrame(frame);
        return false;
    });
    //initRightFrameHeight();
    //initRightFrameWidth();
    //restoreRightFrames();
});