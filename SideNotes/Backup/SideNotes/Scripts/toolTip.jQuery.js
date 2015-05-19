(function ($) {

    $.fn.toolTip = function (options) {
        var settings = $.extend({
            'width': '18em',
            'class': ''
        }, options);
        return this.each(function () {
            var element = $(this);
            var text = element.attr('toolTip');
            if (text == undefined) {
                text = element.html();
            }

            var d1 = new Date(70, 0, 0);
            var d2 = new Date();
            var Id = 'sn' + (d2 - d1);
            element.attr('toolTipRel', Id);

            var position = element.offset();

            var tip = $('<div></div>').addClass('jQueryToolTip').addClass(settings['class']).attr('id', Id).html(text)
                            .insertAfter(element).hide().css({
                                'position': 'absolute',
                                'left': position.left,
                                'top': position.top,
                                'width':settings['width']
                            });
                        var toolTipfadeOutTimeout;
                        tip.hover(
                            function () { clearTimeout(toolTipfadeOutTimeout); },
                            function () {
                                var box = tip;
                                toolTipfadeOutTimeout = setTimeout(function () { $(box).fadeOut(); }, 500);
                            });

                        element.hover(
                            function () {
                                tip.show();
                                clearTimeout(toolTipfadeOutTimeout);
                            },
                            function () {
                                var box = tip;
                                toolTipfadeOutTimeout = setTimeout(function () { $(box).fadeOut(); }, 500);
                            });
        });

    };



})(jQuery); 