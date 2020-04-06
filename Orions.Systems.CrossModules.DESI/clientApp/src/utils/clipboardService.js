window.Orions.ClipboardService = {
	setText: function (text) {
		navigator.clipboard.writeText(text);
	}
}