function global_getUrlQueryParameters(sParam) {
    var sPageURL = decodeURIComponent(window.location.search.substring(1)),
        sURLVariables = sPageURL.split('&'),
        sParameterName,
        i;
    var values = [];
    for (i = 0; i < sURLVariables.length; i++) {
        sParameterName = sURLVariables[i].split('=');

        if (sParameterName[0] === sParam) {
            values.push(sParameterName[1] === undefined ? true : sParameterName[1]);
        }
    }
    return values;
}

function global_getUrlQueryParameter(sParam) {
    values = global_getUrlQueryParameters(sParam);
    if (values.length == 0) return undefined;
    return values[0];
}

function global_getNodeIdsfromMultiselect(selectorId) {
    return global_getIdsfromMultiselect(selectorId);
}

function global_getIdsfromMultiselect(selectorId) {
    var ids = []
    var items = $("#" + selectorId).data("kendoMultiSelect").dataItems();
    if (items == null || items.length == 0) {
        return ids;
    }

    for (var i = 0; i < items.length; i++) {
        ids.push(items[i].Value)
    }

    return ids;
}

function global_getGuidId() {
    function s4() {
        return Math.floor((1 + Math.random()) * 0x10000)
            .toString(16)
            .substring(1);
    }
    return s4() + s4() + s4() + s4() + s4() + s4() + s4() + s4();
}

function global_getResponsiveWindowSize() {
    var h = window.outerHeight;
    var w = window.outerWidth;    
    if (w >= 1500) {
        w = 1500;
    } else if (w < 1600 && w >= 1200) {
        w = 1100;
    } else if (w < 1200 && w >= 992) {
        w = 900;
    } else if (w < 992 && w >= 768) {
        w = 700;
    } else if (w < 768) {
        w = w * 0.85;
    };
    h = h * 0.80;
    var size = { Height: h, Width: w };
    return size;
}

//use global
function getNodeIdsfromMultiselect(selectorId) {
    return global_getNodeIdsfromMultiselect(selectorId)
}

function spinner() {
    return "<div class='k-loading-mask' style='width:80%; height:80%;'>" +
        "<span class='k-loading-text'>Loading...</span><div class='k-loading-image'>" +
        "<div class='k-loading-color'></div></div>" +
        "</div>";
}

function spinnerFullPage() {
    return "<div class='k-loading-mask' style='width:100%; height:100%;'>" +
        "<span class='k-loading-text'>Loading...</span><div class='k-loading-image'>" +
        "<div class='k-loading-color'></div></div>" +
        "</div>";
}

function spinnerBounce() {
    return '<div class="sk-spinner sk-spinner-three-bounce"><div class="sk-bounce1"></div><div class="sk-bounce2"></div><div class="sk-bounce3"></div></div>';
}

function isGuid(value) {
    var regex = /[a-f0-9]{8}(?:-[a-f0-9]{4}){3}-[a-f0-9]{12}/i;
    var match = regex.exec(value);
    return match != null;
}

function setGlobalSpinner() {
    $("#process-spinner").html(spinner());
}
function removeGlobalSpinner() {
    $("#process-spinner").html("");
}

$.fn.selectRow = function (sender, type) {
    var grid = $(this).data(type);
    var row = $(sender).closest("tr");
    grid.select(row);
    return this;
}

$.fn.getSelectedItem = function (sender, type, showMessage) {
    var grid = $(this).data(type);
    var selectedItem = grid.dataItem(grid.select());
    if (selectedItem == null) {
        if (showMessage == null || showMessage == undefined || showMessage != false)
            toastr.error("You have to select a row.");
        return null;
    }
    return selectedItem
}
