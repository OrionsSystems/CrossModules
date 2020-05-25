window.Orions.Player = {
    init: function (vmInstance, videoElementId, file) {
        let video = videojs(videoElementId);
        video.src({ src: file, type: 'application/dash+xml' })

        video.on('timeupdate', function (e) {
            vmInstance.invokeMethodAsync("OnPositionUpdate", this.currentTime());

            console.log("timeupdate: ")
            console.log(e);
        });

        video.on('canplay', function (e) {
            console.log('ready')
            console.log(e)

            vmInstance.invokeMethodAsync("OnPlayerReady");
        });

        video.on('ended', function () {
            vmInstance.invokeMethodAsync("OnVideoEndJs");

            this.isPaused = true;
        });

        video.on('waiting', function (e) {
            console.log('buffer')
            console.log(e)
            vmInstance.invokeMethodAsync("OnVideoBuffering");
        });

        video.on('playing', function (e) {
            console.log('play')
            console.log(e)
            vmInstance.invokeMethodAsync("OnVideoPlaying");
        });

        video.on('pause', function (e) {
            console.log('pause')
            console.log(e)
            vmInstance.invokeMethodAsync("OnVideoPaused");
        });

        this.video = video;

        window.playerDebug = this;
    },
    setSrc: function (url, vmInstance) {
        let self = this;

        self.video.src({ src: url, type: 'application/dash+xml' })
    },

    setPosition: function (position) {
        this.video.seek(position);
    },

    setPositionAndPlay: function (position) {
        console.log('seek request: ' + position)
        this.video.currentTime(position);
        this.video.play();
    },

    setVolumeLevel: function (volLevel) {
        this.video.volume(volLevel / 100);
    },
    setSpeed: function (speed) {
        this.video.playbackRate(speed);
    },
    play: function () {
        this.video.play();
    },
    pause: function () {
        console.log('pause request')
        this.video.pause();
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
