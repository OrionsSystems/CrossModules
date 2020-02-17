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
            if (ev.target != control.node && isNodeDescendant(controlGroup.node, ev.target) == false) {
                controlInstance.select(false)
            }
            else {
                controlInstance.select(true)
                ev.stopPropagation();
            }
        })
    }

    function addDeletionEventListener(controlInstance) {
        document.addEventListener('keydown', function (ev) {
            let isEditingName = controlInstance.isEditingName ? controlInstance.isEditingName.get() : false
            if (ev.keyCode == 46 && controlInstance.isSelected && !isEditingName) {
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
        let pt = box.nearestViewportElement.createSVGPoint();
        pt.x = clientX;
        pt.y = clientY;
        let svgP = pt.matrixTransform(box.getScreenCTM().inverse());

        return svgP;
    }

    class ViewModelProperty{
        constructor(value){
            this.value = value;
            this.changeHandlers = []
        }

        onChange(callback){
            this.changeHandlers.push(callback)
        }

        set(newValue){
            let oldValue = this.value;
            this.changeHandlers.forEach(h => {
                h(oldValue, newValue)
            })

            this.value = newValue;
        }

        get(){
            return this.value;
        }
    }

    class BaseControl {
        constructor(isReadOnly, svgNode, overlayEntry, isSelectable) {
            let self = this;

            if (overlayEntry) {
                this.overlayEntry = overlayEntry;
            }
            else {
                this.overlayEntry = {}
            }

            this.isSelectable = isSelectable;
            this.onRemoveEventHandlers = []
            this.onDblClickEventHandlers = []
            this.onSelectEventHandlers = []
            this.isReadOnly = isReadOnly;
            this.svgNode = svgNode;
            this.controlGroup = this.svgNode.group();

            this.controlGroup.on('dblclick', function () {
                self.onDblClickEventHandlers.forEach(h => h(self))
            })
        }

        draggable(draggable) {
            this.controlGroup.draggable(draggable && !this.isReadOnly);
        }

        setAttr(attr) {
            this.attr = {
                ...this.attr,
                ...attr,
            };

            this.mainShape.attr(attr);
        }

        center(x, y, animate) {
            if (!animate) {
                this.controlGroup.center(x, y)
            }
            else {
                this.controlGroup.animate(500, 0, 'now').center(x, y)
            }
        }

        remove() {
            let self = this;
            this.controlGroup.remove()

            this.onRemoveEventHandlers.forEach(h => h(self))
        }

        onRemove(callback) {
            this.onRemoveEventHandlers.push(callback)
        }

        onDblClick(callback) {
            this.onDblClickEventHandlers.push(callback)
        }

        onSelect(callback) {
            this.onSelectEventHandlers.push(callback)
        }


        raiseOnSelect() {
            let self = this;
            this.onSelectEventHandlers.forEach(h => {
                h(self.overlayEntry);
            })
        }

        select(isSelected) {
            let self = this;
            let editControls = self.controlGroup.node.querySelectorAll('[mapObjectType="edit-control"]');
            if (!isSelected) {
                for (let i = 0; i < editControls.length; i++) {
                    editControls[i].setAttribute("style", "visibility: collapse");
                }

                self.isSelected = false

                if (self.isEditingName) {
                    self.isEditingName.set(false)
                }
            }
            else if (self.isSelectable){
                for (let i = 0; i < editControls.length; i++) {
                    editControls[i].setAttribute("style", "visibility: visible");
                }

                self.isSelected = true
                self.raiseOnSelect();
            }
        }
    }

    class Camera extends BaseControl {
        constructor({ svgRoot, svgNode, attr, isDefaultPosition, points, transformMatrix, isReadOnly, overlayEntry, isSelectable }) {
            super(isReadOnly, svgNode, overlayEntry, isSelectable)
            
            this.attr = attr || {}

            if (isDefaultPosition) {
                let initialPoint = new SVG.Point(100, 100).transform(this.svgNode.node.getCTM().inverse())
                this.polygon = this.svgNode.polygon('5,25 10,55 35,55 40,25').x(initialPoint.x).y(initialPoint.y).attr(this.attr);
            }
            else {
                this.polygon = this.svgNode.polygon().attr(this.attr);
                this.polygon.plot(points.map(p => [p.x, p.y]))
            }
            this.initCameraControls();

            if (transformMatrix) {
                this.transform(transformMatrix)
            }
        }

        updateFromOverlayEntry(entry) {
            throw "Not implemented"
        }

        move(x, y) {
            this.cameraControlsGroup.dx(x).dy(y)
        }

        rotate(angle) {
            var rotationCenter = getMiddlePoint(this.polygon.array()[1], this.polygon.array()[2])
            this.cameraControlsGroup.rotate(angle, rotationCenter[0], rotationCenter[1])
        }

        transform(matrix) {
            this.controlGroup.transform(matrix)
        }

        initCameraControls() {
            let camera = this.polygon;
            const { svgNode } = this;

            this.controlGroup.add(camera);

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
                self.controlGroup.add(rotationPointControl)

                rotateControl.on('mousedown', function (ev) {
                    ev.stopPropagation();
                    this.draggable(false)

                    let prevRotateControlPos = getSvgPoint(ev.clientX, ev.clientY, self.svgNode.node)
                    
                    let mouseMove = function (ev, isMouseDown) {
                        var rotationCenter = getMiddlePoint(camera.array()[1], camera.array()[2])
                        let p = new SVG.Point(rotationCenter[0], rotationCenter[1]);
                        p = p.transform(self.cameraControlsGroup.transform());
                        let transformedCenter = [p.x, p.y]

                        var currentMouseSvgPoint = getSvgPoint(ev.clientX, ev.clientY, self.svgNode.node);

                        var oldPoint = transposeCoords(prevRotateControlPos, self.svgNode.node.nearestViewportElement.viewBox.baseVal)
                        var newPoint = transposeCoords([currentMouseSvgPoint.x, currentMouseSvgPoint.y], self.svgNode.node.nearestViewportElement.viewBox.baseVal)
                        var cameraPointTransposed = transposeCoords(transformedCenter, self.svgNode.node.nearestViewportElement.viewBox.baseVal)

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

                        self.controlGroup.rotate(newAngle - oldAngle, rotationCenter[0], rotationCenter[1])

                        prevRotateControlPos = [currentMouseSvgPoint.x, currentMouseSvgPoint.y]
                    }
                    mouseMove(ev, true);

                    window.addEventListener('mousemove', mouseMove)

                    let mouseUpListener = function () {
                        window.removeEventListener('mousemove', mouseMove)
                        window.removeEventListener('mouseup', mouseUpListener)
                        self.draggable(true)
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
                this.controlGroup.add(resizeControl);
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

            
            this.controlGroup.add(rotateControl);

            this.draggable(true);

            this.cameraControlsGroup = this.controlGroup
            this.mainShape = camera;

            addSelectionEventListener(this.controlGroup, camera, self)
            addDeletionEventListener(self);
        }
    }

    class Zone extends BaseControl {
        constructor({ svgRoot, svgNode, attr, points, startUserDrawing, name, overlayEntry, isReadOnly, isSelectable }) {
            super(isReadOnly, svgNode, overlayEntry, isSelectable)
            this.resizeControlWidth = 4
            this.attr = attr || {}
            this.isEditingName = new ViewModelProperty(false);
            this.name = new ViewModelProperty(name != undefined ? name : 'Zone Name')

            this.polygon = svgRoot.polygon().attr(this.attr);

            let self = this;
            if (startUserDrawing) {
                this.polygon.draw();

                function drawEndEventListener(e) {
                    if (e.keyCode == 13) {
                        self.polygon.draw('done');
                        self.polygon.off('drawstart');
                    }
                }
                this.polygon.on('drawstart', function (e) {
                    document.addEventListener('keydown', drawEndEventListener);
                });

                this.polygon.on('drawstop', function () {
                    self.polygon.addTo(self.svgNode)
                    document.removeEventListener('keydown', drawEndEventListener)
                    self.initZoneControls()
                });
            }
            else {
                if (points) {
                    self.polygon.addTo(self.svgNode)
                    this.polygon.plot(points.map(p => [p.x, p.y]))
                }
                this.initZoneControls()
            }

            this.mainShape = this.polygon;
        }

        cancelDraw() {
            this.polygon.draw('cancel');
            this.controlGroup.remove();
        }

        on(eventName, callback) {
            this.polygon.on(eventName, callback)
        }

        updateFromOverlayEntry(entry) {
            this.overlayEntry = entry;

            this.name.set(entry.name)
            this.attr.fill = entry.color;
            this.polygon.attr(this.attr)
        }

        initZoneControls() {
            let self = this;
            let polygon = self.polygon;

            this.controlGroup.node.classList.add('zone-control-group')
            this.controlGroup.node.classList.add('svg-control-group')

            this.controlGroup.add(polygon)

            // init resize controls
            var verticesArr = polygon.array();
            for (let i = 0; i < verticesArr.length; i++) {
                let vertex = verticesArr[i]
                let resizeControl = self.svgNode
                    .rect(self.resizeControlWidth, self.resizeControlWidth)
                    .move(vertex[0] - self.resizeControlWidth / 2, vertex[1] - self.resizeControlWidth / 2)
                    .attr({ 'stroke-width': 0.5, 'stroke': '#6e6e6e', fill: 'white' });
                resizeControl.attr({ mapObjectType: 'edit-control' })
                resizeControl.draggable();
                self.controlGroup.add(resizeControl);
                resizeControl.on('dragmove', function (ev) {
                    let vIndex = i;
                    let newVertArray = polygon.array();
                    newVertArray[vIndex] = [ev.detail.box.x + 2, ev.detail.box.y + 2];
                    polygon.plot(newVertArray);

                    polygon.fire('resize')
                })
            }

            // init text name control
            function initNameControl() {
                let zoneName = self.name.get();
                let nameLabel = self.svgNode.text(zoneName);
                let nameDrawPoint = { x: self.polygon.cx() - nameLabel.bbox().width / 2, y: self.polygon.cy() - nameLabel.bbox().height / 2 };
                nameLabel.move(nameDrawPoint.x, nameDrawPoint.y);
                let nameInput = self.svgNode.foreignObject(nameLabel.bbox().width, nameLabel.bbox().height).move(nameDrawPoint.x, nameDrawPoint.y)
                var htmlInput = document.createElement('input');
                htmlInput.setAttribute('type', 'text')
                htmlInput.setAttribute('value', zoneName)
                nameInput.attr('style', 'visibility:collapse');
                nameInput.add(htmlInput);
                self.name.onChange((oldValue, newValue) => {
                    nameLabel.text(newValue)
                })
                htmlInput.addEventListener('change', function(ev){
                    self.name.set(ev.target.value);
                });
                nameLabel.on('click', function(){
                    self.isEditingName.set(true);
                    nameLabel.attr('style', 'visibility:collapse');
                    nameInput.attr('style', 'visibility:visible');
                    htmlInput.setAttribute('style', 'background:none');
                    htmlInput.focus()
                })
                self.isEditingName.onChange((oldValue, newValue) =>{
                    if(newValue === false){
                        nameLabel.attr('style', 'visibility:visible');
                        nameInput.attr('style', 'visibility:collapse');
                    }
                })
                self.controlGroup.add(nameInput);
                self.controlGroup.add(nameLabel)

                self.polygon.on('resize', function (ev) {
                    let nameDrawPoint = { x: self.polygon.cx() - nameLabel.bbox().width / 2, y: self.polygon.cy() - nameLabel.bbox().height / 2 };
                    nameLabel.move(nameDrawPoint.x, nameDrawPoint.y);
                })
            }

            initNameControl();

            self.draggable(true);

            addSelectionEventListener(self.controlGroup, polygon, self)
            addDeletionEventListener(self);

            self.select(false);
        }
    }

    class CircleZone extends BaseControl {
        constructor({ svgRoot, svgNode, attr, center, size, startUserDrawing, overlayEntry, isReadOnly, isSelectable }) {
            super(isReadOnly, svgNode, overlayEntry, isSelectable)

            this.attr = attr || {}
            this.circle = svgRoot.circle().attr(this.attr);
            this.mainShape = this.circle;

            let self = this;
            if (startUserDrawing) {
                this.circle.draw();
                this.circle.on('drawstop', function () {
                    self.circle.addTo(svgNode);
                    self.initializeControls();
                })
            }
            else {
                self.circle.addTo(svgNode);
                this.circle.width(size);
                this.circle.center(center.x, center.y);
                this.initializeControls();
            }
        }

        updateFromOverlayEntry(entry) {
            this.center(entry.center.x, entry.center.y, true)
        }

        cancelDraw() {
            this.circle.draw('cancel')
            this.controlGroup.remove();
        }

        on(eventName, callback) {
            this.circle.on(eventName, callback)
        }

        initializeControls() {
            let self = this;
            let circle = self.circle;
            self.isSelected = true;
            self.controlGroup.add(circle);
            self.circle.node.classList.add('svg-control-group')
            self.circle.node.classList.add('cursor-pointer')

            let r = circle.rx()
            var verticesArr = [[circle.cx() - r, circle.cy()], [circle.cx(), circle.cy() - r], [circle.cx() + r, circle.cy()], [circle.cx(), circle.cy() + r]]
            let resizeControls = []
            for (let i = 0; i < verticesArr.length; i++) {
                let vertex = verticesArr[i]
                let resizeControl = self.svgNode.rect(4, 4).move(vertex[0] - 2, vertex[1] - 2).attr({ 'stroke-width': 0.5, 'stroke': '#6e6e6e', fill: 'white' });
                resizeControl.attr({ mapObjectType: 'edit-control' })

                self.controlGroup.add(resizeControl);
                resizeControls.push(resizeControl)

                resizeControl.on('mousedown', function (ev) {
                    ev.stopPropagation();
                    this.draggable(false)

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
                        self.draggable(true)
                    }
                    window.addEventListener('mouseup', mouseUpListener)
                })
            }

            self.draggable(true)

            addSelectionEventListener(self.controlGroup, circle, self)

            addDeletionEventListener(self);

            self.select(false);
        }
    }

    window.SvgToolbox = {
        Camera,
        Zone,
        CircleZone
    }
})();
