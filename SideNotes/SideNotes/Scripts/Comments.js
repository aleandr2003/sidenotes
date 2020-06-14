function initRemoveCommentForms(forms) {
    forms.each(function () {
        var form = this;
        form.success = function (data) {
            if (data != null && data.ErrorMessage != null) {
                $('#lightbox_shadow').click();
                alert(ResourceStrings.OopsError + ": " + data.ErrorMessage);
            } else {
                var container = $(form).closest('.commentContainer');
                container.find('.commentAuthor').remove();
                container.find('.commentAction').remove();
                container.find('.commentContent').html('(удален)');
                $('#lightbox_shadow').click();
            }
        };
    });
}

function initReplyLinks(links) {
    links.attr('href', 'javascript:void(0)');
    links.click(function () {
        $('#AddNewHeadFormContainer').hide();
        var formContainer = $('.AddNewCommentFormContainer').hide().insertAfter($(this).closest('.commentContainer'));
        var parentCommentId = $(this).attr('commentId');
        if (parentCommentId != undefined) {
            formContainer.find('.inputParentCommentId').val(parentCommentId);
        }
        var headCommentId = $(this).attr('headCommentId');
        if (headCommentId != undefined) {
            formContainer.find('.inputHeadCommentId').val(headCommentId);
        }
        formContainer.show();
    });
}

function initReplyLinksWithBox(links) {
    links.attr('href', 'javascript:void(0)');
    links.click(function () {
        var formContainer = $('#addCommentBoxForm');
        var parentCommentId = $(this).attr('commentId');
        if (parentCommentId != undefined) {
            formContainer.find('.inputParentCommentId').val(parentCommentId);
        }
        var headCommentId = $(this).attr('headCommentId');
        if (headCommentId != undefined) {
            formContainer.find('.inputHeadCommentId').val(headCommentId);
        }
    });
}

function createCancelButton() {
    var submitButton = $('.AddNewCommentForm').find('input[type="submit"]');
    var cancelButton = $('<input type="button" value="' + ResourceStrings.Cancel + '" class="cancelButton"/>').insertAfter(submitButton);
    cancelButton.click(function () {
        $('.AddNewCommentFormContainer').hide();
        $('#AddNewHeadFormContainer').show();
    });
}

function initCommentList() {
    initRemoveCommentForms($('.removeCommentForm'));

    $('.commentContainer:even').addClass('commentContainerAlterStyle');

    $('.AddNewCommentForm').removeClass('redirectable').addClass('reloadable');
    createCancelButton();

    initReplyLinks($('.replyLink'));
}