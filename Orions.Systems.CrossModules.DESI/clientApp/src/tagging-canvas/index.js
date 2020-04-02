import * as paper from 'paper';

window.Orions.TaggingSurface = {}

const getMethods = (obj) => {
	let properties = new Set()
	let currentObj = obj
	do {
		Object.getOwnPropertyNames(currentObj).map(item => properties.add(item))
	} while ((currentObj = Object.getPrototypeOf(currentObj)))
	return [...properties.keys()].filter(item => typeof obj[item] === 'function')
}

function stickToCanvasBounds(coords, containerRectangle) {
	if (coords.topLeft.y < 0 + containerRectangle.y) {
		coords.topLeft.y = containerRectangle.y;
	}

	if (coords.topLeft.x < 0 + containerRectangle.x) {
		coords.topLeft.x = containerRectangle.x;
	}

	if (coords.bottomRight.x > containerRectangle.x + containerRectangle.width) {
		coords.bottomRight.x = containerRectangle.x + containerRectangle.width;
	}

	if (coords.bottomRight.y > containerRectangle.y + containerRectangle.height) {
		coords.bottomRight.y = containerRectangle.height + containerRectangle.y;
	}
}

function isDefined(item) {
	return (item !== null && typeof item !== 'undefined');
}

class BaseVisual {
	constructor(containerRectangle) {
		this.containerRectangle = containerRectangle;
		this.min_size = 8;

		this.strokeColor = 'black';
		// RGBA
		this.fillColor = '#7776';
		this.selectedColor = '#f776';
		this.enter_fillColor = '#8886';

		this.elements = [];

		this.moved_event_listeners = [];
		this.eventListeners = {
			'selected': [],
			'deselected': [],
			'moved': [],
			'resized': []
		}

		this.was_moved = false;
	}

	add_moved_event_listener(listener) {
		var aa = this;
		this.moved_event_listeners.push(listener);
	}

	get_topLeft() {
		return new paper.Point(this.main_group.position.x - this.width / 2, this.main_group.position.y - this.height / 2);
	}

	get_center() {
		var topLeft = this.get_topLeft();
		var bottomRight = this.get_bottomRight();

		return new paper.Point(topLeft.x / 2 + bottomRight.x / 2, topLeft.y / 2 + bottomRight.y / 2);
	}

