(function ($) {

    var methods = {
        init: function (options) {
            var settings = $.extend({
                'limit': 200,
                'breakLine': false,
                'expandText': '...',
                'collapseText': '<<<'
            }, options);
            return this.each(function () {
                
                var element = $(this);
                var text = element.html();
                element.empty();
                var chunk = $('<span class="shortenPluginChunk"></span>');
                var whole = $('<span class="shortenPluginWhole"></span>');
                var more = $('<a href="javascript:void(0)" class="shortenPluginMore">' + settings.expandText + '</a>');
                var less = $('<a href="javascript:void(0)" class="shortenPluginLess">' + settings.collapseText + '</a>');
                element.append(chunk, whole);
                if (settings.breakLine) {
                    element.append($('<br/>'));
                }
                element.append(more, less);
                element.attr('limit', settings.limit);
                whole.hide();
                more.hide();
                less.hide();
                more.click(function () {
                    $(this).siblings('.shortenPluginChunk').hide();
                    $(this).siblings('.shortenPluginWhole').show();
                    $(this).hide();
                    $(this).siblings('.shortenPluginLess').show();
                });
                less.click(function () {
                    $(this).siblings('.shortenPluginWhole').hide();
                    $(this).siblings('.shortenPluginChunk').show();
                    $(this).hide();
                    $(this).siblings('.shortenPluginMore').show();
                });

                element.shorten('update', text);
            });
        },
        update: function (content) {
            var element = this;
            var limit = element.attr('limit');

            var chunk = element.find('.shortenPluginChunk');
            var whole = element.find('.shortenPluginWhole');
            var more = element.find('.shortenPluginMore');
            var less = element.find('.shortenPluginLess');
            chunk.show();
            whole.hide();
            more.hide();
            less.hide();
            if (content.length > limit) {
                chunk.html(content.substr(0, limit));
                whole.html(content);
                more.show();
            } else {
                chunk.html(content);
            }
        }
    };

    $.fn.shorten = function (method) {
        if (methods[method]) {
            return methods[method].apply(this, Array.prototype.slice.call(arguments, 1));
        } else if (typeof method === 'object' || !method) {
            return methods.init.apply(this, arguments);
        } else {
            $.error('There is no method ' + method + ' on jQuery.shorten');
        }

    };



})(jQuery); 