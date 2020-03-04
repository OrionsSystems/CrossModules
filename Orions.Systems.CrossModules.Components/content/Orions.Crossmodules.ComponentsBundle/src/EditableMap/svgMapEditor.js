import { SVG } from '@svgdotjs/svg.js'
import * as SvgToolbox from './SvgToolbox.js'
import $ from 'jquery';
import 'jquery-ui/ui/widgets/draggable';
import svgPanZoom from 'svg-pan-zoom'

window.Orions.SvgMapEditor = {
    layoutEditors: {},

    init: function (rootElementId, componentReference, mapOverlay, config, editorEventHandlers) {
        let layoutEditor = SvgMapEditor(rootElementId, componentReference, mapOverlay, config, editorEventHandlers);

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

    makePopupDraggable: function(rootElementId) {
        $('#' + rootElementId + ' .draggable-popup .modal-dialog').draggable()
    },

    destroy: function (rootElementId) {
        let editorToDestroy = this.layoutEditors[rootElementId];
        editorToDestroy.destroy();
    }
}

function SvgMapEditor(rootElementId, componentReference, mapOverlay, config, editorEventHandlers) {
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
        dblClickZoomEnabled: false
    })

    var draw = SVG(rootSelector + ' .svg-pan-zoom_viewport');
    let zonesLayer = draw.group();
    let camerasLayer = draw.group();
    let circlesLayer = draw.group();
    let shapeControlsLayer = draw.group();

    let globalClickEventHandler = () => {
        document.querySelector(rootSelector + " .heatmapBtn").setAttribute('disabled', 'disabled')
        //document.querySelector(rootSelector + " .realMasksMapBtn").setAttribute('disabled', 'disabled')
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
            let eventHandlerInfo = circleOverlayEntry.eventHandlerMappings[key]
            newCircle.on(key, function (e) {
                let svgEvent = { clientX: e.clientX, clientY: e.clientY}
                
                componentReference.invokeMethodAsync(eventHandlerInfo.componentMethodName, newCircle.overlayEntry, svgEvent)
            }, eventHandlerInfo.stopPropagation)
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

        newZone.on('controlDeleted',(z) => {
            zones.splice(zones.findIndex(el => el == z), 1)
        })

        for (var key in zoneOverlayEntry.eventHandlerMappings) {
            let eventName = key
            let eventHandlerInfo = zoneOverlayEntry.eventHandlerMappings[key]
            newZone.on(eventName, function (e) {
                let svgEvent = { clientX: e.clientX, clientY: e.clientY }
                
                componentReference.invokeMethodAsync(eventHandlerInfo.componentMethodName, newZone.overlayEntry, svgEvent)
            }, eventHandlerInfo.stopPropagation)
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
            .then(zoneOverlayEntry => {
                newZoneControl.overlayEntry = zoneOverlayEntry

                let eventHandlerInfo = zoneOverlayEntry.eventHandlerMappings[eventName]

                for (var key in zoneOverlayEntry.eventHandlerMappings) {
                    let eventName = key
                    newZoneControl.on(eventName, function (e) {
                        let svgEvent = { clientX: e.clientX, clientY: e.clientY }

                        componentReference.invokeMethodAsync(eventHandlerInfo.componentMethodName, newZoneControl.overlayEntry, svgEvent)
                    }, eventHandlerInfo.stopPropagation)
                }
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

    function destroyEditor() {
        document.removeEventListener('click', globalClickEventHandler)
    }

    return {
        update: updateOverlay,
        destroy: destroyEditor,
        addCircleTool: addCircleTool,
        addAreaTool: addAreaTool,
        addCameraTool: addCameraTool
    }
}