	get_bottomRight() {
		return new paper.Point(this.main_group.position.x + this.width / 2, this.main_group.position.y + this.height / 2);
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

		return this.main_group.hitTest(point);
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

	hideElements() {
		for (var i = 0; i < this.elements.length; i++) {
			this.elements[i].hide();
		}
	}

	showElements() {
		for (var i = 0; i < this.elements.length; i++) {
			this.elements[i].show();
		}
	}

	path_onMouseEnter(event) {
		// 'this' here is the path
		this.fillColor = this.owner.enter_fillColor;
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

	raise_moved_event() {
		for (var i = 0; i < this.moved_event_listeners.length; i++) {
			this.moved_event_listeners[i].moved_event(this);
		}
	}

	path_onMouseUp(event) {
		if (this.owner.was_moved) {
			this.owner.raise_moved_event();
			this.owner.was_moved = false;
			this.owner.fire('moved');
		}

		this.owner.select(true);

		
		this.owner.updateElements();
	}

	path_onMouseDrag(event) {

		this.owner.was_moved = true;
		this.position = new paper.Point(this.position.x + event.delta.x, this.position.y + event.delta.y);

		this.owner.raise_moved_event();
	}

	// recreate visuals from internal parameters.
	recreate() {
		var p1 = this.get_topLeft();
		var p2 = this.get_bottomRight();

		this.create(p1, p2);
	}

	// normalizes the coordinates
	create(p1, p2, selected = false) {
		this.is_selected = selected;

		var top_left = new paper.Point(Math.min(p1.x, p2.x), Math.min(p1.y, p2.y));
		var bottom_right = new paper.Point(Math.max(p1.x, p2.x), Math.max(p1.y, p2.y));

		// Enforce minimum sizes.
		if (bottom_right.x - top_left.x < this.min_size)
			bottom_right.x = top_left.x + this.min_size;

		if (bottom_right.y - top_left.y < this.min_size)
			bottom_right.y = top_left.y + this.min_size;

		this.create_raw(top_left, bottom_right);
	}


	// create the visual structures of this items, from these 2 points.
	create_raw(top_left, bottom_right) {

		if (top_left.x > bottom_right.x) {
			alert('asset fail');
			return;
		}

		if (top_left.y > bottom_right.y) {
			alert('asset fail');
			return;
		}

		if (isDefined(this.path)) {
			this.path.remove();
		}

		if (isDefined(this.main_group)) {
			this.main_group.remove();
		}

		this.width = bottom_right.x - top_left.x;
		this.height = bottom_right.y - top_left.y;

		var rect = new paper.Rectangle(new paper.Point(), new paper.Point(this.width, this.height)); // top_left is set as position.

		this.main_group = new paper.Group();

		var path = new paper.Path.Rectangle(rect);
		path.strokeColor = this.strokeColor;
		path.fillColor = this.fillColor;

		path.owner = this;

		// http://paperjs.org/reference/mouseevent/
		//this.main_group.onMouseEnter = this.path_onMouseEnter;
		//this.main_group.onMouseLeave = this.path_onMouseLeave;

		this.main_group.onMouseDown = this.path_onMouseDown;
		this.main_group.onMouseUp = this.path_onMouseUp;
		this.main_group.onMouseDrag = this.path_onMouseDrag;

		this.main_group.owner = this;
		this.main_group.addChild(path);

		this.main_group.position = new paper.Point(top_left.x / 2 + bottom_right.x / 2, top_left.y / 2 + bottom_right.y / 2);

		this.updateElements();
	}

	updateElements() {
		this.clearElements();
	}

	select(selected, fireEvent = true) {
		if (selected) {
			this.main_group.fillColor = this.selectedColor;
			this.is_selected = true;

			if (fireEvent)
				this.fire('selected');
		}
		else {
			this.main_group.fillColor = this.fillColor;
			this.is_selected = false;

			if (fireEvent)
				this.fire('deselected');
		}
	}

	fire(eventName) {
		this.eventListeners[eventName].forEach(listener => listener(this))
	}

	on(eventName, callback) {
		this.eventListeners[eventName].push(callback)
	}
}

class AdornerElement extends BaseVisual {
	constructor(containerRectangle) {
		super(containerRectangle);
		this.enter_fillColor = '#bbf';
		this.fillColor = 'blue';
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

class TagVisual extends BaseVisual {

	constructor(containerRectangle) {
		super(containerRectangle)
		this.adornerSize = 8;
		this.is_selected = false;
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

		// TODO : implement missing updates if needed
		console.log(`tag ${this.id} update method has been invoked`)
	}

	updateElements() {
		let self = this;
		super.updateElements();

		var p1 = this.get_topLeft();
		var p2 = this.get_bottomRight();

		var element = new AdornerElement(self.containerRectangle)
		element.create(new paper.Point(p2.x - this.adornerSize / 2, p2.y - this.adornerSize / 2), new paper.Point(p2.x + this.adornerSize / 2, p2.y + this.adornerSize / 2));

		element.add_moved_event_listener(this);
		element.on('moved', function () {
			self.fire('resized');
		})

		this.elements.push(element);
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

	// Element was moved.
	moved_event(element) {
		var center = element.get_center();

		//this.width = this.get_topLeft().x - center.x;
		//if (this.width < 0)
		//	this.width = -1 * this.width;

		var t = this.get_topLeft();
		var t2 = center;

		this.create(t, t2);
		this.updateElements();
	}
}

window.Orions.TaggingSurface.setupTaggingSurface = function (componentRef, componentId) {
	this.componentRef = componentRef;
	let self = this;
	self.canvas = document.querySelector(`#${componentId} .tagging-canvas`);

	var tool = new paper.Tool();

	var mouseDownAt;
	var path;
	var rect;

	var items = [];

	paper.setup(self.canvas);

	self.canvas.addEventListener('contextmenu', function (e) { e.preventDefault(); e.stopPropagation(); }, false);

	self.canvas.addEventListener('wheel', function (e) {
		if (e.shiftKey) {
			let step = 0.2;
			let maxZoom = 3;
			let minZoom = 1;
			if (e.deltaY > 0) {
				if (paper.view.zoom - step >= minZoom) {
					paper.view.zoom -= step;
				}
				else {
					paper.view.zoom = minZoom;
					paper.view.center = new paper.Point(self.canvas.width / 2, self.canvas.height / 2)
				}
			}
			else {
				if (paper.view.zoom + step < 3) {
					paper.view.zoom += step;
				}
				else {
					paper.view.zoom = maxZoom;
				}
			}
		}
	})

	let raster;
	let frameImage = document.getElementsByClassName('frame-img')[0]
	let updateFrameImageOnCanvas = function () {
		frameImage = document.getElementsByClassName('frame-img')[0];

		if (!raster) {
			raster = new paper.Raster(frameImage);
		}
		raster.image = frameImage

		raster.onLoad = function() {
			raster.position = paper.view.center
			raster.size = new paper.Size(frameImage.width, frameImage.height);
		}
	}
	updateFrameImageOnCanvas();

	let frameImageObserver = new MutationObserver((mutations) => {
		for (var i in mutations) {
			let mutation = mutations[i]
			if (mutation.type == 'attributes' && mutation.attributeName == 'src') {
				updateFrameImageOnCanvas()
			}
		}
	});
	frameImageObserver.observe(frameImage, {
		attributes: true		
	})

	frameImage.addEventListener('resize', function (e) {
		updateFrameImageOnCanvas()
	})

	let getProportionalRectangle = function (coords, containerRectangle) {
		let width = (coords.bottomRight.x - coords.topLeft.x) / containerRectangle.width;
		let height = (coords.bottomRight.y - coords.topLeft.y) / containerRectangle.height;

		var x = (coords.topLeft.x - containerRectangle.x) / containerRectangle.width;
		var y = (coords.topLeft.y - containerRectangle.y) / containerRectangle.height;

		return { x, y, width, height };
	};

	let getRectangleRealFromProportional = function (proportional, containerRectangle) {
		let width = proportional.width * containerRectangle.width;
		let height = proportional.height * containerRectangle.height;

		let topLeft = {
			x: proportional.x * containerRectangle.width + containerRectangle.x,
			y: proportional.y * containerRectangle.height + containerRectangle.y
		}

		let bottomRight = {
			x: topLeft.x + width,
			y: topLeft.y + height
		}

		return { topLeft, bottomRight };
	};

	tool.onKeyDown = function (event) {
		if (event.key == 'delete') {
			for (var i = 0; i < items.length; i++) {
				if (items[i].is_selected) {
					items[i].remove(); // Remove from visual space.
					items.splice(i, 1);
				}
			}

		}
	}

	// Define a mousedown and mousedrag handler
	tool.onMouseDown = function (event) {
		if (event.event.buttons == 2) {
			return;
		}

		if (event.point.x < raster.strokeBounds.x
			|| event.point.x > raster.strokeBounds.x + raster.strokeBounds.width
			|| event.point.y < raster.strokeBounds.y
			|| event.point.y > raster.strokeBounds.y + raster.strokeBounds.height) {
			return;
		}

		for (var i = 0; i < items.length; i++) {
			var hitResult = items[i].hitTest(event.point);
			if (isDefined(hitResult)) {
				return; // Hit one of the existing items
			}
		}

		var hitResult = paper.project.hitTest(event.point/*, hitOptions*/);
		if (isDefined(hitResult)) {
			var type = hitResult.item instanceof paper.Raster;
			if (type != true)
				return;
		}

		mouseDownAt = event.point;

		for (var i = 0; i < items.length; i++) {
			items[i].clearElements();
		}

		rect = new paper.Rectangle(mouseDownAt, mouseDownAt);
		path = new paper.Path.Rectangle(rect);
		path.strokeColor = 'black';
	}

	tool.onMouseUp = function (event) {
		if (isDefined(mouseDownAt) == false)
			return;

		// Apply min box size requirements
		if (Math.abs(mouseDownAt.x - event.point.x) < 25 && Math.abs(mouseDownAt.y - event.point.y) < 25) {
			mouseDownAt = undefined;

			if (isDefined(path))
				path.remove();

			return;
		}

		// we don't render the element immediately. instead, we call blazor side to create a tag and then let it ask for creating its visual representation
		// it allows us to create tags in one place (using addTag method)
		let rectCoords = {
			topLeft: { x: Math.min(mouseDownAt.x, event.point.x), y: Math.min(mouseDownAt.y, event.point.y) },
			bottomRight: { x: Math.max(mouseDownAt.x, event.point.x), y: Math.max(mouseDownAt.y, event.point.y) }
		}

		stickToCanvasBounds(rectCoords, raster.strokeBounds);

		let rect = getProportionalRectangle(rectCoords, raster.strokeBounds);
		self.componentRef.invokeMethodAsync("TagAdded", rect);

		mouseDownAt = undefined;
		path.remove();
	}

	tool.onMouseDrag = function (event) {
		if (event.event.buttons == 2) {
			if (paper.view.zoom > 1) {
				paper.view.center = paper.view.center.add(event.downPoint.subtract(event.point));
			}
		}
		else if (isDefined(mouseDownAt)) {
			rect.size = new paper.Size(event.point.x - mouseDownAt.x, event.point.y - mouseDownAt.y);

			if (path) {
				path.remove();
			}

			path = new paper.Path.Rectangle(rect);
			path.strokeColor = 'black';
		}
	}

	window.Orions.TaggingSurface.addTag = function (tag) {
		let rasterg = raster;
		var newTagVisual = new TagVisual(raster.strokeBounds);
		var tagCoords = getRectangleRealFromProportional(tag, raster.strokeBounds);

		newTagVisual.create(tagCoords.topLeft, tagCoords.bottomRight, tag.isSelected)
		newTagVisual.id = tag.id;

		if (tag.isSelected) {
			newTagVisual.select(true);
		}

		items.push(newTagVisual)

		newTagVisual.on('selected', function (selectedVisual) {
			// deselect other tags
			for (var i in items) {
				let visual = items[i]
				if (visual != selectedVisual) {
					visual.select(false);
				}
			}

			self.componentRef.invokeMethodAsync("TagSelected", selectedVisual.id);
		})

		let resizedOrMovedHandler = function (visual) {
			let rectCoords = {
				topLeft: visual.get_topLeft(),
				bottomRight: visual.get_bottomRight()
			};

			let rect = getProportionalRectangle(rectCoords, raster.strokeBounds);
			rect.id = visual.id;
			rect.isSelected = visual.is_selected

			self.componentRef.invokeMethodAsync("TagPositionOrSizeChanged", rect);
		}
		newTagVisual.on('moved', resizedOrMovedHandler)
		newTagVisual.on('resized', resizedOrMovedHandler)
	}

	window.Orions.TaggingSurface.updateTag = function (tag) {
		let tagToUpdate = items.find(i => i.id == tag.id);

		tagToUpdate.update(tag);
	}

	window.Orions.TaggingSurface.removeTag = function (tag) {
		let tagToRemove = items.find(i => i.id == tag.id);

		if (tagToRemove) {
			tagToRemove.remove();

			items.splice(items.indexOf(tagToRemove), 1);
		}
	}

	window.Orions.TaggingSurface.attachElementPositionToTag = function (id, elementSelector) {
		let tag = items.find(i => i.id == id);
		if (tag) {
			

			let tagAbsolutePosition = {
				x: tag.get_topLeft().x,
				y: tag.get_topLeft().y
			}

			let elementPosition = paper.view.projectToView({
				x: tagAbsolutePosition.x + tag.getWidth(),
				y: tagAbsolutePosition.y
			})
			elementPosition.x += 20

			let elementToAttach = document.querySelector(elementSelector);
			if (elementToAttach) {

				let elemWidth = elementToAttach.offsetWidth
				let elemHeight = elementToAttach.offsetHeight

				if (elementPosition.x + elemWidth > self.canvas.width) {
					elementPosition.x = elementPosition.x - elemWidth - tag.getWidth() * paper.view.zoom - 40
				}

				elementToAttach.style.left = elementPosition.x + 'px'
				elementToAttach.style.top = elementPosition.y + 'px'

				if (elementPosition.y + elemHeight > self.canvas.height) {
					elementToAttach.style.bottom = '20px'
					elementToAttach.style.top = ''
				}
			}
		}

	}
}