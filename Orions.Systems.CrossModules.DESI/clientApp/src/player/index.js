import videojs from 'video.js';
import {mp4} from './mux.js';

window.Orions.Player = {
    init: function (vmInstance, videoElementId) {
        videojs.registerPlugin('framebyframe', GenerateFrameByFramePlugin(vmInstance));

        let video = videojs(videoElementId, {
            inactivityTimeout: 0
        });

        video.framebyframe({
            steps: [{
                text: '<',
                step: -1
            },
            {
                text: '>',
                step: 1
                }]
        }, vmInstance);

        

        video.on('pause', function () {
            vmInstance.invokeMethodAsync("OnPauseJsCallback", video.currentTime());
        });

        video.on('timeupdate', function () {
            if (!video.doNotProcessPositionChanged) {
                vmInstance.invokeMethodAsync("OnPositionUpdate", video.currentTime());
            }

            video.doNotProcessPositionChanged = false;
        });

        video.on('play', function () {
            vmInstance.invokeMethodAsync("OnPlayJsCallback");
        });

        video.on('loadeddata', function () {
            vmInstance.invokeMethodAsync("OnPlayerDataLoaded");
        });

        this.video = video;
    },
    setSrc: function(payload, vmInstance, options) {
        let self = this;

        var blob = new Blob([Base64ToByteArray(payload)], { type: "video/mp4" });

        var url = URL.createObjectURL(blob);

        self.video.src({
            type: blob.type,
            src: url
        });
    },
    setPosition: function (position) {
        this.video.doNotProcessPositionChanged = true;
        this.video.currentTime(position);
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

function GenerateFrameByFramePlugin(vmInstance) {
    var VjsButton = videojs.getComponent('Button');
    var FBFButton = videojs.extend(VjsButton, {
        constructor: function (player, options) {
            VjsButton.call(this, player, options);
            this.player = player;
            this.step_size = options.value;
            this.on('click', this.onClick);
        },

        onClick: function () {
            this.player.pause();
            if (this.step_size > 0) {
                vmInstance.invokeMethodAsync("GoToNextFrame");
            }
            else {
                vmInstance.invokeMethodAsync("GoToPreviousFrame");
            }
        }
    });

    function framebyframe(options, vmInstance) {
        var player = this;
        player.on('loadeddata', function () {
        });

        player.on('timeupdate', function () {
        });

        player.ready(function (a) {
            options.steps.forEach(function (opt) {
                player.controlBar.addChild(
                    new FBFButton(player, {
                        el: videojs.dom.createEl(
                            'button',
                            {
                                className: 'vjs-res-button vjs-control',
                                innerHTML: '<div class="vjs-control-content" style="font-size: 11px; line-height: 28px;"><span class="vjs-fbf">' + opt.text + '</span></div>'
                            },
                            {
                                role: 'button'
                            }
                        ),
                        value: opt.step
                    }),
                    {}, opt.index);
            });

        });
    }

    return framebyframe;
}
