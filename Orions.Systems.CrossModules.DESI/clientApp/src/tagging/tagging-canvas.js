import * as paper from 'paper';
import CanvasCrosshair from './canvas-crosshair'
import { isDefined } from '../../../../Orions.Systems.CrossModules.Components/content/WebPack/src/utils'
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

window.Orions.TaggingSurface = {
	canvasViewPositionUpdatedListeners: [],
	surfaceRegisteredListeners: [],
	surfaces: {},
	setupTaggingSurface: function (componentRef, componentId) {
		let newSufrace = new TaggingSurface(componentRef, componentId);
		newSufrace.setupTaggingSurface()
		this.surfaces[componentId] = newSufrace
	},
	addTag: function (componentId, tag) {
		let surface = this.surfaces[componentId];
		if (isDefined(surface)) {
			surface.addTag(tag);
		}
	},
	updateTag: function (componentId, tag) {
		let surface = this.surfaces[componentId];
		if (isDefined(surface)) {
			surface.updateTag(tag);
		}
	},
	removeTag: function (componentId, tag) {
		let surface = this.surfaces[componentId];
		if (isDefined(surface)) {
			surface.removeTag(tag);
		}
	},
	updateRectangles: function (componentId, updates) {
		let surface = this.surfaces[componentId];
		if (isDefined(surface)) {
			surface.updateRectangles(updates);
		}
	},
	attachElementPositionToTag: function (componentId, id, elementSelector) {
		let surface = this.surfaces[componentId];
		if (isDefined(surface)) {
			surface.attachElementPositionToTag(id, elementSelector);
		}
	},
	resetZoom: function (componentId) {
		let surface = this.surfaces[componentId];
		if (isDefined(surface)) {
			surface.resetZoom();
		}
	},
	updateFrameImage: function (componentId, imageBase64) {
		let surface = this.surfaces[componentId];
		if (isDefined(surface)) {
			return surface.updateFrameImage(imageBase64);
		}
	},
	setFrameMode: function (componentId, visible) {
		let surface = this.surfaces[componentId];
		if (isDefined(surface)) {
			surface.setFrameMode(visible);
		}
	},
	dispose: function (componentId) {
		let surface = this.surfaces[componentId];
		if (isDefined(surface)) {
			surface.dispose();
		}
	}
}

class TaggingSurface {
	constructor(componentRef, componentId) {
		this.componentRef = componentRef;
		this.componentId = componentId;
	}

