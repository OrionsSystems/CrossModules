(function () {
    function getPerpendicular(p1, p2, box) {
        p1 = transposeCoords(p1, box);
        p2 = transposeCoords(p2, box);

        var point = getMiddlePoint(p1, p2);
        var length = 60;

        var tan = (p1[0] - p2[0]) / (p1[1] - p2[1]);
        var xLen = length / Math.sqrt(1 + tan * tan);
        var yLen = Math.sqrt(length * length - xLen * xLen);

        point[0] = point[0] + (xLen * (tan < 0 ? 1 : -1) * (p2[0] > p1[0] ? 1 : -1));
        point[1] = point[1] + yLen * (p2[0] > p1[0] ? 1 : -1);

        point = transposeCoordsBack(point, box);

        return point;
    }

    function cursorPoint(evt) {
        pt.x = evt.clientX; pt.y = evt.clientY;
        return pt.matrixTransform(svg.getScreenCTM().inverse());
    }

    function transposeCoords(p, box) {
        return [p[0], box.height - p[1]]
    }

    function transposeCoordsBack(p, box) {
        return [p[0], box.height - p[1]]
    }

    function getMiddlePoint(p1, p2) {
        return [(p1[0] + p2[0]) / 2, (p1[1] + p2[1]) / 2]
    }

    function getSvgPoint(clientX, clientY, box) {
        let pt = box.createSVGPoint();
        pt.x = clientX;
        pt.y = clientY;
        let svgP = pt.matrixTransform(box.getScreenCTM().inverse());

        return svgP;
    }

    class Camera {
        constructor(svgNode, attr) {
            this.svgNode = svgNode;
            this.attr = attr || {}
        }

        draw() {
            this.cameraPolygon = this.svgNode.polygon('5,25 10,55 35,55 40,25').x(100).y(100).attr(this.attr);
            this.initCameraControls(this.cameraPolygon);
        }

        initCameraControls(camera) {
            const { svgNode } = this;

            let controlGroup = svgNode.group();
            controlGroup.add(camera);

            let self = this;
            let rotateControl;
            function initRotateControl() {
                var perpend = getPerpendicular(camera.array()[1], camera.array()[2], svgNode.bbox());
                rotateControl = svgNode.circle(6).move(perpend[0] - 3, perpend[1] - 3).fill('red').attr({ class: 'shape-control' });

                rotateControl.on('mousedown', function (ev) {
                    controlGroup.draggable(false)

                    let prevRotateControlPos = getSvgPoint(ev.clientX, ev.clientY, self.svgNode.node)

                    let mouseMove = function (ev, isMouseDown) {
                        var rotationCenter = getMiddlePoint(camera.array()[1], camera.array()[2])

                        var currentMouseSvgPoint = getSvgPoint(ev.clientX, ev.clientY, self.svgNode.node);

                        var oldPoint = transposeCoords(prevRotateControlPos, self.svgNode.node.viewBox.baseVal)
                        var newPoint = transposeCoords([currentMouseSvgPoint.x, currentMouseSvgPoint.y], self.svgNode.node.viewBox.baseVal)
                        var cameraPointTransposed = transposeCoords(rotationCenter, self.svgNode.node.viewBox.baseVal)

                        var newAngle = Math.atan((newPoint[0] - cameraPointTransposed[0]) / (newPoint[1] - cameraPointTransposed[1]));
                        var oldAngle = Math.atan((oldPoint[0] - cameraPointTransposed[0]) / (oldPoint[1] - cameraPointTransposed[1]));

                        oldAngle = oldAngle * 180 / Math.PI;
                        newAngle = newAngle * 180 / Math.PI;

                        if (newPoint[1] < cameraPointTransposed[1]) {
                            newAngle = newAngle - 180;
                        }
                        if (oldPoint[1] < cameraPointTransposed[1]) {
                            oldAngle = oldAngle - 180;
                        }

                        controlGroup.rotate(newAngle - oldAngle, rotationCenter[0], rotationCenter[1])

                        prevRotateControlPos = [currentMouseSvgPoint.x, currentMouseSvgPoint.y]
                    }
                    mouseMove(ev, true);

                    window.addEventListener('mousemove', mouseMove)

                    let mouseUpListener = function () {
                        window.removeEventListener('mousemove', mouseMove)
                        window.removeEventListener('mouseup', mouseUpListener)
                        controlGroup.draggable(true)
                    }
                    window.addEventListener('mouseup', mouseUpListener)
                })
            }

            initRotateControl();

            var verticesArr = camera.array();
            for (let i = 0; i < verticesArr.length; i++) {
                let vertex = verticesArr[i]
                let resizeControl = svgNode.rect(4, 4).move(vertex[0] - 2, vertex[1] - 2).attr({ 'stroke-width': 0.5, 'stroke': '#6e6e6e', fill: 'white' });
                resizeControl.draggable();
                controlGroup.add(resizeControl);
                resizeControl.on('dragmove', function (ev) {
                    let vIndex = i;
                    let newVertArray = camera.array();
                    newVertArray[vIndex] = [ev.detail.box.x + 2, ev.detail.box.y + 2];
                    camera.plot(newVertArray);
                })
            }

            controlGroup.draggable();
            controlGroup.add(rotateControl);
            controlGroup.on("dragstart", function (ev) {
                // rotateControl.remove()
            })

            controlGroup.on("dragend", function (ev) {
                // initRotateControl()
            })

            this.cameraControlsGroup = controlGroup
        }

        setAttr(attr) {
            this.attr = {
                ...this.attr,
                ...attr,
            };

            this.cameraPolygon.attr(attr);
        }
    }

    class Zone {
        constructor(svgNode, attr) {
            this.svgNode = svgNode;
            this.attr = attr || {}
        }

        draw() {
            let self = this;
            this.polygon = this.svgNode.polygon().attr(this.attr).draw();

            this.polygon.on('drawstart', function (e) {
                document.addEventListener('keydown', function (e) {
                    if (e.keyCode == 13) {
                        self.polygon.draw('done');
                        self.polygon.off('drawstart');
                    }
                });
            });

            this.polygon.on('drawstop', function () {
                // remove listener
            });

            this.polygon.draggable();
        }
    }

    class CircleZone {
        constructor(svgNode, attr) {
            this.svgNode = svgNode;
            this.attr = attr || {}
        }

        draw() {

            this.circle = this.svgNode.circle().attr(this.attr).draw();
            this.circle.draggable();
        }
    }

    window.SvgToolbox = {
        Camera,
        Zone,
        CircleZone
    }
})();
