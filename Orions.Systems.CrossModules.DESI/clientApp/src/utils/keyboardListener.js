window.Orions.KeyboardListener = {

    init: function (dotNetHandle) {
        let keyEventHandler = function (event) {
            if (event.defaultPrevented || event.target.tagName.toLowerCase() === "input" || event.repeat) {
                return;
            }

            let keyCode = event.keyCode;

            let modKeys = (event.shiftKey ? 0x01 : 0) +
                (event.ctrlKey ? 0x02 : 0) +
                (event.altKey ? 0x04 : 0) +
                (event.metaKey ? 0x08 : 0);
            let handled = false;
            dotNetHandle.invokeMethodAsync('OnKeyEvent', keyCode, modKeys, event.type == 'keydown'? 0 : 1);

            if (handled) {
                event.preventDefault();
            }
        }

        window.addEventListener("keyup", keyEventHandler, false);
        window.addEventListener("keydown", keyEventHandler, false);
    }
}



