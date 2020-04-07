window.Orions.Dom = {
	// positions element with absolute coordinates relative to its parent
	positionAbsoluteToParent: function (pos, elementSelector, cornerToPositionTo, fitDocumentBody, setVisibilityToTrue = false) {
		let elements = document.querySelectorAll(elementSelector);

		for (let el of elements) {
			let parent = el.parentElement;
			let boundingRect = parent.getBoundingClientRect();

			el.style.position = 'absolute';

			let elPos = {
				top: 0,
				left: 0
			}
			switch (cornerToPositionTo) {
				case 'topLeft':
					elPos.top = boundingRect.top + pos.top
					elPos.left = boundingRect.left + pos.left

					break;

				case 'topRight':
					elPos.top = boundingRect.top + pos.top 
					elPos.left = boundingRect.right + pos.left
					break;

				case 'bottomLeft':
					elPos.top = boundingRect.bottom + pos.top 
					elPos.left = boundingRect.left + pos.left 
					break;

				case 'bottomRight':
					elPos.top = boundingRect.bottom + pos.top
					elPos.left = boundingRect.right + pos.left 
					break;
			}

			if (fitDocumentBody) {
				let body = document.getElementsByTagName('body')[0]
				let elRect = el.getBoundingClientRect()
				let bodyRect = body.getBoundingClientRect();

				if (elPos.top + elRect.height > bodyRect.bottom) {
					elPos.top = bodyRect.bottom - elRect.height
				}

				if (elPos.left + elRect.width > bodyRect.right) {
					elPos.left = bodyRect.right - elRect.width
				}
			}

			el.style.top = elPos.top + 'px'
			el.style.left = elPos.left + 'px'

			if (setVisibilityToTrue) {
				el.style.visibility = 'visible'
			}
		}
	},

	setStyle: function (elementSelector, styles) {
		let elements = document.querySelectorAll(elementSelector);

		for (let el of elements) {
			for (let key of Object.keys(styles)) {
				el.style[key] = styles[key];
			}
		}
	}
}