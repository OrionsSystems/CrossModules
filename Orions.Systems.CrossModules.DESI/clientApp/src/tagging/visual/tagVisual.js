﻿import * as paper from 'paper';
import BaseVisual from './baseVisual'
import AdornerElement from './adornerElement'
import { isDefined } from '../../utils';

export default class TagVisual extends BaseVisual {

	constructor(containerRectangle, opts = {}) {
		super(containerRectangle, opts)
		this.adornerSize = 8;
		this.is_selected = false;
		this.label = isDefined(opts.label) ? opts.label : null;
	}

	path_onMouseDown(event) {
		super.path_onMouseDown(event);
	}

	path_onMouseLeave(event) {
		super.path_onMouseLeave(event);
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

			var element = new AdornerElement(self.containerRectangle)
			element.create(new paper.Point(p2.x - this.adornerSize / 2, p2.y - this.adornerSize / 2), new paper.Point(p2.x + this.adornerSize / 2, p2.y + this.adornerSize / 2));

			element.on('moved_inner', (el) => this.adorner_moved(el));
			element.on('moved', function () {
				self.fire('resized');
			})

			this.elements.push(element);
		}

		if (this.label != null) {

			let labelPos = new paper.Point(this.path.bounds.left + 10, this.path.bounds.top + 10);
			if (this.labelItem == null) {
				this.labelItem = new paper.PointText(labelPos);
			}
			else {
				this.labelItem.position.top = labelPos.y;
				this.labelItem.position.left = labelPos.x;
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

		super.path_onMouseDrag(event);
	}

	adorner_moved(element) {
		var center = element.get_center();

		var t = this.get_topLeft();
		var t2 = center;

		this.create(t, t2);
		this.updateElements();
	}
}
