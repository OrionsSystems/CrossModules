
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
				textField: "name"

			}).on("click", ".k-leaf, .k-treemap-title", function (e) {
				var item = $(id).data("kendoTreeMap").dataItem($(this).closest(".k-treemap-tile"));
				//console.log(item.name + ": " + item.value);
			});
		}

		function addToolTip(id) {

			$(id).kendoTooltip({
				filter: ".k-leaf,.k-treemap-title",
				position: "top",
				content: function (e) {
					var treemap = $(id).data("kendoTreeMap");
					var item = treemap.dataItem(e.target.closest(".k-treemap-tile"));
					//return item.name + ": " + item.value;
					return item.value;
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
