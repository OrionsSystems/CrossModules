import _ from 'lodash'

if (typeof window.Orions.JsInteropUtils === 'undefined') {
	// this function serves as an async wrapper to IJSRutime calls from the server. It expect the called function to return a Promise object
	// and calls a task completion method when the Promise succedes
	window.Orions.JsInteropUtils = {
		callFunctionWithPromise: function (dotnetObjRef, functionPath, args) {
			// get a path to an object that contains a function to be called
			let objectPathToCallAfunctionOn = null;
			if (functionPath.lastIndexOf('.') != -1) {
				objectPathToCallAfunctionOn = functionPath.substring(0, functionPath.lastIndexOf('.'))
			}

			// get a function name
			let functionName = functionPath; // default it to the whole path
			if (objectPathToCallAfunctionOn != null) {
				functionName = functionPath.slice(functionPath.lastIndexOf('.') + 1)
			}

			// get an object to call a function on. defaulting to window object
			let objectToCallAFunctionOn = window;
			if (objectPathToCallAfunctionOn != null) {
				objectToCallAFunctionOn = _.get(window, objectPathToCallAfunctionOn)
			}

			// get a promise by calling a function
			let promise = objectToCallAFunctionOn[functionName](...args);

			promise
				.then((result) => {
					if (typeof result !== 'undefined') {
						dotnetObjRef.invokeMethodAsync('ResolvePromise', result);
					}
					else {
						dotnetObjRef.invokeMethodAsync('ResolvePromiseVoid', result);
					}
				})
				.catch(error => {
					dotnetObjRef.invokeMethodAsync('ErrorPromise', error.toString());
				})
		}
	}
}