//testing git
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
    
    //var shareBlockContainer = $('#addHeadCommentBox .shareBlock').empty();
    //var shareBlock = new ShareBlock(shareBlockContainer, content, url, title, picture);
    //shareBlock.init();
    

    var form = lightBox.find('#addHeadCommentBoxForm')[0];
    form.success = function () {
        $('#addHeadCommentBoxForm textarea').text('');
        $('#lightbox_shadow').click();
        successFunc();
    };
}



