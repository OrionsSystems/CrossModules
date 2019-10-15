/*
need to construct a grid with these configuration
		.Pageable()
		.AutoBind(false)
..
DataSource
    .Ajax
    .PageSize
    .ServerOperation(true)
..
*/
$.fn.initPaging = function (kendoType, pageSize) {
    var ds = $(this).data(kendoType).dataSource;

    if (pageSize != undefined && pageSize > 0) {
        ds.options.pageSize = pageSize;
        ds._pageSize = pageSize;
    }

    ds.options.page = 1;
    ds._page = 1;

    ds._total = 10000000;
    ds.options.total = 10000000;

    //First Read
    ds.read(ds._page);

    var pager = $(this).find(".k-pager-wrap.k-grid-pager");
    pager.html(
        "<a class='grid-prev-page p-sm k-state-disabled'><i class='fa fa-chevron-left fa-2x'></i></a>" +
        "<span class='grid-current-page k-state-selected p-sm'><b>1</b></span>" +
        "<a class='grid-next-page p-sm'><i class='fa fa-chevron-right fa-2x'></i></a>"
    );
    var prev = pager.find(".grid-prev-page");
    var next = pager.find(".grid-next-page");
    var currentPage = pager.find(".grid-current-page");
    $(next).click(function () {
        ds.options.page = ds.options.page + 1;
        ds._page = ds.options.page;
        ds.read(ds.options.page);
        currentPage.html(ds.options.page);
        if (ds.options.page > 1) {
            prev.removeClass("k-state-disabled");
        }
    });
    $(prev).click(function () {
        if (prev.hasClass("k-state-disabled")) {
            return;
        }
        ds.options.page = ds.options.page - 1;
        if (ds.options.page <= 0) {
            ds.options.page = 1;
        }
        ds._page = ds.options.page
        ds.read(ds.options.page);
        if (ds.options.page <= 1) {
            prev.addClass("k-state-disabled");
        }
        currentPage.html(ds.options.page);
    });
};
