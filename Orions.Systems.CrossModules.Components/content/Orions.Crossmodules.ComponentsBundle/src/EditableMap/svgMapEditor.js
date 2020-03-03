import { SVG } from '@svgdotjs/svg.js'
import * as SvgToolbox from './SvgToolbox.js'
import $ from 'jquery';
import 'jquery-ui/ui/widgets/draggable';
import svgPanZoom from 'svg-pan-zoom'

window.Orions.SvgMapEditor = {
    layoutEditors: {},

    init: function (rootElementId, componentReference, mapOverlay, config) {
        let layoutEditor = SvgMapEditor(rootElementId, componentReference, mapOverlay, config);

        this.layoutEditors[rootElementId] = layoutEditor;
    },

    update: function (rootElementId, updateDetails, persist, mode) {
        let layoutEditor = this.layoutEditors[rootElementId];

        layoutEditor.update(updateDetails, persist, mode)
    },

    addCircleTool: function (rootElementId) {
        let layoutEditor = this.layoutEditors[rootElementId];

        layoutEditor.addCircleTool()
    },

    addAreaTool: function (rootElementId) {
        let layoutEditor = this.layoutEditors[rootElementId];

        layoutEditor.addAreaTool()
    },

    addCameraTool: function (rootElementId) {
        let layoutEditor = this.layoutEditors[rootElementId];

        layoutEditor.addCameraTool()
    },

    saveMapOverlay: function (rootElementId) {
        let layoutEditor = this.layoutEditors[rootElementId];

        layoutEditor.saveMapOverlay()
    },

    makePopupDraggable: function(rootElementId) {
        $('#' + rootElementId + ' .draggable-popup .modal-dialog').draggable()
    },

    destroy: function (rootElementId) {
        let editorToDestroy = this.layoutEditors[rootElementId];
        editorToDestroy.destroy();
    }
}

