
window.Orions.KendoMediaPlayer = {

	init: function (mediaPlayerData) {
		var id = mediaPlayerData.id;
		var title = mediaPlayerData.title;
		var source = mediaPlayerData.source;

		var playerId = "#" + id;
		$(playerId).kendoMediaPlayer({
			autoPlay: mediaPlayerData.autoPlay,
			navigatable: mediaPlayerData.navigatable,
			mute: mediaPlayerData.mute,
			media: {
				title: title,
				source: source
			},
			play: function (e) {
				DotNet.invokeMethodAsync('Orions.Systems.CrossModules.Components', 'OnPlayAsync');
			},
			pause: function (e) {

				DotNet.invokeMethodAsync('Orions.Systems.CrossModules.Components', 'OnPauseAsync');

			},

			timeChange: function (e) {
				DotNet.invokeMethodAsync('Orions.Systems.CrossModules.Components', 'OnTimeChangeAsync');
			}
		});
	},

	getPlayer: function (id) {
		var playerId = "#" + id;

		return $(playerId).getKendoMediaPlayer();
	}
};