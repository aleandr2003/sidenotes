/**
 *
 * jExpandFilter plugin based on JQuery. Filter the description field by typing in the query.
 * Expand or collapse all description fields with one mouse click or do it individuel.
 * This plugin can be used free of charge. Use it for commercial sites or for your private 
 * projects.
 *
 * Author: Dariusz Borowski
 * Date: 2011/09/23
 **/
(function ($) {
    $.ajaxSetup({ cache: false });

    var block = false;
    var rightIcon, downIcon;

    // settings
    var settings = {
        rightIcon: 'src/img/arrow_right_expand.png',
        downIcon: 'src/img/arrow_down_expand.png'
    };


    // filter results based on query 
    function filter(selector, query) {
        query = $.trim(query); // trim white space  
        query = query.replace(/ /gi, '|'); // add OR for regex query

        $(selector).each(function () {
            if (($(this).text().search(new RegExp(query, "i")) < 0)) {
                $(this).hide().removeClass('visible');
                $(this).prev().find('.expand').css('background', rightIcon);
            } else {
                $(this).parent().find('.expand').css('background', downIcon);
                $(this).show().addClass('visible');
            }
        });

    }

    // set styles on arrows
    function init(el) {
        $(el).css('cursor', 'pointer');
        $(el).css('float', 'left');
        $(el).css('width', '16px');
        $(el).css('height', '16px');
        $(el).css('margin-right', '7px');
        $(el).css('background', downIcon);

        $(el).each(function () {
            var descriptionDiv = $(this).parent().next();
            var isHidden = readCookie($(this).attr('id') + "_IsHidden");
            // change the icon based on its status
            if (isHidden == "false") {
                $(descriptionDiv).show();
                $(this).css('background', downIcon);
            } else {
                $(descriptionDiv).hide();
                $(this).css('background', rightIcon);
            }
        }
        );
    }

    var methods = {

        // function for sliding a description field underneath of the title tag
        toggle: function (options) {

            if (options) {
                $.extend(settings, options);
            }

            rightIcon = 'url\(' + settings.rightIcon + '\) transparent left center no-repeat';
            downIcon = 'url\(' + settings.downIcon + '\) transparent left center no-repeat';
            init($(this));

            // method toggles description field every time it's clicked
            return this.each(function () {
                $(this).mousedown(function () {
                    var descriptionDiv = $(this).parent().next();
                    // change the icon based on its status
                    var isHidden = "";
                    if ($(descriptionDiv).is(":hidden")) {
                        $(this).css('background', downIcon);
                        isHidden = "false";
                    } else {
                        $(this).css('background', rightIcon);
                        isHidden = "true";
                    }
                    createCookie($(this).attr('id') + "_IsHidden", isHidden, 30);
                    $(descriptionDiv).slideToggle("fast");
                });
            });
        },
        // function for showing and hiding all description fields
        showHideAll: function () {

            $(this).mousedown(function () {
                $('.collapsibleContent').each(function (index) {
                    var value = $(this).css('display');
                    if (block) {
                        $(this).css('display', 'block');
                    } else {
                        $(this).css('display', 'none');
                    }
                });

                if (block) {
                    block = false;
                    $('.expand').each(function (index) {
                        $(this).css('background', downIcon);
                    });
                } else {
                    block = true;
                    $('.expand').each(function (index) {
                        $(this).css('background', rightIcon);
                    });
                }
            });
        },
        // function for filtering the description output based on the query 
        filterText: function () {
            $(this).keyup(function (event) {

                //if esc is pressed or nothing is entered  
                if (event.keyCode == 27) {
                    //if esc is pressed we want to clear the value of search box 
                    $(this).val('');
                    filter('.collapsibleContent', $(this).val());
                }

                //if there is text, lets filter
                else {
                    filter('.collapsibleContent', $(this).val());
                }
            });
        }

    };

    $.fn.jExpandFilter = function (method, options) {

        if (methods[method]) {
            return methods[method].apply(this, Array.prototype.slice.call(arguments, 1));
        } else if (typeof method === 'object' || !method) {
            return methods.init.apply(this, arguments);
            alert('ALERT: <' + method + '> is not a method jQuery.jExpandFilter call');
        } else {
            alert('ALERT: Method <' + method + '> does not exist on jQuery.jExpandFilter plugin');
        }
    };
})(jQuery);