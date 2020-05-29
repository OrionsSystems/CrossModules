import { Player } from 'shaka-player'

window.Orions.ShakaPlayer = {
	players: {},
	init: function (config) {
		let player = this.players[config.id]
		if (player) {
			player.remove();
		}

		player = new ShakaPlayer();
		player.init(config)
		this.players[config.id] = player;
	},
	seek(id, position) {
		let player = this.players[id];
		if (player) {
			player.seek(position);
		}
	},
	remove(id) {
		let player = this.players[id];
		if (player) {
			player.remove();
		}
	}
};

class ShakaPlayer {
	init(config) {
		let self = this;

		this.playerVideoElement = document.getElementById(config.id)
		var playerInstance = new Player(this.playerVideoElement);
		playerInstance.load(config.file)
			.then(() => {
				if (config.autostart) {
					self.playerVideoElement.play();
				}

				if (config.startAt) {
					self.playerVideoElement.currentTime = config.startAt;
				}
			})

		this.shakaPlayer = playerInstance;
	}

	seek(position) {
		this.playerVideoElement.currentTime = position;
	}

	remove(){
		this.shakaPlayer.destroy();
	}
}