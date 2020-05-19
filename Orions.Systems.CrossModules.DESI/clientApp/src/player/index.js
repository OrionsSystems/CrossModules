window.Orions.Player = {
    init: function (vmInstance, videoElementId, file) {
        window.jwplayer.key = '0cEcLfyR+RBLPh2z3KkWfFzX1U/w/2AtglS1xoT4xl8=';

        let config = {
            "file": file,
            controls: false
        }

        let video = window.jwplayer(videoElementId).setup(config);
        video.isPaused = true;

        video.on('time', function (e) {
            if (!this.seekAndPlay) {
                vmInstance.invokeMethodAsync("OnPositionUpdate", e.position);
			}

            console.log("timeupdate: ")
            console.log(e);
        });

        video.on('ready', function () {
            if (this.getState() == 'idle') {
                this.isPaused = true;
            }
            else {
                this.isPaused = false;
            }

            vmInstance.invokeMethodAsync("OnPlayerDataLoaded");
        });

        video.on('complete', function () {
            vmInstance.invokeMethodAsync("OnVideoEndJs");

            this.isPaused = true;
        });


        video.on('seeked', function (e) {
            console.log('seeked')
            if (this.isPaused && !this.seekAndPlay) {
                this.pause();
                console.log('pause on seeked')
            }
        })

        video.on('firstFrame', function () {
            console.log('firstFrame')
            if (this.seekAndPlay) {
                this.seek(this.seekAndPlayPos);
                this.seekAndPlay = false;
			}
		})

        this.video = video;
    },
    setSrc: function (url, vmInstance) {
        let self = this;

        self.video.load({
            file: url
		})
    },

    setPosition: function (position) {
        this.video.seek(position);
    },

    setPositionAndPlay: function (position) {
        console.log('seek request: ' + position)
        if (this.video.getState() == 'complete') {
            this.video.play();
            this.video.seekAndPlay = true;
            this.video.seekAndPlayPos = position;
        }
        else {
            this.video.seek(position)
            this.video.play();
		}
        this.video.isPaused = false;
    },

    setVolumeLevel: function (volLevel) {
        this.video.setVolume(volLevel);
    },
    setSpeed: function (speed) {
        this.video.setPlaybackRate(speed);
    },
    play: function () {
        this.video.play();
        this.video.isPaused = false;
    },
    pause: function () {
        this.video.pause();
        this.video.isPaused = true;
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
