
window.Orions.JwPlayer = {

	init: function (config) {

		jwplayer.key = config.key;

		var playerSetup = {
			file: config.file,
			image: config.image,
			title: config.title,
			description: config.description,
			sources: config.sources,
			logo: config.logoConfig,
			width: config.width,
			height: config.height,
			displaytitle: config.displayTitle,
			stretching: config.stretching,
			aspectratio: config.aspectratio,
			autoplay: config.autoplay,
			preload: config.preload,
			abouttext: config.aboutText,
			aboutlink: config.aboutLink,
			autostart: config.autostart,
			mute: config.mute,
			playlist: [
				{
					file: config.file,
					starttime: config.startAt
				}
			]
			//playlist: 'https://cdn.jwplayer.com/v2/playlists/qI5YMsQg?related_media_id=OpQMbAfZ',
			//related: {
			//    autoplaytimer: 10,
			//    displayMode: "shelfWidget",
			//    onclick: "link",
			//    oncomplete: "autoplay"
			//}

		};

		var playerIsntance = jwplayer(config.id);
		var player = playerIsntance.setup(playerSetup);

		player.on('ready', function () {
			player.play();
		});
	},

	seek: function (id, position) {
		jwplayer(id).seek(position);
	},

	remove: function (id) {
		var player = jwplayer(id);
		if (player !== null) {
			player.remove();
		}
	}
};