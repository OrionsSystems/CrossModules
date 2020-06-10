import paper, { Point, Path } from 'paper'

function getMousePos(canvas, evt) {
	var rect = canvas.getBoundingClientRect();
	return {
		x: evt.clientX - rect.left,
		y: evt.clientY - rect.top
	};
}

export default class CanvasCrosshair {
	constructor(paperScope, layer) {
		this.scope = paperScope;
		this.layer = layer;

		this.vertticalLine1 = new Path.Line(new Point(0, 0), new Point(0, 0));
		this.vertticalLine2 = new Path.Line(new Point(0, 0), new Point(0, 0));

		this.horizontalLine1 = new Path.Line(new Point(0, 0), new Point(0, 0));
		this.horizontalLine2 = new Path.Line(new Point(0, 0), new Point(0, 0));

		this.layer.addChild(this.vertticalLine1)
		this.layer.addChild(this.vertticalLine2)
		this.layer.addChild(this.horizontalLine1)
		this.layer.addChild(this.horizontalLine2)
	}

	move(point) {
		this.vertticalLine1.remove()
		this.vertticalLine2.remove()
		this.vertticalLine1 = new Path.Line(new Point(point.x, this.scope.view.bounds.top), new Point(point.x, point.y - 1));
		this.vertticalLine2 = new Path.Line(new Point(point.x, point.y + 1), new Point(point.x, this.scope.view.bounds.bottom))
		this.vertticalLine1.strokeColor = 'red'
		this.vertticalLine2.strokeColor = 'red'

		this.horizontalLine1.remove()
		this.horizontalLine2.remove()
		this.horizontalLine1 = new Path.Line(new Point(this.scope.view.bounds.left, point.y), new Point(point.x - 1, point.y))
		this.horizontalLine2 = new Path.Line(new Point(point.x + 1, point.y), new Point(this.scope.view.bounds.right, point.y))
		this.horizontalLine1.strokeColor = 'red'
		this.horizontalLine2.strokeColor = 'red'

		this.layer.addChild(this.vertticalLine1)
		this.layer.addChild(this.vertticalLine2)
		this.layer.addChild(this.horizontalLine1)
		this.layer.addChild(this.horizontalLine2)
	}
}