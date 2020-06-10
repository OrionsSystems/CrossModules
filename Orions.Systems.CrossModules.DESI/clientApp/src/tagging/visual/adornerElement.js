import * as paper from 'paper'
import BaseVisual from './baseVisual'

export default class AdornerElement extends BaseVisual {
	constructor(containerRectangle, adornerPositionName, layer, opts = {}) {
		super(containerRectangle, layer, opts);
		this.fillColor = 'white';
		this.strokeColor = 'black';
		this.positionName = adornerPositionName;
	}


	path_onMouseDrag(event) {
		// if adorner was dragged outside the canvas - dont process such event
		let containerRectangle = this.owner.containerRectangle;
		let center = this.owner.get_center();
		if (center.x + event.delta.x > containerRectangle.x + containerRectangle.width
			|| center.x + event.delta.x < containerRectangle.x
			|| center.y + event.delta.y > containerRectangle.y + containerRectangle.height
			|| center.y + event.delta.y < containerRectangle.y) {
			event.stopPropagation();
			return;
		}

		super.path_onMouseDrag(event);
	}
}