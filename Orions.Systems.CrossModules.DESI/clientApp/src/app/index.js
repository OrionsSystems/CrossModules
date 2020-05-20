window.Orions.DesiApp = {
    initialize: function (componentRef) {
        $('body').click(function () {
            this.blur();
        });
    }
}