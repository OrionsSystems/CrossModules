window.Orions.SvgMapEditor = {
    init: function (rootElementId, componentReference, mapOverlay) {
        var doc = document;
        var rootSelector = '#' + rootElementId;
        var svg = doc.querySelector(rootSelector + ' .map-container svg');
        var draw = SVG(svg);

        let zones = []
        let cameras = []
        let circles = []

        var shapeCommonAttr = {
            fill: 'red',
            'fill-opacity': 0.5,
            'stroke-width': 0.5,
            'stroke': '#6e6e6e'
        }

        initializeMapOverlay(mapOverlay)

        function initializeMapOverlay(overlay) {
            let initializeZone = function (zone) {
                let newZone = new SvgToolbox.Zone({ svgNode: draw, attr: shapeCommonAttr, points: zone.points, startUserDrawing: false, name:zone.name });
                setZoneAttr(newZone);

                zones.push(newZone)
            }

            if (overlay.zones) {
                overlay.zones.forEach((item) => {
                    initializeZone(item)
                })
            }


            let initializeCircle = function (circle) {
                let newCircle = new SvgToolbox.CircleZone({ svgNode: draw, attr: shapeCommonAttr, center: circle.center, size: circle.size, startUserDrawing: false });
                setCircleAttr(newCircle);

                circles.push(newCircle)
            }

            if (overlay.circles) {
                overlay.circles.forEach((item) => {
                      initializeCircle(item)
                })
            }

            let initializeCamera = function (camera) {
                let newCamera = new SvgToolbox.Camera({ svgNode: draw, attr: shapeCommonAttr, points: camera.points, transformMatrix: camera.transformMatrix });
                setCameraAttr(newCamera);

                cameras.push(newCamera)
            }

            if (overlay.cameras) {
                overlay.cameras.forEach((item) => {
                    initializeCamera(item)
                })
            }
        }

        addPanelButtonsEventListeners();

        function addPanelButtonsEventListeners() {
            document.querySelector(rootSelector + " .circleToolBtn")
                .addEventListener("click", selectCircleTool);

            document.querySelector(rootSelector + " .areaToolBtn")
                .addEventListener("click", selectAreaTool);

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
                zones: zones.map(z => {
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
                circles: circles.map(c => {
                    return {
                        center: { x: c.circle.cx(), y: c.circle.cy() },
                        size: c.circle.width()
                    }
                }),
                cameras: cameras.map(c => {
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

        let circle;
        function selectCircleTool() {
            if (circle) {
                circle.cancelDraw()
            }

            resetActiveBtns();

            circle = new SvgToolbox.CircleZone({ svgNode: draw, attr: shapeCommonAttr, startUserDrawing: true })
            circles.push(circle)
            circle.on('drawstop', function (ev) {
                resetActiveBtns();
            });

            circle.setAttr({ fill: 'yellow' })


            document.querySelector(rootSelector + " .circleToolBtn").classList.add("active")
        }

        let zone;
        function selectAreaTool() {
            if (zone) {
                zone.cancelDraw();
            }
            resetActiveBtns();

            document.querySelector(rootSelector + " .areaToolBtn").classList.add("active")

            zone = new SvgToolbox.Zone({ svgNode: draw, startUserDrawing: true });
            zones.push(zone);
            setZoneAttr(zone);

            zone.on('drawstop', function (ev) {
                resetActiveBtns()
            });
        }

        function addCameraTool() {
            resetActiveBtns();

            let camera = new SvgToolbox.Camera({ svgNode: draw, attr: shapeCommonAttr, isDefaultPosition: true });
            cameras.push(camera);
            setCameraAttr(camera);
        }

        function setCameraAttr(camera) {
            camera.setAttr({
                ...shapeCommonAttr,
                fill: 'green'
            })
        }

        function setZoneAttr(zone) {
            zone.setAttr(shapeCommonAttr);
        }

        function setCircleAttr(circle) {
            let attr = {
                ...shapeCommonAttr,
                fill: 'yellow',
            }
            circle.setAttr(attr);
        }
	}
} 