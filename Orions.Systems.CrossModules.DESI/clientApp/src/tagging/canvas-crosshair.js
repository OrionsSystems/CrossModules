import paper, { Point, Path } from 'paper'

function getMousePos(canvas, evt) {
	var rect = canvas.getBoundingClientRect();
	return {
		x: evt.clientX - rect.left,
		y: evt.clientY - rect.top
	};
}

export default class CanvasCrosshair {
	constructor(paperScope) {
		this.scope = paperScope;
		this.vertticalLine1 = new Path.Line(new Point(0, 0), new Point(0, 0));
		this.vertticalLine2 = new Path.Line(new Point(0, 0), new Point(0, 0));

		this.horizontalLine1 = new Path.Line(new Point(0, 0), new Point(0, 0));
		this.horizontalLine2 = new Path.Line(new Point(0, 0), new Point(0, 0));
	}

	move(point) {
		this.vertticalLine1.remove()
		this.vertticalLine2.remove()
		this.vertticalLine1 = new Path.Line(new Point(point.x, 0), new Point(point.x, point.y - 1));
		this.vertticalLine2 = new Path.Line(new Point(point.x, point.y + 1), new Point(point.x, this.scope.view.getContext().canvas.height))
		this.vertticalLine1.strokeColor = 'red'
		this.vertticalLine2.strokeColor = 'red'

		this.horizontalLine1.remove()
		this.horizontalLine2.remove()
		this.horizontalLine1 = new Path.Line(new Point(0, point.y), new Point(point.x - 1, point.y))
		this.horizontalLine2 = new Path.Line(new Point(point.x + 1, point.y), new Point(this.scope.view.getContext().canvas.width, point.y))
		this.horizontalLine1.strokeColor = 'red'
		this.horizontalLine2.strokeColor = 'red'
	}
}