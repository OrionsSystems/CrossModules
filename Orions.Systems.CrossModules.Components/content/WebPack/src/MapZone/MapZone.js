

(function () {
	window.Orions.MapZone = {
		Init: function (elementId, classNameOnMouseOver, componentInstance) {
			var element = document.querySelector('#' + elementId);

			if (element && element.hasChildNodes()) {
				NodeList.prototype.forEach = Array.prototype.forEach;
				var zones = element.childNodes;

				zones.forEach(function (zone) {
					if (zone.nodeType !== Node.TEXT_NODE) {

						zone.addEventListener("mouseover", function (event) {
							event.target.classList.add(classNameOnMouseOver);
						});

						zone.addEventListener("mouseout", function (event) {
							event.target.classList.remove(classNameOnMouseOver);
						});

						zone.addEventListener("click", function (event) {
							componentInstance.invokeMethodAsync('OnZoneClick', zone.id).then(null, function (err) {
								throw new Error(err);
							});
						});
					}
				});
			}
		},
		AddClassById: function (elementId, className) {
			var element = document.querySelector('#' + elementId);

			element.className = className;
			element.classList.add(className);
		},
		RemoveClassById: function (elementId, className) {
			var element = document.querySelector('#' + elementId);

			element.className = className;
			element.classList.remove(className);
		}
	};
})();
