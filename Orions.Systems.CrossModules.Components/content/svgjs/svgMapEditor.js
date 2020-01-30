window.Orions.SvgMapEditor = {
	init: function (rootElementId, componentReference, cameras) {
        var doc = document;
        var rootSelector = '#' + rootElementId;
        var svg = doc.querySelector(rootSelector + ' .map-container svg');
        var draw = SVG(svg);
        

        var shapeCommonAttr = {
            fill: 'red',
            'fill-opacity': 0.5,
            'stroke-width': 0.5,
            'stroke': '#6e6e6e'
        }

        addPanelButtonsEventListeners();

        function addPanelButtonsEventListeners() {
            document.querySelector(rootSelector + " .circleToolBtn")
                .addEventListener("click", selectCircleTool);

            document.querySelector(rootSelector + " .areaToolBtn")
                .addEventListener("click", selectAreaTool);

            document.querySelector(rootSelector + " .cameraToolBtn")
                .addEventListener("click", addCameraTool);
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

            circle = new SvgToolbox.CircleZone(draw, shapeCommonAttr)
            circle.draw();
            circle.on('drawstop', function (ev) {
                resetActiveBtns()
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

            zone = new SvgToolbox.Zone(draw, shapeCommonAttr);
            zone.draw();
            zone.on('drawstop', function (ev) {
                resetActiveBtns()
            });
        }

        function addCameraTool() {
            resetActiveBtns();

            var camInst = new SvgToolbox.Camera(draw, shapeCommonAttr);
            camInst.draw();

            camInst.setAttr({ fill: 'green' })
        }
	}
} 