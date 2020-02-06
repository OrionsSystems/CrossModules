window.Orions.SvgMapEditor = {
    layoutEditors: {},

    init: function (rootElementId, componentReference, mapOverlay, isReadOnly) {
        let layoutEditor = SvgMapEditor(rootElementId, componentReference, mapOverlay, isReadOnly);

        this.layoutEditors[rootElementId] = layoutEditor;
    },

    update: function (rootElementId, updateDetails) {
        let layoutEditor = this.layoutEditors[rootElementId];

        layoutEditor.update(updateDetails)
    }
} 

function SvgMapEditor(rootElementId, componentReference, mapOverlay, isReadOnly) {
    var rootSelector = '#' + rootElementId;
    var svg = document.querySelector(rootSelector + ' .map-container svg');
    

    svgPanZoom(svg, {
        zoomScaleSensitivity: 0.4,
        controlIconsEnabled: true
    })

    var draw = SVG(rootSelector + ' .svg-pan-zoom_viewport');
    let zonesLayer = draw.group();
    let camerasLayer = draw.group();
    let circlesLayer = draw.group();

    let zones = []
    let cameras = []
    let circles = []

    var shapeCommonAttr = {
        fill: 'red',
        'fill-opacity': 0.5,
        'stroke-width': 0.5,
        'stroke': '#6e6e6e'
    }

    let initializeCircle = function (circleOverlayEntry, startUserDrawing) {
        let newCircle = new SvgToolbox.CircleZone({
            svgRoot: draw,
            svgNode: circlesLayer,
            attr: getCircleAttr(),
            center: circleOverlayEntry.center,
            size: circleOverlayEntry.size,
            startUserDrawing: startUserDrawing,
            id: circleOverlayEntry.id,
            isReadOnly
        });
        newCircle.persist = circleOverlayEntry.persist;

        circles.push(newCircle);

        newCircle.onRemove((c) => {
            circles.splice(circles.findIndex(el => el == c), 1)
        })

        return newCircle;
    }

    let initializeZone = function (zoneOverlayEntry, startUserDrawing) {
        let newZone = new SvgToolbox.Zone({
            svgRoot: draw,
            svgNode: zonesLayer,
            attr: getZoneAttr(),
            points: zoneOverlayEntry.points,
            startUserDrawing: startUserDrawing,
            name: zoneOverlayEntry.name,
            isReadOnly
        });
        newZone.persist = zoneOverlayEntry.persist;

        zones.push(newZone)

        newZone.onRemove((z) => {
            zones.splice(zones.findIndex(el => el == z), 1)
        })

        return newZone;
    }

    let initializeCamera = function (cameraOverlayEntry, isDefaultPosition) {
        let newCamera = new SvgToolbox.Camera({
            svgRoot: draw,
            svgNode: camerasLayer,
            attr: getCameraAttr(),
            points: cameraOverlayEntry.points,
            transformMatrix: cameraOverlayEntry.transformMatrix,
            isDefaultPosition: isDefaultPosition,
            isReadOnly
        });

        newCamera.persist = cameraOverlayEntry.persist;

        cameras.push(newCamera)

        newCamera.onRemove((c) => {
            cameras.splice(cameras.findIndex(el => el == c), 1)
        })

        return newCamera;
    }

    let updateCircle = function (circleOverlayEntry) {
        let circleControl = circles.find(c => c.id == circleOverlayEntry.id);
        circleControl.center(circleOverlayEntry.center.x, circleOverlayEntry.center.y, true)
    }

    function initializeMapOverlay(overlay) {
        if (overlay.zones) {
            overlay.zones.forEach((item) => {
                item.persist = true;
                initializeZone(item)
            })
        }

        if (overlay.circles) {
            overlay.circles.forEach((item) => {
                item.persist = true
                initializeCircle(item)
            })
        }

        if (overlay.cameras) {
            overlay.cameras.forEach((item) => {
                initializeCamera(item)
            })
        }
    }

    function addPanelButtonsEventListeners() {
        document.querySelector(rootSelector + " .circleToolBtn")
            .addEventListener("click", addCircleTool);

        document.querySelector(rootSelector + " .areaToolBtn")
            .addEventListener("click", addAreaTool);

        document.querySelector(rootSelector + " .cameraToolBtn")
            .addEventListener("click", addCameraTool);

        document.querySelector(rootSelector + " .saveBtn")
            .addEventListener("click", saveMapOverlay);
    }

    function saveMapOverlay() {
        document.querySelector(rootSelector + " .saveBtn").setAttribute('disabled', 'disabled')

        let overlay = {
            id: mapOverlay.id,
            name: mapOverlay.name,
            zones: zones.filter(z => z.persist).map(z => {
                return {
                    name: z.name.get(),
                    points: z.polygon.array().map(ap => {
                        return {
                            x: ap[0],
                            y: ap[1]
                        }
                    })
                }
            }),
            circles: circles.filter(c => c.persist).map(c => {
                return {
                    center: { x: c.circle.cx(), y: c.circle.cy() },
                    size: c.circle.width()
                }
            }),
            cameras: cameras.filter(c => c.persist).map(c => {
                return {
                    points: c.polygon.array().map(ap => {
                        return {
                            x: ap[0],
                            y: ap[1]
                        }
                    }),
                    transformMatrix: c.controlGroup.transform()
                }
            })
        }

        componentReference.invokeMethodAsync("SaveMapOverlay", overlay)
            .then(() => {
                document.querySelector(rootSelector + " .saveBtn").removeAttribute('disabled')
            })
    }

    function resetActiveBtns() {
        var btns = document.querySelectorAll(rootSelector + ' .buttons-container .btn');
        for (var i = 0; i < btns.length; i++) {
            btns[i].classList.remove("active");
        }
    }

    let currentControlDrawing;
    function addCircleTool() {
        if (currentControlDrawing) {
            currentControlDrawing.cancelDraw()
        }

        resetActiveBtns();

        let newOverlayEntry = {
            persist: true
        };

        circle = initializeCircle(newOverlayEntry, true);
        currentControlDrawing = circle

        document.querySelector(rootSelector + " .circleToolBtn").classList.add("active")

        circle.on('drawstop', function (ev) {
            currentControlDrawing = null
            resetActiveBtns();
        });
    }

    function addAreaTool() {
        if (currentControlDrawing) {
            currentControlDrawing.cancelDraw();
        }
        resetActiveBtns();

        let newOverlayEntry = {
            persist: true
        };

        zone = initializeZone(newOverlayEntry, true);
        currentControlDrawing = zone

        document.querySelector(rootSelector + " .areaToolBtn").classList.add("active")

        zone.on('drawstop', function (ev) {
            currentControlDrawing = null
            resetActiveBtns()
        });
    }

    function addCameraTool() {
        resetActiveBtns();

        let newOverlayEntry = {
            persist: true
        };

        initializeCamera(newOverlayEntry, true);
    }

    function getCameraAttr() {
        return {
            ...shapeCommonAttr,
            fill: 'green'
        }
    }

    function getZoneAttr() {
        return shapeCommonAttr;
    }

    function getCircleAttr() {
        let attr = {
            ...shapeCommonAttr,
            fill: 'yellow',
            'fill-opacity':1
        }

        return attr;
    }

    let updateOverlay = function (updateDetails, persist) {
        let updateHandlers = {}

        updateHandlers.circle = function (updateDetails) {
            switch (updateDetails.type) {
                case 'addOrUpdate':
                    if (circles.find(c => c.id == updateDetails.overlayEntry.id)) {
                        updateCircle(updateDetails.overlayEntry);
                    }
                    else {
                        initializeCircle(updateDetails.overlayEntry)
                    }
                    break;
            }
        }

        updateDetails.overlayEntry = JSON.parse(updateDetails.overlayEntry)
        updateDetails.overlayEntry.persist = persist

        updateHandlers[updateDetails.entryType](updateDetails)
    }

    initializeMapOverlay(mapOverlay)

    if (!isReadOnly) {
        addPanelButtonsEventListeners();
    }

    return {
        update: updateOverlay
    }
}