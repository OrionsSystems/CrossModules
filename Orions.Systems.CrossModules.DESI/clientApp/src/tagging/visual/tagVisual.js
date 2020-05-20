import * as paper from 'paper';
import BaseVisual from './baseVisual'
import AdornerElement from './adornerElement'
import { isDefined } from '../../../../../Orions.Systems.CrossModules.Components/content/WebPack/src/utils'

export default class TagVisual extends BaseVisual {

	constructor(containerRectangle, opts = {}) {
		super(containerRectangle, opts)
		this.adornerSize = 8;
		this.is_selected = false;
		this.label = isDefined(opts.label) ? opts.label : null;
	}

	update(tag) {
		const { x, y, width, height, isSelected } = tag;

		if (this.is_selected != isSelected) {
			this.select(isSelected, false)
		}

		if (isDefined(tag.borderColor)) {
			this.strokeColor = tag.borderColor;
			this.selectedStrokeColor = tag.borderColor;

			this.path.strokeColor = tag.borderColor;
		}

		if (isDefined(tag.label)) {
			this.label = tag.label;
		}

		this.updateElements();
	}

	updateElements() {
		let self = this;
		this.clearElements();

		if (this.is_selected) {
			var p1 = this.get_topLeft();
			var p2 = this.get_bottomRight();

			let adornerMovedInnerHandler = (el) => this.adorner_moved(el);
			let adornerMovedHandler = function () {
				self.fire('resized');
			}
			let rightBottomAdorner = new AdornerElement(self.containerRectangle, 'bottomRight')
			rightBottomAdorner.create(new paper.Point(p2.x - this.adornerSize / 2, p2.y - this.adornerSize / 2), new paper.Point(p2.x + this.adornerSize / 2, p2.y + this.adornerSize / 2));
			rightBottomAdorner.on('moved_inner', adornerMovedInnerHandler);
			rightBottomAdorner.on('moved', adornerMovedHandler)
			this.elements.push(rightBottomAdorner);

			let topLeftAdorner = new AdornerElement(self.containerRectangle, 'topLeft')
			topLeftAdorner.create(new paper.Point(p1.x - this.adornerSize / 2, p1.y - this.adornerSize / 2), new paper.Point(p1.x + this.adornerSize / 2, p1.y + this.adornerSize / 2));
			topLeftAdorner.on('moved_inner', adornerMovedInnerHandler);
			topLeftAdorner.on('moved', adornerMovedHandler)
			this.elements.push(topLeftAdorner);
		}

		if (this.label != null) {

			let labelPos = new paper.Point(this.path.bounds.left + 10, this.path.bounds.top + 10);
			if (this.labelItem == null) {
				this.labelItem = new paper.PointText(labelPos);
			}
			else {
				this.labelItem.position.y = labelPos.y;
				this.labelItem.position.x = labelPos.x;
			}

			this.labelItem.fillColor = 'white';
			this.labelItem.content = this.label;
			this.main_group.addChild(this.labelItem);
		}
	}

	clearElements() {
		super.clearElements();
	}

	path_onMouseDrag(event) {
		let containerRectangle = this.owner.containerRectangle;
		let topLeft = this.owner.get_topLeft();
		let bottomRight = this.owner.get_bottomRight();
		let newTopLeftX = event.delta.x + topLeft.x;
		let newTopLeftY = event.delta.y + topLeft.y;
		let newBottomRightX = event.delta.x + bottomRight.x;
		let newBottomRightY = event.delta.y + bottomRight.y;

		if (newTopLeftX < 0 + containerRectangle.x
			|| newTopLeftY < 0 + containerRectangle.y
			|| newBottomRightX > containerRectangle.x + containerRectangle.width
			|| newBottomRightY > containerRectangle.y + containerRectangle.height
		) {
			event.stopPropagation()
			return;
		}

		if (isDefined(this.owner.path) && this.owner.path.hitTest(event.point) == null) {
			event.stopPropagation();
			return;
		}

		super.path_onMouseDrag(event);
	}

	path_onMouseUp(event) {
		if (isDefined(this.owner.path) && this.owner.path.hitTest(event.point) == null) {
			event.stopPropagation();
			return;
		}

		super.path_onMouseUp(event);
	}

	path_onMouseDown(event) {
		if (isDefined(this.owner.path) && this.owner.path.hitTest(event.point) == null) {
			event.stopPropagation();
			return;
		}

		super.path_onMouseDown(event);
	}

	path_onMouseLeave(event) {
		if (isDefined(this.owner.path) && this.owner.path.hitTest(event.point) == null) {
			event.stopPropagation();
			return;
		}

		super.path_onMouseLeave(event);
	}

	adorner_moved(element) {
		var center = element.get_center();

		let topLeft;
		let bottomRight;

		switch (element.positionName) {
			case 'bottomRight':
				topLeft = this.get_topLeft();
				bottomRight = center;
				break;
			case 'topLeft':
				topLeft = center;
				bottomRight = this.get_bottomRight();
				break;
		}

		this.create(topLeft, bottomRight);
		this.updateElements();
	}
}
