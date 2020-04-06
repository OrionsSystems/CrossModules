import * as paper from 'paper';
import CanvasCrosshair from './canvas-crosshair'
import { isDefined } from '../utils'
import TagVisual from './visual/tagVisual'

window.Orions.TaggingSurface = {}

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

window.Orions.TaggingSurface.setupTaggingSurface = function (componentRef, componentId) {
	this.componentRef = componentRef;
	this.scope = new paper.PaperScope

	let self = this;
	self.canvas = document.querySelector(`#${componentId} .tagging-canvas`);

	var tool = new self.scope.Tool();
	var mouseDownAt;
	var path;
	var rect;
	var items = [];

	self.scope.setup(self.canvas);
	let crosshair = new CanvasCrosshair(self.scope);

	self.canvas.addEventListener('contextmenu', function (e) { e.preventDefault(); e.stopPropagation(); }, false);

	self.canvas.addEventListener('wheel', function (e) {
		if (e.shiftKey) {
			let step = 0.2;
			let maxZoom = 3;
			let minZoom = 1;
			if (e.deltaY > 0) {
				if (self.scope.view.zoom - step >= minZoom) {
					self.scope.view.zoom -= step;
				}
				else {
					self.scope.view.zoom = minZoom;
					self.scope.view.center = new paper.Point(self.canvas.width / 2, self.canvas.height / 2)
				}
			}
			else {
				if (self.scope.view.zoom + step < 3) {
					self.scope.view.zoom += step;
				}
				else {
					self.scope.view.zoom = maxZoom;
				}
			}

			crosshair.move(self.scope.view.viewToProject({ x: e.offsetX, y: e.offsetY }))
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
			raster.position = self.scope.view.center
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

		mouseDownAt = event.point;

		for (var i = 0; i < items.length; i++) {
			items[i].clearElements();
		}

		rect = new paper.Rectangle(mouseDownAt, mouseDownAt);
		path = new paper.Path.Rectangle(rect);
		path.strokeColor = 'red';
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
		crosshair.move(event.point)

		if (event.event.buttons == 2) {
			if (self.scope.view.zoom > 1) {
				self.scope.view.center = self.scope.view.center.add(event.downPoint.subtract(event.point));
			}
		}
		else if (isDefined(mouseDownAt)) {
			rect.size = new paper.Size(event.point.x - mouseDownAt.x, event.point.y - mouseDownAt.y);

			if (path) {
				path.remove();
			}

			path = new paper.Path.Rectangle(rect);
			path.strokeColor = 'red';
		}
	}

	tool.onMouseMove = function (event) {
		crosshair.move(event.point)
	}

	window.Orions.TaggingSurface.addTag = function (tag) {
		let rasterg = raster;
		var newTagVisual = new TagVisual(raster.strokeBounds, tag);
		var tagCoords = getRectangleRealFromProportional(tag, raster.strokeBounds);

		newTagVisual.create(tagCoords.topLeft, tagCoords.bottomRight)
		newTagVisual.id = tag.id;

		if (tag.isSelected) {
			newTagVisual.select(true);
		}

		items.push(newTagVisual)

		newTagVisual.on('selected', function (selectedVisual) {
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

			let elementPosition = self.scope.view.projectToView({
				x: tagAbsolutePosition.x + tag.getWidth(),
				y: tagAbsolutePosition.y
			})
			elementPosition.x += 20

			let elementToAttach = document.querySelector(elementSelector);
			if (elementToAttach) {

				let elemWidth = elementToAttach.offsetWidth
				let elemHeight = elementToAttach.offsetHeight

				if (elementPosition.x + elemWidth > self.canvas.width) {
					elementPosition.x = elementPosition.x - elemWidth - tag.getWidth() * self.scope.view.zoom - 40
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

	window.Orions.TaggingSurface.dispose = function () {
		self.scope.remove();
	}
}