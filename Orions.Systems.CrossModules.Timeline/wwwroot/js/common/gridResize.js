$.fn.kendoGridAutoResize = function () {
    var targetGrid = $(this);
    var container = targetGrid.parent("div");
    var outsideElements = []
    $(document).find(".grid-external-elements").each(function () {
        outsideElements.push($(this));
    });

    resizeGrids();

    function resizeGrids() {

        var containerHeight = container.height();
        var outsideHeight = 0;
        for (var i = 0; i < outsideElements.length; i++) {
            outsideHeight += outsideElements[i].outerHeight();
        }

        var height = containerHeight - outsideHeight;

        var gridPagerHeight = targetGrid.find(".k-grid-pager").outerHeight();
        var gridHeaderHeight = targetGrid.find(".k-grid-header").outerHeight();
        var gridToolbarHeight = targetGrid.find(".k-grid-toolbar").outerHeight();
        var gridGroupHeaderHeight = targetGrid.find(".k-grouping-header").outerHeight();

        var gridElements = gridPagerHeight + gridHeaderHeight + gridToolbarHeight + gridGroupHeaderHeight;
        targetGrid.find(".k-grid-content").height(height - gridElements);

        targetGrid.height(height);
    }

    $(window).resize(function () {
        resizeGrids();
    });
};