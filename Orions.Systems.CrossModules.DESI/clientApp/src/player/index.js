import videojs from 'video.js';
import {mp4} from './mux.js';

window.Orions.Player = {};

window.Orions.Player.setSrc = (payload) => {
	//var muxer = new mp4.Transmuxer();
	//			muxer.init();

	//muxer.on('data', segment => {
	//	console.log(segment);

	//	var video = videojs("my-video");

	//	var blob = new Blob([segment], { type: "video/mp4" });

	//	var url = URL.createObjectURL(blob);

	//	video.src({
	//		type: 'video/mp4',
	//		src: url
	//	});
	//});
	//muxer.push(payload);
	//muxer.flush();
	//console.log("flushed");

	var binary_string = window.atob(payload);
	var len = binary_string.length;
	var bytes = new Uint8Array(len);
	for (var i = 0; i < len; i++) {
		bytes[i] = binary_string.charCodeAt(i);
	}

	var video = videojs("my-video");

	var blob = new Blob([bytes.buffer], { type: "video/mp4" });

	var url = URL.createObjectURL(blob);

	video.src({
		type: blob.type,
		src: url
	});

	//video.on('ended', function () {
	//	videojs.log('Awww...over so soon?!');
	//});
}
