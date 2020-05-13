import { isDefined } from '../utils/index'

window.Orions.Vizlist = {
	init: function (elementId) {
		let element = $(`#${elementId}`);

		if (isDefined(element) && isDefined(element.draggable)) {
			element.draggable();
		}
	}
}