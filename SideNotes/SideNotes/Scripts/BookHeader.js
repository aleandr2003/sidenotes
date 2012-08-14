var bookTitleLengthLimit = 22;
var bookAuthorListLengthLimit = 40;

function reloadBookCommentsCount() {
    var url = $('#commentCounter_container').attr('reloadUrl');
    $.ajax({
        'url': url,
        'type': 'POST',
        'dataType': 'json',
        'success': function (data) {
            $('#commentCounter_container').find('.commentCounter_counter').html(data.count);
        },
        'error': function () { },
        'beforeSubmit': function () {
            //показываем крутилку
        },
        'complete': function () {

        }
    });

}

function fixBookTitle() {
    var element = $('.book_name');
    var link = element.find('a');
    var text = link.html();
    if (text.length > bookTitleLengthLimit) {
        element.attr('toolTip', element.html());
        element.toolTip({ 'class': 'bookTitleToolTip' });
        link.html(text.substring(0, bookTitleLengthLimit) + '...');
    }
}
function fixBookAuthors() {
    var element = $('.book_author_list');
    var link = element.find('a');
    var text = link.html();
    if (text.length > bookAuthorListLengthLimit) {
        element.attr('toolTip', element.html());
        element.toolTip({ 'class': 'bookAuthorToolTip' }); 
        link.html(text.substring(0, bookAuthorListLengthLimit) + '...');
    }
}

$(function () {
    fixBookTitle();
    fixBookAuthors();
});