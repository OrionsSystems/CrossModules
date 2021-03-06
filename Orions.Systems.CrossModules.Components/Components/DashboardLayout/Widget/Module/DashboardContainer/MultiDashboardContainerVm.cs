﻿using System;
using System.Linq;
using System.Threading.Tasks;
using Orions.Common;
using Orions.Infrastructure.HyperMedia;
using Orions.Node.Common;

namespace Orions.Systems.CrossModules.Components
{
	[Config(typeof(MultiDashboardContainerWidget))]
	public class MultiDashboardContainerVm : WidgetVm<MultiDashboardContainerWidget>
	{
		public string ParrentDashboardId { get { return this.Widget.CurrentDashboard?.Id; } }

		public DashboardData CurrentDashboardData { get; private set; }

		public DashboardVm ChildDashboardVm { get; set; }

		//string _lastView = null;

		public MultiDashboardContainerVm()
		{
		}

		public override async Task HandleFiltersChangedAsync()
		{
			await base.HandleFiltersChangedAsync();

			var context = this.DashboardVm.ObtainFilterGroup(this.Widget.FilterGroup);

			bool dashboardChanged = this.Widget.ChangeCurrentDashboard(context.View);

			CurrentDashboardData = await GetDashboardAsync();

			if (ChildDashboardVm != null)
			{
				if (dashboardChanged)
					ChildDashboardVm.ClearWidgets(); // Drop all the widgets from the old dashboard.

				await ChildDashboardVm.FromParent_HandleFiltersChangedAsync(this.DashboardVm.FilterGroups);
			}

			if (dashboardChanged)
				this.RaiseNotify(nameof(CurrentDashboardData));
		}

		public async Task<DashboardData> GetDashboardAsync()
		{
			if (string.IsNullOrWhiteSpace(this.Widget.CurrentDashboard?.Id))
				return null;

			var documentId = HyperDocumentId.Create<DashboardData>(this.Widget.CurrentDashboard?.Id);
			var args = new RetrieveHyperDocumentArgs(documentId);
			var doc = await HyperStore.ExecuteAsync(args);

			if (args.ExecutionResult.IsNotSuccess)
				return null;

			var dashboard = doc?.GetPayload<DashboardData>();
			return dashboard;
		}
	}
}