function SvgMapEditor(rootElementId, componentReference, mapOverlay, config) {
    let { isReadOnly, zoneColor, circleColor, cameraColor } = config;

    var rootSelector = '#' + rootElementId;
    var svg = document.querySelector(rootSelector + ' .map-container svg');


    let onZoomHandler = function (newZoomScale) {
        zones.forEach(z => z.handleZoom(newZoomScale))
    }

    let panZoomCtrl = svgPanZoom(svg, {
        zoomScaleSensitivity: 0.4,
        controlIconsEnabled: true,
        onZoom: onZoomHandler,
        dblClickZoomEnabled: false,
        panEnabled: !isReadOnly
    })

    var draw = SVG(rootSelector + ' .svg-pan-zoom_viewport');
    let zonesLayer = draw.group();
    let camerasLayer = draw.group();
    let circlesLayer = draw.group();
    let shapeControlsLayer = draw.group();

    let globalClickEventHandler = () => {
        document.querySelector(rootSelector + " .heatmapBtn").setAttribute('disabled', 'disabled')
        document.querySelector(rootSelector + " .realMasksMapBtn").setAttribute('disabled', 'disabled')
        componentReference.invokeMethodAsync("CloseHyperTagInfoPopup")
    }

    document.addEventListener('click', globalClickEventHandler)

    let zones = []
    let cameras = []
    let circles = []

    var shapeCommonAttr = {
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
            overlayEntry: circleOverlayEntry,
            isReadOnly,
            isDraggable: circleOverlayEntry.isDraggable ?? true,
            isSelectable: circleOverlayEntry.isSelectable ?? true,
            shapeControlsLayer
        });
        newCircle.persist = circleOverlayEntry.persist;

        circles.push(newCircle);

        for (var key in circleOverlayEntry.eventHandlerMappings) {
            newCircle.on(key, function (e) {
                let svgEvent = { clientX: e.clientX, clientY: e.clientY}

                componentReference.invokeMethodAsync(circleOverlayEntry.eventHandlerMappings[key], newCircle.overlayEntry, svgEvent)
            })
        }

        newCircle.onRemove((c) => {
            circles.splice(circles.findIndex(el => el == c), 1)
        })

        return newCircle;
    }

    let initializeZone = function (zoneOverlayEntry, startUserDrawing) {
        let newZone = new SvgToolbox.Zone({
            svgRoot: draw,
            svgNode: zonesLayer,
            attr: getZoneAttr(zoneOverlayEntry),
            points: zoneOverlayEntry.points,
            startUserDrawing: startUserDrawing,
            name: zoneOverlayEntry.name,
            overlayEntry: zoneOverlayEntry,
            isReadOnly,
            isDraggable: zoneOverlayEntry.isDraggable ?? true,
            isSelectable: zoneOverlayEntry.isSelectable ?? true,
            maxPointsNumber: 4,
            shapeControlsLayer
        });
        newZone.persist = zoneOverlayEntry.persist;

        zones.push(newZone)

        newZone.onRemove((z) => {
            zones.splice(zones.findIndex(el => el == z), 1)
        })

        for (var key in zoneOverlayEntry.eventHandlerMappings) {
            let eventName = key
            newZone.on(eventName, function (e) {
                let svgEvent = { clientX: e.clientX, clientY: e.clientY }

                componentReference.invokeMethodAsync(zoneOverlayEntry.eventHandlerMappings[eventName], newZone.overlayEntry, svgEvent)
            })
        }

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
            overlayEntry: cameraOverlayEntry,
            isReadOnly,
            isDraggable: cameraOverlayEntry.isDraggable ?? true,
            isSelectable: cameraOverlayEntry.isSelectable ?? true,
            shapeControlsLayer
        });

        newCamera.persist = cameraOverlayEntry.persist;

        cameras.push(newCamera)

        newCamera.onRemove((c) => {
            cameras.splice(cameras.findIndex(el => el == c), 1)
        })

        return newCamera;
    }

    let createControlFromOverlayEntry = function(entry){
        switch (entry.entryType) {
            case "circle":
                initializeCircle(entry, false)
                break;
            case "zone":
                initializeZone(entry, false)
                break;
            case "camera":
                initializeCamera(entry, false)
                break;
        }
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
                item.persist = true
                initializeCamera(item)
            })
        }
    }

    function addPanelButtonsEventListeners() {
        //if (!isReadOnly) {
        //    document.querySelector(rootSelector + " .circleToolBtn")
        //        .addEventListener("click", addCircleTool);

        //    document.querySelector(rootSelector + " .areaToolBtn")
        //        .addEventListener("click", addAreaTool);

        //    document.querySelector(rootSelector + " .cameraToolBtn")
        //        .addEventListener("click", addCameraTool);

        //    document.querySelector(rootSelector + " .saveBtn")
        //        .addEventListener("click", saveMapOverlay);
        //}
    }

    function saveMapOverlay() {
        document.querySelector(rootSelector + " .saveBtn").setAttribute('disabled', 'disabled')

        let overlay = {
            id: mapOverlay.id,
            name: mapOverlay.name,
            zones: zones.filter(z => z.persist).map(z => {
                return {
                    id: z.overlayEntry.id,
                    name: z.name.get(),
                    points: z.polygon.array().map(ap => {
                        return {
                            x: ap[0],
                            y: ap[1]
                        }
                    }),
                    color: z.attr.fill,
                    alias: z.overlayEntry.alias,
                    fixedCameraEnhancementId: z.overlayEntry.fixedCameraEnhancementId == null ? null : z.overlayEntry.fixedCameraEnhancementId,
                    metadataSetId: z.overlayEntry.metadataSetId == null ? null : z.overlayEntry.metadataSetId,
                    view: z.overlayEntry.view
                }
            }),
            circles: circles.filter(c => c.persist).map(c => {
                return {
                    id: z.overlayEntry.id,
                    center: { x: c.circle.cx(), y: c.circle.cy() },
                    size: c.circle.width()
                }
            }),
            cameras: cameras.filter(c => c.persist).map(c => {
                return {
                    id: z.overlayEntry.id,
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
    let circle
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

    let zone
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

            addNewZoneBlazorSide(zone);
        });
    }

    function addNewZoneBlazorSide(newZoneControl){
        let newZoneOverlayEntry = {
            points: newZoneControl.polygon.array().map(ap => {
                return {
                    x: ap[0],
                    y: ap[1]
                }
            }),
            color: newZoneControl.attr.fill,
            name: newZoneControl.name.get()
        }

        componentReference.invokeMethodAsync("AddNewZoneToVm", newZoneOverlayEntry)
            .then(resp => {
                newZoneControl.overlayEntry = resp
            })
    }

    let addCameraTool = function() {
        resetActiveBtns();

        let newOverlayEntry = {
            persist: true
        };

        initializeCamera(newOverlayEntry, true);
    }

    function getCameraAttr() {
        return {
            ...shapeCommonAttr,
            fill: cameraColor
        }
    }

    function getZoneAttr(entry) {
        let attr = {
            ...shapeCommonAttr,
            fill: entry.color ? entry.color : zoneColor,
        }

        return attr;
    }

    function getCircleAttr() {
        let attr = {
            ...shapeCommonAttr,
            fill: circleColor,
            'fill-opacity': 1,
            'stroke-width': 0.1
        }

        return attr;
    }

    let updateOverlay = function (updateDetails, persist, mode) {
        if (mode == "batch") {
            updateDetails.forEach(d => {
                d.overlayEntry = JSON.parse(d.overlayEntry)
                d.overlayEntry.persist = persist
            })
        }
        else {
            updateDetails.overlayEntry = JSON.parse(updateDetails.overlayEntry)
            updateDetails.overlayEntry.persist = persist
        }
        

        let allControls = [...circles, ...zones, ...cameras]

        let updateDetailsFunc = details => {
            switch (details.type) {
                case 'addOrUpdate':
                    let controlToUpdate = allControls.find(c => c.overlayEntry.entryType == details.overlayEntry.entryType
                        && c.overlayEntry.id == details.overlayEntry.id
                        && c.overlayEntry.id != null)
                    if (controlToUpdate) {
                        controlToUpdate.updateFromOverlayEntry(details.overlayEntry);
                    }
                    else {
                        createControlFromOverlayEntry(details.overlayEntry)
                    }
                    break;
                case 'delete':
                    let controlToDelete = allControls.find(c => c.overlayEntry.entryType == details.overlayEntry.entryType
                        && c.overlayEntry.id == details.overlayEntry.id
                        && c.overlayEntry.id != null);
                    if (controlToDelete) {
                        controlToDelete.remove();
                    }
                    break;
            }
        }

        if (mode == 'batch') {
            updateDetails.forEach(e => {
                updateDetailsFunc(e)
            })
        }
        else {
            updateDetailsFunc(updateDetails)
        }
    }

    initializeMapOverlay(mapOverlay)

    addPanelButtonsEventListeners();

    function destroyEditor() {
        document.removeEventListener('click', globalClickEventHandler)
    }

    return {
        update: updateOverlay,
        destroy: destroyEditor,
        addCircleTool: addCircleTool,
        addAreaTool: addAreaTool,
        addCameraTool: addCameraTool,
        saveMapOverlay: saveMapOverlay
    }
}