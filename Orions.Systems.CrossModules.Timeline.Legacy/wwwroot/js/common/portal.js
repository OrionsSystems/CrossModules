if (location.href.indexOf("#") != -1) {
    // Your code in here accessing the string like this
    location.href.substr(location.href.indexOf("#"));
}

if (window.location.hash != "") {
    $('a[href="' + window.location.hash + '"]').click()
}

function ajaxCompleteGlobal(e) {
    try {
        var target = $(e.target);
        var el = $(e.target.activeElement);
        var isButton = el.hasClass('btn') || el.hasClass("btn");

        // just a precautionary check
        if (!isButton) {
            if (target != undefined && target != null) {
                isButton = target.hasClass("btn") || target.hasClass("btn")
                if (!isButton) return;
                el = target;
            } else {
                return;
            }
        }

        el.button('reset');

    } catch (ex) {
        console.log(ex);
    }
};

function ajaxStartGlobal(e) {
    try {
        var target = $(e.target);
        var el = $(e.target.activeElement);
        var isButton = el.hasClass('btn') || el.hasClass("btn");

        // just a precautionary check
        if (!isButton) {
            if (target != undefined && target != null) {
                isButton = target.hasClass("btn") || target.hasClass("btn")
                if (!isButton) return;
                el = target;
            } else {
                return;
            }
        }

        var text = el.data("loading-text");
        if (text == undefined || typeof text == 'undefined' || text == false) {
            el.attr("data-loading-text", "<i class='fa fa-circle-o-notch fa-spin'></i> loading")
        }
        el.button('loading'); // or whatever else you want to do

    } catch (ex) {
        console.log(ex);
    }
};

$(document).ajaxStart(function (e) {
    ajaxStartGlobal(e);
});

$(document).ajaxComplete(function (e) {
    ajaxCompleteGlobal(e);
});

$(document).ajaxSuccess(function (e) {
    ajaxCompleteGlobal(e);
});

$(document).ajaxStop(function (e) {
    ajaxCompleteGlobal(e);
});

