import { Observable, Subject, of, from, interval } from 'rxjs'
import { sample, takeUntil, concatMap } from 'rxjs/operators';

window.Orions.KeyboardListener = {

    init: function (dotNetHandle) {
        let repeatKeyEventSubject = new Subject();
        repeatKeyEventSubject
            .pipe(
                sample(interval(200))
            )
            .subscribe(event => {
                event.delayApplied = true
                keyEventHandler(event)
		    })

        let keyEventHandler = function (event) {
            if (event.defaultPrevented || event.target.tagName.toLowerCase() === "input" ) {
                return;
            }

            if (event.repeat && !event.delayApplied) {
                repeatKeyEventSubject.next(event)
                return
			}

            let keyCode = event.keyCode;

            let modKeys = (event.shiftKey ? 0x01 : 0) +
                (event.ctrlKey ? 0x02 : 0) +
                (event.altKey ? 0x04 : 0) +
                (event.metaKey ? 0x08 : 0);
            let handled = true;
            dotNetHandle.invokeMethodAsync('OnKeyEvent', keyCode, modKeys, event.type == 'keydown'? 0 : 1);

            if (handled) {
                event.preventDefault();
            }
        }

        window.addEventListener("keyup", keyEventHandler, false);
        window.addEventListener("keydown", keyEventHandler, false);
    }
}



