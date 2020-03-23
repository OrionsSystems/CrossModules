import videojs from 'video.js';
import {mp4} from 'mux.js';

window.Orions.Player = {};

window.Orions.Player.setSrc = (payload) => {
	var muxer = new mp4.Transmuxer();
				muxer.init();

	muxer.on('data', segment => {
		console.log(segment);

		var video = videojs("my-video");

		var blob = new Blob([segment], { type: "video/mp4" });

		var url = URL.createObjectURL(blob);

		video.src({
			type: 'video/mp4',
			src: url
		});
	});
	muxer.push(payload);
	muxer.flush();
	console.log("flushed");
}
