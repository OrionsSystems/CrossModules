(function () {

	window.Orions.FileSaveAs = {
		init: function (filename, fileContent) {
			var link = document.createElement('a');
			link.download = filename;
			link.href = "data:text/plain;charset=utf-8," + encodeURIComponent(fileContent);
			document.body.appendChild(link);
			link.click();
			document.body.removeChild(link);
		}
	};

	window.Orions.FileArhiveAs = {
		init: function (filename, fileContent) {
			var link = document.createElement('a');
			link.download = filename;
			link.href = "data:application/zip;base64," + fileContent;
			document.body.appendChild(link);
			link.click();
			document.body.removeChild(link);
		}
	};
})();