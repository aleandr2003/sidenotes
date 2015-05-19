function createCookie(name, value, days) {
    if (days) {
        var date = new Date();
        date.setTime(date.getTime() + (days * 24 * 60 * 60 * 1000));
        var expires = "; expires=" + date.toGMTString();
    }
    else var expires = "";
    document.cookie = name + "=" + value + expires + "; path=/";
}

function readCookie(name) {
    var nameEQ = name + "=";
    var ca = document.cookie.split(';');
    for (var i = 0; i < ca.length; i++) {
        var c = ca[i];
        while (c.charAt(0) == ' ') c = c.substring(1, c.length);
        if (c.indexOf(nameEQ) == 0) return c.substring(nameEQ.length, c.length);
    }
    return null;
}

function eraseCookie(name) {
    createCookie(name, "", -1);
}

function shuffle(array) {
    for (var i = array.length - 1; i > 0; i--) {
        var num = Math.floor(Math.random() * (i + 1));
        var d = array[num];
        array[num] = array[i];
        array[i] = d;
    }
    return array;
}

function setParameter(url, param, value) {
    if (url.indexOf("?") < 0) {
        return url + "?" + param + "=" + value;
    } else {
        var path = url.substring(0, url.indexOf("?"));
        var query = url.substring(url.indexOf("?") + 1);
        if (query.indexOf(param + '=') < 0) {
            return url + '&' + param + '=' + value;
        } else {
            var params = query.split('&');
            for (var i in params) {
                var item = params[i];
                if (item.indexOf(param + '=') == 0) {
                    params[i] = param + '=' + value;
                }
            }
            return path + '?' + params.join('&');
        }
    }
}