window.Orions.KeyboardListener = {

    init: function (dotNetHandle) {

        window.addEventListener("keydown", function (event) {
            if (event.defaultPrevented || event.target.tagName.toLowerCase() === "input" || event.repeat) {
                return;
            }

            let keyCode = event.keyCode;

            let modKeys = (event.shiftKey ? 0x01 : 0) +
                (event.ctrlKey ? 0x02 : 0) +
                (event.altKey ? 0x04 : 0) +
                (event.metaKey ? 0x08 : 0);
            let handled = false;
            dotNetHandle.invokeMethodAsync('OnKeyEvent', keyCode, modKeys);

            if (handled) {
                event.preventDefault();
            }
        }, false);
    }
}



