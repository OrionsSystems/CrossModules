import videojs from 'video.js';

window.Orions.Player = {
    init: function (vmInstance, videoElementId) {
        let video = document.getElementById(videoElementId)

        video.addEventListener('keydown', function (e) {
            e.preventDefault();
        })

        video.addEventListener('timeupdate', function () {
            if (!video.doNotProcessPositionChanged) {
                vmInstance.invokeMethodAsync("OnPositionUpdate", video.currentTime);

                console.log(video.currentTime);
            }

            video.doNotProcessPositionChanged = false;
        });

        video.addEventListener('loadeddata', function () {
            vmInstance.invokeMethodAsync("OnPlayerDataLoaded");
        });

        video.addEventListener('ended', function () {
            vmInstance.invokeMethodAsync("OnVideoEndJs");
        });

        this.video = video;
    },
    setSrc: function (payload, vmInstance) {
        let self = this;

        var blob = new Blob([Base64ToByteArray(payload)], { type: "video/mp4" });

        var url = URL.createObjectURL(blob);

        self.video.src = url;
    },
    setPosition: function (position) {
        this.video.doNotProcessPositionChanged = true;
        this.video.currentTime = position;
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
