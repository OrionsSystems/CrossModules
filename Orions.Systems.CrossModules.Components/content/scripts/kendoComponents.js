window.KendoTreemap = {

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
}

window.KendoMediaPlayer = {

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
}