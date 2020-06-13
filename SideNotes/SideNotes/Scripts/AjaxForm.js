function baseSuccessClosure(form) {
    return function baseSuccess(data) {
        var classes = form.attr('class').split(/\s+/);
        if ($.inArray('errorable', classes) > -1 && data != null && data.ErrorMessage != null) {
            form.children(".errorMessage").html(data.ErrorMessage);
        }
        if ($.inArray('redirectable', classes) > -1 && data != null && data.RedirectUrl != null) {
            $('#lightbox_shadow').click();
            window.location = data.RedirectUrl;
        }
        if ($.inArray('closable', classes) > -1) {
            $('#lightbox_shadow').click();
        }
        if ($.inArray('reloadable', classes) > -1) {
            window.location.reload();
        }
    }
}
function errorFunc(jqXHR, textStatus, errorThrown) {
    alert("Sorry, something failed");
}

function validateFields(descr) {
    var fields = descr.closest('form').find('.field-validation-error');
    for (var i = 0; i < fields.length; i++)
    //если рядом с каким-то из полей ASP.NET MVC сгенерировал сообщение об ошибке
        if (fields[i].childNodes.length > 0)
            return false;

    var requiredFields = descr.closest('form').find('.requiredField');
    for (var i = 0; i < requiredFields.length; i++)
    //если какое-то из необходимых полей пустое
        if (requiredFields[i].value == "")
            return false;
    return true;
}

function initSubmitButtons(buttons) {
    buttons.click(function () {
        if (validateFields($(this)) == false)
            return false;
        return true;
    });
}
function initAjaxForms(forms) {
    forms.submit(function () {
        var data = $(this).serialize();
        var successFunc = $(this)[0].success || baseSuccessClosure($(this));
        var dataType = $(this)[0].dataType || 'json';
        var url = $(this).attr('action');
        if (url.indexOf("?") > -1) { url += "&json=True"; }
        else { url += "?json=True"; }
        $('#lightbox_shadow').show();
        $.ajax({
            'url': url,
            'type': 'POST',
            'dataType': dataType,
            'data': data,
            'success': successFunc,
            'error': errorFunc,
            'beforeSubmit': function () {
                //показываем крутилку
            },
            'complete': function () {

            }
        });
        return false;

    });
}

$(document).ready(function () {
    $(document).ajaxStart(function (e) {
        //		alert('ПОказать крутилку');
        $('input[type=submit]', this).attr('disabled', 'disabled');
    });

    $(document).ajaxStop(function () {
        //		alert('Убрать крутилку');
        $('input[type=submit]', this).removeAttr('disabled');
    });

    $(document).ajaxError(function () {
        //		alert('Ошибка!!');
    });

    initSubmitButtons($('.submitButton'));
    initAjaxForms($('.ajaxForm'));
});