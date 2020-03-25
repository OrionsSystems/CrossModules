import videojs from 'video.js';
import {mp4} from './mux.js';

window.Orions.Player = {

    setSrc: (payload, vmInstance) => {
        videojs.registerPlugin('framebyframe', GenerateFrameByFramePlugin(vmInstance));

        var video = videojs("my-video");
        video.framebyframe({
            fps: 30,
            steps: [{
                text: '<',
                step: -1
            },
                {
                text: '>',
                step: 1
            }]
        });

        var blob = new Blob([Base64ToByteArray(payload)], { type: "video/mp4" });

        var url = URL.createObjectURL(blob);

        video.src({
            type: blob.type,
            src: url
        });

        video.on('pause', function () {
            vmInstance.invokeMethodAsync("OnPauseAsync", video.currentTime());
        });

        video.on('play', function () {
            vmInstance.invokeMethodAsync("OnPlay");
        });
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

function MuxIntoMp4(payload) {
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
}

function GenerateFrameByFramePlugin(vmInstance) {
    var VjsButton = videojs.getComponent('Button');
    var FBFButton = videojs.extend(VjsButton, {
        constructor: function (player, options) {
            VjsButton.call(this, player, options);
            this.player = player;
            this.frameTime = 1 / options.fps;
            this.step_size = options.value;
            this.on('click', this.onClick);
        },

        onClick: function () {
            // Start by pausing the player
            this.player.pause();
            // Calculate movement distance
            var dist = this.step_size;
            this.player.currentFrame = this.player.currentFrame + dist;
            //console.log('current pause:', (this.player.currentFrame * this.frameTime).toFixed(2));
            this.player.currentTime((this.player.currentFrame * this.frameTime).toFixed(2));
            vmInstance.invokeMethodAsync("OnPauseAsync", this.player.currentTime());
        }
    });

    function framebyframe(options) {
        var player = this,
            frameTime = 1 / options.fps;

        player.on('loadeddata', function () {
            player.totalFrames = player.duration() / frameTime;
            player.currentFrame = 0;
        });

        player.on('timeupdate', function () {
            player.currentFrame = Math.round(player.currentTime() / frameTime);
            //console.log('current pause:', player.currentFrame)
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
                        value: opt.step,
                        fps: options.fps,
                    }),
                    {}, opt.index);
            });

        });
    }

    return framebyframe;
}
