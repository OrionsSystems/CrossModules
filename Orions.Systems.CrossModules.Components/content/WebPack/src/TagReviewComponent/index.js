window.Orions.TagReviewComponent = {
	init: function () {
		document.querySelector(".metadatareview-page-controls .page-number-input").addEventListener("keypress", function (evt) {
			if (evt.which != 8 && evt.which != 0 && evt.which < 48 || evt.which > 57) {
				evt.preventDefault();
			}
		});

		document.querySelector(".metadatareview-page-controls .page-number-input").addEventListener("blur", function (evt) {
			var targetValueInt = evt.target.value - 0;
			var targetMinInt = evt.target.min - 0;
			var targetMaxInt = evt.target.max - 0;

			if (targetValueInt < targetMinInt) {
				evt.target.value = evt.target.min;
			}

			if (targetValueInt > targetMaxInt) {
				evt.target.value = evt.target.max;
			}
		});
	}
}