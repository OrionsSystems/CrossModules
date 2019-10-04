$(document).ajaxError(function (e, jqxhr, settings, exception) {
    if (jqxhr != null) {
        try {
            if (jqxhr.responseJSON && jqxhr.responseJSON.Message) {
                toastr.error(jqxhr.responseJSON.Message);
            } else {
                toastr.error(jqxhr.responseText);
            }
        } catch (ex) {
            console.log(ex);
        }
    }
});

function guid() {
    function _p8(s) {
        var p = (Math.random().toString(16) + "000000000").substr(2, 8);
        return s ? "-" + p.substr(0, 4) + "-" + p.substr(4, 4) : p;
    }
    return _p8() + _p8(true) + _p8(true) + _p8();
}

function getUrlQueryParameters(sParam) {
    var sPageURL = decodeURIComponent(window.location.search.substring(1)),
        sURLVariables = sPageURL.split('&'),
        sParameterName,
        i;
    var values = [];
    for (i = 0; i < sURLVariables.length; i++) {
        sParameterName = sURLVariables[i].split('=');

        if (sParameterName[0] === sParam) {
            values.push(sParameterName[1] === undefined ? true : sParameterName[1]);
        }
    }
    return values;
}

function encodeQuery(data) {
	var ret = [];
	for (var d in data) {
		var innerData = data[d];
		if (typeof data[d] === 'object') {
			innerData = JSON.stringify(data[d]);
		}
		ret.push(encodeURIComponent(d) + "=" + encodeURIComponent(innerData));
	}
	return '?' + ret.join("&");
}

function updateUrl(params, pageName) {
	try {
		if (!params) return;
		var query = Object.keys(params).map(function (key) {
			if (Array.isArray(params[key])) {
				var elements = [];
				for (var i = 0; i < params[key].length; i++)
					elements.push(key + '=' + params[key][i]);
				return elements.join("&");
			} else {
				return key + '=' + params[key];
			}
		}).join('&');

		if (query && (query.length > 1)) {
			query = query.indexOf('&') === 0 ? query.substring(1) : query;
		}

		var segments = document.location.href.split("?");
		var newUrl = segments[0] + "?" + query;
		if (history.pushState) {
			window.history.pushState("", pageName|| "Index", newUrl);
		} else {
			document.location.href = newUrl;
		}
	} catch (err) {
		console.log(err);
	}
}

function getBaseAddress() {
	return window.location.protocol + '//' + window.location.hostname + ':' + window.location.port;
}

function decodeUrl() {
	var params = {};
	var decodeUrl = decodeURIComponent(window.location.href);
	decodeUrl.replace(/[?&]+([^=&]+)=([^&]*)/gi, function (m, key, value) {
		params[key] = value.replace(/[+]/g, ' ').trim();
	});
	return params;
}

function getUrlQueryParameter(sParam) {
    values = getUrlQueryParameters(sParam);

    if (values.length === 0) return undefined;

    return values[0];
}

function copyTextToClipboard(text) {
	if (window.clipboardData && window.clipboardData.setData) {
		// IE specific code path to prevent textarea being shown while dialog is visible.
		return clipboardData.setData("Text", text);

	} else if (document.queryCommandSupported && document.queryCommandSupported("copy")) {
		var textarea = document.createElement("textarea");
		textarea.textContent = text;
		textarea.style.position = "fixed";  // Prevent scrolling to bottom of page in MS Edge.
		document.body.appendChild(textarea);
		textarea.select();
		try {
			return document.execCommand("copy");  // Security exception may be thrown by some browsers.
		} catch (ex) {
			console.warn("Copy to clipboard failed.", ex);
			return false;
		} finally {
			document.body.removeChild(textarea);
		}
	}
}