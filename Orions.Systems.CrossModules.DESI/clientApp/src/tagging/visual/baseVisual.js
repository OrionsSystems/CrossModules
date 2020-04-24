import * as paper from 'paper'
import { isDefined } from '../../utils'

const defaultFillColor = new paper.Color(255, 255, 255, 0.1);
const defaultSelectedColor = new paper.Color(255, 255, 255, 0.1);
const defaultStrokeColor = new paper.Color(255, 0, 0, 1);
const defaultSelectedStrokeColor = new paper.Color(0, 0, 255, 1);

export default class BaseVisual {
	constructor(containerRectangle, opts = {}) {
		this.min_size = 8;
		this.containerRectangle = containerRectangle;
		this.strokeColor = isDefined(opts.borderColor) ? opts.borderColor : defaultStrokeColor;
		this.fillColor = defaultFillColor;
		this.selectedColor = defaultSelectedColor;
		this.selectedStrokeColor = isDefined(opts.borderColor) ? opts.borderColor : defaultSelectedStrokeColor;
		this.elements = [];
		this.eventListeners = {
			'selected': [],
			'deselected': [],
			'moved': [],
			'resized': [],
			'moved_inner': []
		}

		this.was_moved = false;
	}

	get_topLeft() {
		return new paper.Point(this.path.bounds.left, this.path.bounds.top);
	}

	get_center() {
		var topLeft = this.get_topLeft();
		var bottomRight = this.get_bottomRight();

		return new paper.Point(topLeft.x / 2 + bottomRight.x / 2, topLeft.y / 2 + bottomRight.y / 2);
	}

	get_bottomRight() {
		return new paper.Point(this.path.bounds.right, this.path.bounds.bottom)
	}

	getWidth() {
		return this.width;
	}

	getHeight() {
		return this.height;
	}

	hitTest(point) {

		for (var i = 0; i < this.elements.length; i++) {
			var res = this.elements[i].hitTest(point);
			if (isDefined(res))
				return res;
		}

		return this.path.hitTest(point);
	}

	remove() {
		this.clearElements();
		this.main_group.remove();
	}

	hide() {
		this.main_group.visible = false;
	}

	show() {
		this.main_group.visible = true;
	}

	clearElements() {
		for (var i = 0; i < this.elements.length; i++) {
			this.elements[i].remove();
		}

		this.elements = [];
	}

	path_onMouseLeave(event) {
		// 'this' here is the path
		this.fillColor = this.owner.fillColor;
	}

	path_onMouseDown(event) {
		this.owner.was_moved = false;
		// 'this' here is the path

		this.owner.initial_position = this.position;

		this.owner.clearElements();
	}

	path_onMouseUp(event) {
		if (this.owner.was_moved) {
			this.owner.was_moved = false;
			this.owner.fire('moved');
		}

		this.owner.select(true);

		this.owner.updateElements();
	}

	path_onMouseDrag(event) {
		this.owner.was_moved = true;
		this.position = new paper.Point(this.position.x + event.delta.x, this.position.y + event.delta.y);

		this.owner.fire('moved_inner')
	}

	create(p1, p2) {

		var top_left = new paper.Point(Math.min(p1.x, p2.x), Math.min(p1.y, p2.y));
		var bottom_right = new paper.Point(Math.max(p1.x, p2.x), Math.max(p1.y, p2.y));

		// Enforce minimum sizes.
		if (bottom_right.x - top_left.x < this.min_size) {
			bottom_right.x = top_left.x + this.min_size;
		}

		if (bottom_right.y - top_left.y < this.min_size)
			bottom_right.y = top_left.y + this.min_size;

		if (isDefined(this.path)) {
			this.path.remove();
		}

		if (isDefined(this.main_group)) {
			this.main_group.remove();
		}

		this.width = bottom_right.x - top_left.x;
		this.height = bottom_right.y - top_left.y;

		var rect = new paper.Rectangle(new paper.Point(), new paper.Point(this.width, this.height)); // top_left is set as position.
		this.path = new paper.Path.Rectangle(rect);
		this.path.strokeColor = this.is_selected ? this.selectedStrokeColor : this.strokeColor;
		this.path.fillColor = this.is_selected ? this.selectedColor : this.fillColor;
		this.path.owner = this;

		this.main_group = new paper.Group();
		this.main_group.onMouseDown = this.path_onMouseDown;
		this.main_group.onMouseUp = this.path_onMouseUp;
		this.main_group.onMouseDrag = this.path_onMouseDrag;

		this.main_group.owner = this;
		this.main_group.addChild(this.path);
		this.main_group.position = new paper.Point(top_left.x / 2 + bottom_right.x / 2, top_left.y / 2 + bottom_right.y / 2);

		this.updateElements();
	}

	updateElements() {
		// leave base implementation empty
	}

	select(selected, fireEvent = true) {
		if (selected == this.is_selected) {
			return;
		}

		if (selected) {
			this.path.fillColor = this.selectedColor;
			this.path.strokeColor = this.selectedStrokeColor;
			this.is_selected = true;

			if (fireEvent)
				this.fire('selected');
		}
		else {
			this.path.fillColor = this.fillColor;
			this.path.strokeColor = this.strokeColor;
			this.is_selected = false;

			if (fireEvent)
				this.fire('deselected');
		}

		this.updateElements();
	}

	fire(eventName) {
		this.eventListeners[eventName].forEach(listener => listener(this))
	}

	on(eventName, callback) {
		this.eventListeners[eventName].push(callback)
	}
}