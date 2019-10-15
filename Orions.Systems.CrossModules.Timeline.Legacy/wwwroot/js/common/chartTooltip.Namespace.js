var previousPoint = null;
$.fn.NamespaceStatsTooltip = function () {
    function showTooltip(x, y, contents) {
        var tooltip = $('<div id="tooltip">' + contents + '</div>');
        tooltip.css({
            position: 'absolute',
            display: 'none',
            top: y + 20,
            left: x + 10,
            border: '2px solid #4572A7',
            padding: '10px',
            size: '10',
            'border-radius': '6px',
            'background-color': '#fff',
            opacity: 0.90,
            'z-index': '9999'
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

    $(this).bind("plotclick", function (event, pos, item) {
        if (item) {
            if (previousPoint !== item.dataIndex) {
                previousPoint = item.dataIndex;

                $("#tooltip").remove();

                var x = item.datapoint[0];
                var number = item.datapoint[1];
                var label = item.series.data[x][0];
                var sendData = label + "<br /><b>" + number + "</b>";
                showTooltip(item.pageX, item.pageY, sendData);
            }
        } else {
            $("#tooltip").remove();
            previousPoint = null;
        }
    });
};
