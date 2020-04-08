window.Orions.TaggingPage = {
	tagonomyInfoInit: function (selector, componentRef) {
		let el = document.querySelector(selector)
		el.style.visibility = 'visible'

		let clickEvHandler = function (e) {
			if (e.target.classList.contains('copy-btn') == false) {
				e.stopPropagation();
			}
		}
		let mouseDownEventListener = function (e) {
			if (!el.contains(e.target)) {
				componentRef.invokeMethodAsync("Close");
				document.removeEventListener('mousedown', mouseDownEventListener)
				el.removeEventListener('click', clickEvHandler)
			}
		}
		document.addEventListener('mousedown', mouseDownEventListener)

		el.addEventListener('click', clickEvHandler)
	}
}