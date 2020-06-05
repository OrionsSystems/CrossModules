window.Orions.Player = {
    init: function (vmInstance, videoElementId, file) {
        this.vmInstance = vmInstance;
        let self = this;

        let video = document.getElementById(videoElementId)
        let shakaPlayer = new shaka.Player(video);
        this.shakaPlayer = shakaPlayer;
        

        video.addEventListener('timeupdate', function (e) {
            vmInstance.invokeMethodAsync("OnPositionUpdate", this.currentTime);

            console.log("timeupdate: " + this.currentTime)
            console.log(e);
        });

        video.addEventListener('canplay', function (e) {
            console.log('ready')
            console.log(e)

            vmInstance.invokeMethodAsync("OnPlayerReady");
        });

        video.addEventListener('ended', function () {
            vmInstance.invokeMethodAsync("OnVideoEndJs");

            this.isPaused = true;
        });

        shakaPlayer.addEventListener('buffering', function (e) {
            console.log('buffer')
            console.log(e)
            if (e.buffering) {
                vmInstance.invokeMethodAsync("OnVideoBuffering", true, this.getMediaElement().paused);
            }
            else {
                vmInstance.invokeMethodAsync("OnVideoBuffering", false, this.getMediaElement().paused);
			}
        });

        video.addEventListener('pause', function (e) {
            console.log('pause')
            console.log(e)
            vmInstance.invokeMethodAsync("OnVideoPaused");
        });

        this.video = video;

        this.wheelEventHandler = function (e) {
            if (!e.shiftKey) {
                vmInstance.invokeMethodAsync('OnMouseWheelHandler', e.deltaY > 0);
			}
        }
        document.querySelector('.right-column').addEventListener('wheel', this.wheelEventHandler)

        window.playerDebug = this;

        return shakaPlayer.load(file)
            .then(() => {
                self.vmInstance.invokeMethodAsync("OnPlayerReady");
            })
    },
    setSrc: function (url, vmInstance) {
        let self = this;

        return self.shakaPlayer.load(url)
            .then(() => {
                self.vmInstance.invokeMethodAsync("OnPlayerReady");
			})
    },

    setPosition: function (position) {
        this.video.currentTime = position;
	},

    setPositionAndPlay: function (position) {
        console.log('seek request: ' + position)

        let self = this;

        self.video.currentTime = position;

        return self.video.play();
    },

    setVolumeLevel: function (volLevel) {
        this.video.volume = volLevel / 100;
    },
    setSpeed: function (speed) {
        this.video.playbackRate = speed;
    },
    play: function () {
        this.video.play();
    },
    pause: function () {
        console.log('pause request')

        let pausePromise = new Promise((resolve, reject) => {
            let pausedHandler = function(e) {
                resolve();
                this.removeEventListener('pause', pausedHandler)
            };
            this.video.addEventListener('pause', pausedHandler)
            this.video.pause();
		})

        return pausePromise;
    },

    dispose: function () {
        let rightColumn = document.querySelector('.right-column')
        if (typeof rightColumn !== 'undefined') {
            rightColumn.removeEventListener('wheel', this.wheelEventHandler);
		}
	},

    playbackControl: {
        init: function (elementId, componentRef) {
            let self = this;

            self.componentRef = componentRef;
            self.elementId = elementId;

            let element = document.getElementById(elementId);
            let timeline = element.querySelector('.timeline');

            timeline.addEventListener('click', function (e) {
                let boundingRect = this.getBoundingClientRect();
                let percentage = (e.clientX - boundingRect.x) / this.getBoundingClientRect().width;

                if (percentage < 0) percentage = 0;
                if (percentage > 1) percentage = 1;

                self.componentRef.invokeMethodAsync('TimelineClick', percentage * 100 < 0 ? 0 : percentage * 100)
            });

            let autohideControls = element.querySelectorAll('.control-autohide');
            let autoHideControlParents = element.querySelectorAll('.control-autohide-parent');
            for (let control of autoHideControlParents) {
                control.addEventListener('click', function (e) {
                    let child = this.querySelector('.control-autohide')

                    if (child.style.visibility == 'hidden') {
                        child.style.visibility = 'visible'
                    }
                    else {
                        child.style.visibility = 'hidden'
                    }
                })
            }

            document.addEventListener('click', function (e) {
                for (let control of autohideControls) {
                    if (!e.target.parentElement.contains(control)) {
                        control.style.visibility = 'hidden'
                    }
                }
            })
        },
        positionMarkers: function (markers) {
            let self = this;
            let element = document.getElementById(self.elementId);
            let timeline = element.querySelector('.timeline');
            let boundingRect = timeline.getBoundingClientRect();

            for (let marker of markers) {
                let markerEl = element.querySelector(`.marker[data-sliceId="${marker.id}"]`);
                markerEl.style.left = boundingRect.width * marker.percentagePosition + 'px';
            }
        }
    }
};

function Base64ToByteArray(payload) {
    var binary_string = window.atob(payload);
    var len = binary_string.length;
    var bytes = new Uint8Array(len);
    for (var i = 0; i < len; i++) {
        bytes[i] = binary_string.charCodeAt(i);
    }
    return bytes.buffer;
}