$(document).ready(function () {
    Init();
    function Init() {
        $(".settings :submit").each(function () {
            var button = $(this);
            if (button.hasClass("no-disable")) return;

            button = $(this).attr("disabled", true);
            button.addClass("btn-default");
        });
        $("button, a").each(function (e) {
            staticModal($(this))
        });
    }

    function staticModal(item) {
        if (item.data("toggle") == "modal" &&
            (item.data("backdrop") == "" || item.data("backdrop") == undefined)) {
            item.data("backdrop", "static");
        }
    }

    $('form').submit(function (e) {
        try {
            var $el = $(e.target);

            var btn = $el.find("button[type=submit]");

            // just a precautionary check
            if (btn == null) {
                btn = $el.find("input[type=submit]");
                if (btn == null) {
                    return;
                }
            }
            var validator = $(this).validate(); // obtain validator

            if (validator == null) return;

            if (validator.valid()) {

                var text = btn.data("loading-text");
                if (text == undefined || typeof text == 'undefined' || text == false) {
                    btn.attr("data-loading-text", "<i class='fa fa-circle-o-notch fa-spin'></i> loading")
                }
                btn.button('loading'); // or whatever else you want to do
            }

        } catch (ex) {
            console.log(ex);
        }
    });

    $('.modal').on('shown.bs.modal', function (event) {
        try {
            $(this).backdrop = 'static';
            $(this).keyboard = false;
            var $el = $(event.relatedTarget);
            $el.button('reset');
        } catch (exception) {

        }
    });

    $(".settings input").on("change paste keyup", function () {
        var form = $(this).closest("form");
        button = form.find(":submit");
        button.attr("disabled", false);
        button.removeClass("btn-default");
    });

    $(".settings").on("change", "[type=checkbox]", function () {
        var form = $(this).closest("form");
        button = form.find(":submit");
        button.attr("disabled", false);
        button.removeClass("btn-default");
    });

    $(".settings textarea").on("change paste keyup", function () {
        var form = $(this).closest("form");
        button = form.find(":submit");
        button.attr("disabled", false);
        button.removeClass("btn-default");
    });

    $(".confirm .delete").on("click", function (e) {
        if (confirm("Do you realy want to delete this item?")) {
            return true;
        }
        return false;
    })

    function ConfirmDelete() {
        if (confirm("Do you realy want to delete this item?")) {
            return true;
        }
        return false;
    };

    $.fn.serializeToJson = function () {
        var o = {};
        var a = this.serializeArray();
        $.each(a, function () {
            if (o[this.name]) {
                if (!o[this.name].push) {
                    o[this.name] = [o[this.name]];
                }
                o[this.name].push(this.value || '');
            } else {
                o[this.name] = this.value || '';
            }
        });
        return o;
    };

    $.fn.pinToDashboard = function (requestUrl) {
        var element = $(this).data("element");
        var data = {};
        data["element"] = JSON.stringify(element);
        $.ajax({
            type: 'POST',
            url: requestUrl,
            data: data,
            context: $(this),
            success: function (result, message, ctx) {
                toastr.success(result);
            }
        });
    };

    $.fn.refreshNotificaitons = function (requestUrl) {
        var alerts = $(this).find("#alerts-list");
        alerts.html(spinnerCircle());
        $.ajax({
            type: 'GET',
            url: requestUrl,
            success: function (result, message, ctx) {
                alerts.html(result);
            },
            error: function (result, message, ctx) {
                alerts.html("");
            }
        });
    };

    $.fn.markAllAsReadNotificaitons = function (requestUrl) {
        var alerts = $(this).find("#alerts-list");
        var alert = $(this).find("#alerts-count");
        return $.ajax({
            type: 'POST',
            url: requestUrl,
            success: function (result, message, ctx) {
                alerts.html("");
                if (!alert.hasClass("hidden")) {
                    alert.addClass("hidden");
                    alert.html("");
                };
            },
            error: function (result, message, ctx) {
            }
        });
    };

    $.fn.countNotificaitons = function (requestUrl) {
        $.ajax({
            type: 'GET',
            url: requestUrl,
            success: function (result, message, ctx) {
                var container = $("#notifications");
                var alert = container.find("#alerts-count");
                if ((result == "" || result == "0" || result == 0)) {
                    if (!alert.hasClass("hidden")) alert.addClass("hidden");
                } else {
                    alert.removeClass("hidden");
                    alert.html(result);
                }
            },
            error: function (result, message, ctx) {
                var container = $("#notifications");
                var alert = container.find("#alerts-count");
                if (!alert.hasClass("hidden")) alert.addClass("hidden");
                alert.html("");
            }
        });
    };
});

function Spinner() {
    return "<div class='sk-spinner sk-spinner-circle'><div class='sk-circle1 sk-circle'></div><div class='sk-circle2 sk-circle'></div><div class='sk-circle3 sk-circle'></div><div class='sk-circle4 sk-circle'></div><div class='sk-circle5 sk-circle'></div><div class='sk-circle6 sk-circle'></div><div class='sk-circle7 sk-circle'></div><div class='sk-circle8 sk-circle'></div><div class='sk-circle9 sk-circle'></div><div class='sk-circle10 sk-circle'></div><div class='sk-circle11 sk-circle'></div><div class='sk-circle12 sk-circle'></div></div>";
}

function spinnerWave() {
    return "<div class='sk-spinner sk-spinner-wave'><div class='sk-rect1'></div><div class='sk-rect2'></div><div class='sk-rect3'></div><div class='sk-rect4'></div><div class='sk-rect5'></div></div>";
}

function spinnerCircle() {
    return "<div class='sk-spinner sk-spinner-circle'><div class='sk-circle1 sk-circle'></div><div class='sk-circle2 sk-circle'></div><div class='sk-circle3 sk-circle'></div><div class='sk-circle4 sk-circle'></div><div class='sk-circle5 sk-circle'></div><div class='sk-circle6 sk-circle'></div><div class='sk-circle7 sk-circle'></div><div class='sk-circle8 sk-circle'></div><div class='sk-circle9 sk-circle'></div><div class='sk-circle10 sk-circle'></div><div class='sk-circle11 sk-circle'></div><div class='sk-circle12 sk-circle'></div></div>";
}