	setupTaggingSurface() {
		window.taggingSurfaceDebug = this;

		let self = this;
		self.scope = new paper.PaperScope()
		self.canvas = document.querySelector(`#tagging-surface-${self.componentId} .tagging-canvas`);
		self.items = [];
		self.raster = null;

		let tool = new self.scope.Tool();
		let mouseDownAt;
		let path;
		let rect;

		self.scope.setup(self.canvas);
		self.itemsLayer = new paper.Layer();
		self.rasterLayer = new paper.Layer();
		self.scope.project.addLayer(self.rasterLayer);
		self.scope.project.addLayer(self.itemsLayer);

		let crosshair = new CanvasCrosshair(self.scope, self.itemsLayer);

		function notifyViewPositionUpdate() {
			for (let listener of Orions.TaggingSurface.canvasViewPositionUpdatedListeners) {
				listener(self.raster, self.scope.view)
			}
		}

		self.canvas.addEventListener('contextmenu', function (e) { e.preventDefault(); e.stopPropagation(); }, false);

		self.canvas.addEventListener('wheel', function (e) {
			if (e.shiftKey) {
				let mousePosPaper = self.scope.view.viewToProject({ x: e.offsetX, y: e.offsetY })

				let zoomOut = e.deltaY > 0
				self.zoomOneStep(zoomOut, false);

				let mousePosPaperNew = self.scope.view.viewToProject({ x: e.offsetX, y: e.offsetY })

				if (self.scope.view.zoom > 1) {
					self.scope.view.center = self.scope.view.center.add(mousePosPaper.subtract(mousePosPaperNew));
				}
				crosshair.move(mousePosPaper)

				notifyViewPositionUpdate();
			}
		})

		self.zoomOneStep = function (zoomOut, notify = false) {
			let step = 0.2;
			let maxZoom = 3;
			let minZoom = 1;

			if (zoomOut) {
				if (self.scope.view.zoom - step > minZoom) {
					self.scope.view.zoom -= step;
				}
				else {
					self.scope.view.zoom = minZoom;
					self.scope.view.center = new paper.Point(self.scope.view.bounds.width / 2, self.scope.view.bounds.height / 2)
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

			if (notify) {
				notifyViewPositionUpdate();
			}
		}

		let updateFrameImageOnCanvas = function (imageBase64) {
			let raster = new paper.Raster(imageBase64);

			raster.visible = false;
			self.rasterLayer.addChild(raster);

			let loadPromise = new Promise((resolve, reject) => {
				raster.onLoad = function () {
					raster.position = self.scope.view.center

					let viewAspectRatio = self.scope.view.bounds.width / self.scope.view.bounds.height
					let imageAspectRatio = raster.width / raster.height

					if (viewAspectRatio < imageAspectRatio) {
						raster.size.width = self.scope.view.bounds.width
						raster.size.height = raster.size.width / imageAspectRatio;
					}
					else {
						raster.size.height = self.scope.view.bounds.height
						raster.size.width = raster.size.height * imageAspectRatio;
					}

					if (self.raster != null) {
						self.raster.remove();
					}

					self.raster = raster;

					self.raster.visible = true;
					resolve();
				}
			})

			return loadPromise;
		}

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

		tool.onMouseDown = function (event) {
			if (event.event.buttons == 2) {
				return;
			}

			if (self.raster == null || event.point.x < self.raster.strokeBounds.x
				|| event.point.x > self.raster.strokeBounds.x + self.raster.strokeBounds.width
				|| event.point.y < self.raster.strokeBounds.y
				|| event.point.y > self.raster.strokeBounds.y + self.raster.strokeBounds.height) {
				return;
			}

			for (var i = 0; i < self.items.length; i++) {
				var hitResult = self.items[i].hitTest(event.point);
				if (isDefined(hitResult)) {
					return; // Hit one of the existing self.items
				}
			}

			mouseDownAt = event.point;

			for (var i = 0; i < self.items.length; i++) {
				self.items[i].clearElements();
			}

			rect = new paper.Rectangle(mouseDownAt, mouseDownAt);
			path = new paper.Path.Rectangle(rect);
			path.strokeColor = 'red';
		}

		tool.onMouseUp = function (event) {
			let propagateClickToUnderlyingPlayerContainer = function () {
				let videoPlayerContainer = document.querySelector('.video-player')
				if (typeof videoPlayerContainer !== 'undefined') {
					videoPlayerContainer.click();
				}
			}

			if (self.raster == null) {
				propagateClickToUnderlyingPlayerContainer();
			}

			if (isDefined(mouseDownAt) == false)
				return;

			// Apply min box size requirements
			if (Math.abs(mouseDownAt.x - event.point.x) < 5 && Math.abs(mouseDownAt.y - event.point.y) < 5) {
				mouseDownAt = undefined;

				if (isDefined(path))
					path.remove();

				propagateClickToUnderlyingPlayerContainer();
				return;
			}

			// we don't render the element immediately. instead, we call blazor side to create a tag and then let it ask for creating its visual representation
			// it allows us to create tags in one place (using addTag method)
			let rectCoords = {
				topLeft: { x: Math.min(mouseDownAt.x, event.point.x), y: Math.min(mouseDownAt.y, event.point.y) },
				bottomRight: { x: Math.max(mouseDownAt.x, event.point.x), y: Math.max(mouseDownAt.y, event.point.y) }
			}

			stickToCanvasBounds(rectCoords, self.raster.strokeBounds);

			let rect = getProportionalRectangle(rectCoords, self.raster.strokeBounds);
			self.componentRef.invokeMethodAsync("TagAdded", rect);

			mouseDownAt = undefined;
			path.remove();
		}

		tool.onMouseDrag = function (event) {
			crosshair.move(event.point)

			if (event.event.buttons == 2) {
				if (self.scope.view.zoom > 1) {
					self.scope.view.center = self.scope.view.center.add(event.downPoint.subtract(event.point));

					notifyViewPositionUpdate();
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

		self.addRectangle = function (rectangle) {
			let rectanglewithSameId = self.items.find(t => t.id == rectangle.id)
			if (isDefined(rectanglewithSameId)) {
				rectanglewithSameId.remove();
				self.items.splice(self.items.indexOf(rectanglewithSameId), 1);
			}

			var newRectangleVisual = new TagVisual(self.raster.strokeBounds, self.itemsLayer, rectangle);
			var rectangleCoords = getRectangleRealFromProportional(rectangle, self.raster.strokeBounds);

			newRectangleVisual.create(rectangleCoords.topLeft, rectangleCoords.bottomRight)
			newRectangleVisual.id = rectangle.id;

			if (rectangle.isSelected) {
				newRectangleVisual.select(true);
			}

			self.items.push(newRectangleVisual)

			newRectangleVisual.on('selected', function (selectedVisual) {
				self.componentRef.invokeMethodAsync("TagSelected", selectedVisual.id);
			})

			let resizedOrMovedHandler = function (visual) {
				let rectCoords = {
					topLeft: visual.get_topLeft(),
					bottomRight: visual.get_bottomRight()
				};

				let rect = getProportionalRectangle(rectCoords, self.raster.strokeBounds);
				rect.id = visual.id;
				rect.isSelected = visual.is_selected

				self.componentRef.invokeMethodAsync("TagPositionOrSizeChanged", rect);
			}
			newRectangleVisual.on('moved', resizedOrMovedHandler)
			newRectangleVisual.on('resized', resizedOrMovedHandler)
		}

		self.updateRectangle = function (rectangle) {
			let rectangleToUpdate = self.items.find(i => i.id == rectangle.id);

			if (isDefined(rectangleToUpdate)) {
				rectangleToUpdate.update(rectangle);
			}
		}

		self.removeRectangle = function (rectangle) {
			let rectangleToRemove = self.items.find(i => i.id == rectangle.id);

			if (isDefined(rectangleToRemove)) {
				rectangleToRemove.remove();

				self.items.splice(self.items.indexOf(rectangleToRemove), 1);
			}
		}

		self.updateRectangles = function (updates) {
			for (let tag of updates.addings) {
				self.addRectangle(tag);
			}

			for (let tag of updates.updates) {
				self.updateRectangle(tag);
			}

			for (let tag of updates.removals) {
				self.removeRectangle(tag);
			}
		}

		self.attachElementPositionToTag = function (id, elementSelector) {
			let tag = self.items.find(i => i.id == id);
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

		self.resetZoom = function () {
			if (isDefined(self.scope.view)) {
				self.scope.view.zoom = 1;
				self.scope.view.center = new paper.Point(self.scope.view.bounds.width / 2, self.scope.view.bounds.height / 2)

				notifyViewPositionUpdate();
			}
		}

		self.updateFrameImage = function (imageBase64) {
			return updateFrameImageOnCanvas(imageBase64);
		}

		self.setViewPosition = function (relativeX, relativeY) {
			let rasterX = self.raster.width * relativeX + self.raster.position.x - self.raster.width / 2;
			let rasterY = self.raster.height * relativeY + self.raster.position.y - self.raster.height / 2;
			self.scope.view.center = new paper.Point(rasterX, rasterY)

			notifyViewPositionUpdate();
		}

		self.dispose = function () {
			self.items = [];
			self.scope.remove();
			self.raster.remove();
			self.raster = null;
			tool.remove();
		}

		for (let listener of Orions.TaggingSurface.surfaceRegisteredListeners) {
			listener(this);
		}
	}

	setFrameMode(visible) {
		if (this.raster != null) {
			this.raster.visible = visible
		}

		this.resetZoom();
		this.canvas.style.background = visible ? 'black' : 'transparent'
	}
}