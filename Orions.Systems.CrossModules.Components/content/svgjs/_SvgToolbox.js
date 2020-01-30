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

    function addSelectionEventListener(controlGroup, control, controlInstance) {
        window.addEventListener('click', function (ev) {
            let editControls = controlGroup.node.querySelectorAll('[mapObjectType="edit-control"]');
            if (ev.target != control.node && isNodeDescendant(controlGroup.node, ev.target) == false) {
                for (let i = 0; i < editControls.length; i++) {
                    editControls[i].setAttribute("style", "visibility: collapse");
                }

                controlInstance.isSelected = false
            }
            else {
                for (let i = 0; i < editControls.length; i++) {
                    editControls[i].setAttribute("style", "visibility: visible");
                }

                controlInstance.isSelected = true
            }
        })
    }

    function addDeletionEventListener(controlInstance) {
        document.addEventListener('keydown', function (ev) {
            if (ev.keyCode == 46 && controlInstance.isSelected) {
                controlInstance.remove()
            }
        })
    }

    function isNodeDescendant(parent, child) {
        var node = child.parentNode;
        while (node != null) {
            if (node == parent) {
                return true;
            }
            node = node.parentNode;
        }
        return false;
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

    class BaseControl {
        setAttr(attr) {
            this.attr = {
                ...this.attr,
                ...attr,
            };

            this.mainShape.attr(attr);
        }

        remove() {
            this.controlGroup.remove()
        }
    }

    class Camera extends BaseControl {
        constructor(svgNode, attr) {
            super()
            this.svgNode = svgNode;
            this.attr = attr || {}
        }

        draw() {
            this.cameraPolygon = this.svgNode.polygon('5,25 10,55 35,55 40,25').x(100).y(100).attr(this.attr);
            this.initCameraControls(this.cameraPolygon);
        }

        move(x, y) {
            this.cameraControlsGroup.dx(x).dy(y)
        }

        rotate(angle) {
            var rotationCenter = getMiddlePoint(this.cameraPolygon.array()[1], this.cameraPolygon.array()[2])
            this.cameraControlsGroup.rotate(angle, rotationCenter[0], rotationCenter[1])
        }

        initCameraControls(camera) {
            const { svgNode } = this;

            let controlGroup = svgNode.group();
            controlGroup.add(camera);

            let self = this;
            let rotateControl;
            let rotationPointControl;

            function initRotateControl() {
                var perpend = getPerpendicular(camera.array()[1], camera.array()[2], svgNode.bbox());
                rotateControl = svgNode.circle(6).move(perpend[0] - 3, perpend[1] - 3).fill('red').attr({ class: 'shape-control' });
                rotateControl.attr({ mapObjectType: 'edit-control' })

                var rotationPoint = getMiddlePoint(camera.array()[1], camera.array()[2]);
                rotationPointControl = svgNode.circle(6).move(rotationPoint[0] - 3, rotationPoint[1] - 3).fill('red').attr({ class: 'shape-control' });
                rotationPointControl.attr({ mapObjectType: 'edit-control' })
                controlGroup.add(rotationPointControl)

                rotateControl.on('mousedown', function (ev) {
                    controlGroup.draggable(false)

                    //let prevRotateControlPos = getSvgPoint(ev.clientX, ev.clientY, self.cameraControlsGroup.node)
                    let prevRotateControlPos = getSvgPoint(ev.clientX, ev.clientY, self.svgNode.node)
                    console.log(prevRotateControlPos)

                    // let line
                    let mouseMove = function (ev, isMouseDown) {
                        var rotationCenter = getMiddlePoint(camera.array()[1], camera.array()[2])
                        let p = new SVG.Point(rotationCenter[0], rotationCenter[1]);
                        p = p.transform(self.cameraControlsGroup.transform());
                        let transformedCenter = [p.x, p.y]

                        var currentMouseSvgPoint = getSvgPoint(ev.clientX, ev.clientY, self.svgNode.node);

                        var oldPoint = transposeCoords(prevRotateControlPos, self.svgNode.node.viewBox.baseVal)
                        var newPoint = transposeCoords([currentMouseSvgPoint.x, currentMouseSvgPoint.y], self.svgNode.node.viewBox.baseVal)
                        var cameraPointTransposed = transposeCoords(transformedCenter, self.svgNode.node.viewBox.baseVal)


                        // if (line) line.remove()
                        // line = self.svgNode.line(transformedCenter[0], transformedCenter[1], prevRotateControlPos[0], prevRotateControlPos[1]).stroke({ color: '#f06', width: 1, linecap: 'round' })

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
                resizeControl.attr({ mapObjectType: 'edit-control' })
                resizeControl.draggable();
                controlGroup.add(resizeControl);
                resizeControl.on('dragmove', function (ev) {
                    let vIndex = i;
                    let newVertArray = camera.array();
                    newVertArray[vIndex] = [ev.detail.box.x + 2, ev.detail.box.y + 2];
                    camera.plot(newVertArray);
                })

                resizeControl.on('dragend', function () {
                    var perpend = getPerpendicular(camera.array()[1], camera.array()[2], svgNode.bbox());
                    rotateControl.move(perpend[0] - 3, perpend[1] - 3);

                    var rotationPoint = getMiddlePoint(camera.array()[1], camera.array()[2]);
                    rotationPointControl.move(rotationPoint[0] - 3, rotationPoint[1] - 3);
                })
            }

            controlGroup.draggable();
            controlGroup.add(rotateControl);

            this.cameraControlsGroup = controlGroup
            this.controlGroup = controlGroup;
            this.mainShape = camera;

            addSelectionEventListener(controlGroup, camera, self)
            addDeletionEventListener(self);
        }
    }

    class Zone extends BaseControl {
        constructor(svgNode, attr) {
            super()
            this.svgNode = svgNode;
            this.attr = attr || {}
        }

        cancelDraw() {
            this.polygon.draw('cancel')
        }

        on(eventName, callback) {
            this.polygon.on(eventName, callback)
        }

        draw() {
            let self = this;
            this.polygon = this.svgNode.polygon().attr(this.attr).draw();
            let polygon = this.polygon;

            function drawEndEventListener(e) {
                if (e.keyCode == 13) {
                    self.polygon.draw('done');
                    self.polygon.off('drawstart');
                }
            }
            polygon.on('drawstart', function (e) {
                document.addEventListener('keydown', drawEndEventListener);
            });

            polygon.on('drawstop', function () {
                document.removeEventListener('keydown', drawEndEventListener)
                polygon.draw('done')

                let controlGroup = self.svgNode.group();
                controlGroup.add(polygon)

                var verticesArr = polygon.array();
                for (let i = 0; i < verticesArr.length; i++) {
                    let vertex = verticesArr[i]
                    let resizeControl = self.svgNode.rect(4, 4).move(vertex[0] - 2, vertex[1] - 2).attr({ 'stroke-width': 0.5, 'stroke': '#6e6e6e', fill: 'white' });
                    resizeControl.attr({ mapObjectType: 'edit-control' })
                    resizeControl.draggable();
                    controlGroup.add(resizeControl);
                    resizeControl.on('dragmove', function (ev) {
                        let vIndex = i;
                        let newVertArray = polygon.array();
                        newVertArray[vIndex] = [ev.detail.box.x + 2, ev.detail.box.y + 2];
                        polygon.plot(newVertArray);
                    })
                }

                controlGroup.draggable();
                self.controlGroup = controlGroup;

                addSelectionEventListener(controlGroup, polygon, self)
                addDeletionEventListener(self);
            });
        }
    }

    class CircleZone extends BaseControl {
        constructor(svgNode, attr) {
            super()
            this.svgNode = svgNode;
            this.attr = attr || {}
        }

        remove() {
            this.controlGroup.remove()
        }

        cancelDraw() {
            this.circle.draw('cancel')
        }

        on(eventName, callback) {
            this.circle.on(eventName, callback)
        }

        draw() {
            //let circle = this.svgNode.circle(10).attr(this.attr).draw();
            let self = this;
            let circle = this.svgNode.circle().attr(this.attr).draw();
            this.circle = circle;

            this.mainShape = circle;

            circle.on('drawstop', function () {
                self.isSelected = true;
                let controlGroup = self.svgNode.group()
                controlGroup.add(circle);

                let r = circle.rx()
                var verticesArr = [[circle.cx() - r, circle.cy()], [circle.cx(), circle.cy() - r], [circle.cx() + r, circle.cy()], [circle.cx(), circle.cy() + r]]
                let resizeControls = []
                for (let i = 0; i < verticesArr.length; i++) {
                    let vertex = verticesArr[i]
                    let resizeControl = self.svgNode.rect(4, 4).move(vertex[0] - 2, vertex[1] - 2).attr({ 'stroke-width': 0.5, 'stroke': '#6e6e6e', fill: 'white' });
                    resizeControl.attr({ mapObjectType: 'edit-control' })

                    controlGroup.add(resizeControl);
                    resizeControls.push(resizeControl)

                    resizeControl.on('mousedown', function (ev) {
                        controlGroup.draggable(false)

                        let prevPos = getSvgPoint(ev.clientX, ev.clientY, self.svgNode.node)
                        let mouseMove = function (ev, isMouseDown) {
                            let currentPos = getSvgPoint(ev.clientX, ev.clientY, self.svgNode.node)

                            let radiusChange
                            if (i == 0)
                                radiusChange = prevPos.x - currentPos.x
                            else if (i == 2)
                                radiusChange = currentPos.x - prevPos.x
                            else if (i == 3)
                                radiusChange = currentPos.y - prevPos.y
                            else {
                                radiusChange = prevPos.y - currentPos.y
                            }
                            circle.radius(circle.rx() + radiusChange)
                            prevPos = currentPos

                            resizeControls[0].dx(-radiusChange)
                            resizeControls[1].dy(-radiusChange)
                            resizeControls[2].dx(radiusChange)
                            resizeControls[3].dy(radiusChange)
                        }
                        window.addEventListener('mousemove', mouseMove)

                        let mouseUpListener = function () {
                            window.removeEventListener('mousemove', mouseMove)
                            window.removeEventListener('mouseup', mouseUpListener)
                            controlGroup.draggable(true)
                        }
                        window.addEventListener('mouseup', mouseUpListener)
                    })
                }

                controlGroup.draggable()
                self.controlGroup = controlGroup

                addSelectionEventListener(controlGroup, circle, self)

                addDeletionEventListener(self);
            })
        }
    }

    window.SvgToolbox = {
        Camera,
        Zone,
        CircleZone
    }
})();
