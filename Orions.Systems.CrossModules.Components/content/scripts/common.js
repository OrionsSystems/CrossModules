window.Orions = {};

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

    remove: function (id) {
        var player = jwplayer(id);
        if (player !== null) {
            player.remove();
        }
    }
};

window.Orions.KendoTreemap = {

    init: function (id, data) {
        var treemapId = "#" + id;
        createTreeMap(treemapId, data);
        $(document).bind("kendo:skinChange", createTreeMap);
        $(".options").bind("change", refresh);

        addToolTip(treemapId);

        function createTreeMap(id, data) {
            $(id).kendoTreeMap({
                dataSource: {
                    data: data,
                    schema: {
                        model: {
                            children: "items"
                        }
                    }
                },
                valueField: "value",
                textField: "name",

            }).on("click", ".k-leaf, .k-treemap-title", function (e) {
                var item = $(id).data("kendoTreeMap").dataItem($(this).closest(".k-treemap-tile"));
                console.log(item.name + ": " + item.value);
            });
        }

        function addToolTip(id) {

            $(id).kendoTooltip({
                filter: ".k-leaf,.k-treemap-title",
                position: "top",
                content: function (e) {
                    var treemap = $(id).data("kendoTreeMap");
                    var item = treemap.dataItem(e.target.closest(".k-treemap-tile"));
                    return item.name + ": " + item.value;
                }
            });
        }

        function refresh() {
            $("#treeMap").getKendoTreeMap().setOptions({
                type: $("input[name=type]:checked").val()
            });
        }

    }
};

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

var _pickerMap = {};
window.Orions.VanillaColorPicker = {

    init: function (config) {

        var parent = document.querySelector('#' + config.parentId);

        var options = {
            parent: parent,
            popup: config.popup,
            alpha: config.alpha,
            editor: config.editor,
            editorFormat: config.editorFormat,
            cancelButton: config.cancelButton,
            color: config.color
        };
        

        var picker = new Picker(options);

        //picker.openHandler();

        picker.onDone = function (color) {
            console.log('onDone', this.settings.parent.id, color.rgba);

            if (this.settings.editorFormat === 'rgb') {
                parent.style.background = this.color.rgbString;
                parent.innerText = this.color.rgbString;
            }

            if (this.settings.editorFormat === 'hex') {
                parent.style.background = this.color.hex;
                parent.innerText = this.color.hex;
            }

            if (this.settings.editorFormat === 'hsl') {
                parent.style.background = this.color.hslString;
                parent.innerText = this.color.hslString;
            }
            

            //var evt = document.createEvent("HTMLEvents");
            //evt.initEvent("change", false, true);
            //parent.dispatchEvent(evt);
        };
        picker.onOpen = function (color) { console.log('Opened', this.settings.parent.id, color.rgba); };
        picker.onClose = function (color) { console.log('Closed', this.settings.parent.id, color.rgba); };
        picker.onChange = function (color) {
            //console.log('Change');
        };

        _pickerMap[config.parentId] = picker;
    },
    destroy: function () {  },
    setColor: function (color) {  },

};