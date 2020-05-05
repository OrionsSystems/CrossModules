

function raiseResizeEvent() {
	var namespace = 'Orions.Systems.CrossModules.Components'; // the namespace of the app
	var method = 'RaiseWindowResizeEvent'; //the name of the method in our "service"
	DotNet.invokeMethodAsync(namespace, method, Math.floor(window.innerWidth), Math.floor(window.innerHeight));
}

var timeout = false;
window.addEventListener("resize", function () {
	if (timeout !== false)
		clearTimeout(timeout);

	timeout = setTimeout(raiseResizeEvent, 200);
});