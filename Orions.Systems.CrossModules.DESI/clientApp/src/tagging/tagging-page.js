window.Orions.TaggingPage = {
	tagonomyInfoInit: function (selector, componentRef) {
		document.addEventListener('mousedown', function (e) {
			let el = document.querySelector(selector)

			if (e.target != el && el != null) {
				componentRef.invokeMethodAsync("Close");
			}
		})
	}
}