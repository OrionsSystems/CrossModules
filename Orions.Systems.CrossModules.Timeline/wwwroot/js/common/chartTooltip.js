var previousPoint = null;
$.fn.UseTooltip = function () {
    function showTooltip(x, y, contents) {
        var tooltip = $('<div id="tooltip">' + contents + '</div>');
        tooltip.css({
            position: 'absolute',
            display: 'none',
            top: y + 20,
            left: x + 10,
            border: '2px solid #4572A7',
            padding: '2px',
            size: '10',
            'border-radius': '6px 6px 6px 6px',
            'background-color': '#fff',
            opacity: 0.80,
            'z-index': '9999',
        });
        tooltip.appendTo("body").fadeIn(200);
        var l = tooltip.position().left;
        var w = tooltip.width();
        var dw = $(window).width() - 50;
        if (l + w > dw) {
            var offset = (l + w) - dw;
            tooltip.css({ 'left': l - offset + 'px' });
        }
    }
    $(this).bind("plothover", function (event, pos, item) {
        if (item) {
            if (previousPoint !== item.dataIndex) {
                previousPoint = item.dataIndex;
                $("#tooltip").remove();
                var x = item.datapoint[0];
                var y = item.datapoint[1];
                try {
                    showTooltip(item.pageX, item.pageY,
                        "<strong>" + y + "</strong> <br/> (" + item.series.data[x][0] + ")");
                } catch (ex) {
                    showTooltip(item.pageX, item.pageY,
                        "<strong>" + y + "</strong>");
                }
            }
        }
        else {
            $("#tooltip").remove();
            previousPoint = null;
        }
    });
};