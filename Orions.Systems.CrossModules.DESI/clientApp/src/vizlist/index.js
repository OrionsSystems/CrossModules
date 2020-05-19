import { createPopper } from '@popperjs/core';
import { isDefined } from '../utils/index'

window.Orions.Vizlist = {
	init: function (elementId) {
		let element = $(`#${elementId}`);
		if (isDefined(element) && isDefined(element.draggable)) {
			element.draggable();
		}
	},

	initPopover: function (referenceElId, targetElId) {
		let nodeButton = document.querySelector(`#${referenceElId}`)
		let imagePopup = document.querySelector(`#${targetElId}`)
		let customBoundary = document.querySelector('body')
		let cfg = {
			placement: 'top',
			modifiers: [
				{
					name: 'preventOverflow',
					options: {
						boundary: customBoundary, // false by default
					},
				},
				{
					name: 'flip',
					options: {
						fallbackPlacements: ['top', 'right', 'left', 'bottom'],
					},
				}
			]
		};
		let popper = createPopper(nodeButton, imagePopup, cfg);
		nodeButton.addEventListener('mouseenter', function (e) {
			imagePopup.style.display = 'block'
			popper.update();
		})
		nodeButton.addEventListener('mouseleave', function (e) {
			imagePopup.style.display = 'none';
		})

		nodeButton.addEventListener('click', function (e) {
			imagePopup.style.display = 'none';
		})
	}
}