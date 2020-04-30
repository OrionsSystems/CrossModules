import { isDefined } from "../utils";

class Minimap{
	constructor() {
		this.init.bind(this)
		this.registerSurface.bind(this)
		this.updateCurrentRegionPosition.bind(this)
		this.dispose.bind(this)
	}

	

	init() {
		let self = this;
		Orions.TaggingSurface.canvasViewPositionUpdatedListeners.push((raster, view) => this.updateCurrentRegionPosition(raster, view))
		Orions.TaggingSurface.surfaceRegisteredListeners.push((surface) => this.registerSurface(surface))

		this.currentRegion = document.querySelector('.minimap .current-region')
		this.minimapContainer = document.querySelector('.minimap')
		this.minimapContainer.addEventListener('mousedown', function (e) {
			let moveViewEventListener = function(e) {
				if (self.taggingSurface != null) {
					let offsetX = e.pageX - self.minimapContainer.getBoundingClientRect().x
					let offsetY = e.pageY - self.minimapContainer.getBoundingClientRect().y
					let clickRelativePosX = offsetX / self.minimapContainer.offsetWidth
					let clickRelativePosY = offsetY / self.minimapContainer.offsetHeight

					self.taggingSurface.setViewPosition(clickRelativePosX, clickRelativePosY);
				}
			}

			moveViewEventListener(e);

			self.minimapContainer.addEventListener('mousemove', moveViewEventListener)

			let mouseupEventListener = function(e) {
				self.minimapContainer.removeEventListener('mousemove', moveViewEventListener);
				document.removeEventListener('mouseup', mouseupEventListener)
			}
			document.addEventListener('mouseup', mouseupEventListener)
		})

		this.minimapContainer.addEventListener('wheel', function (e) {
			if (e.shiftKey && self.taggingSurface != null) {
				let zoomOut = e.deltaY > 0

				self.taggingSurface.zoomOneStep(zoomOut, true);
			}
		})
	}

	registerSurface(surface) {
		this.taggingSurface = surface;
	}

	updateCurrentRegionPosition(raster, view) {
		let self = this;
		if (!isDefined(raster)) {
			return
		}

		let currentRegion = self.currentRegion;

		let rasterTopOffset = (raster.position.y - raster.height / 2) - taggingSurfaceDebug.scope.view.bounds.top
		rasterTopOffset = rasterTopOffset < 0 ? 0 : rasterTopOffset;
		let rasterBottomOffset = taggingSurfaceDebug.scope.view.bounds.bottom - (raster.position.y + raster.height / 2);
		rasterBottomOffset = rasterBottomOffset < 0 ? 0 : rasterBottomOffset;
		let visibleRasterHeight = view.bounds.height - rasterTopOffset - rasterBottomOffset;

		let regionHeightNormalized = visibleRasterHeight / raster.height
		currentRegion.style.height = regionHeightNormalized > 1 ? '100%' :
			regionHeightNormalized
			* 100 + '%'

		let rasterLeftOffset = (raster.position.x - raster.width / 2) - view.bounds.left
		rasterLeftOffset = rasterLeftOffset < 0 ? 0 : rasterLeftOffset;
		let rasterRightOffset = view.bounds.right - (raster.position.x + raster.width / 2);
		rasterRightOffset = rasterRightOffset < 0 ? 0 : rasterRightOffset;
		let visibleRasterWidth = view.bounds.width - rasterLeftOffset - rasterRightOffset;

		let regionWidthNormalized = visibleRasterWidth / raster.width
		currentRegion.style.width = regionWidthNormalized > 1 ? '100%' : regionWidthNormalized * 100 + '%'

		currentRegion.style.left = (taggingSurfaceDebug.scope.view.bounds.x - (raster.position.x - raster.width / 2)) / raster.width * 100 > 0 ? (taggingSurfaceDebug.scope.view.bounds.x - (raster.position.x - raster.width / 2)) / raster.width * 100 + '%' : 0
		currentRegion.style.top = (taggingSurfaceDebug.scope.view.bounds.y - (raster.position.y - raster.height / 2)) / raster.height * 100 > 0 ? (taggingSurfaceDebug.scope.view.bounds.y - (raster.position.y - raster.height / 2)) / raster.height * 100 + '%' : 0

		if (!(regionWidthNormalized >= 1 && regionHeightNormalized >= 1)) {
			self.minimapContainer.style.display = 'block'
		}
		else {
			self.minimapContainer.style.display = 'none'
		}
	}

	dispose() {
		Orions.TaggingSurface.canvasViewPositionUpdatedListeners.splice(Orions.TaggingSurface.canvasViewPositionUpdatedListeners.indexOf(this.updateCurrentRegionPosition), 1);
	}
}

window.Orions.Minimap = new Minimap();