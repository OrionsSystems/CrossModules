using System.Linq;
using Orions.Common;
using Orions.Node.Common;

namespace Orions.Systems.CrossModules.Components
{
	public class MultiDashboardContainerWidget : DashboardWidget
	{
		public DashboardLayout DashboardLayout;

		public class Item
		{
			[HelpText("The Id of the dashboard to use", HelpTextAttribute.Priorities.Mandatory)]
			[HyperDocumentId.DocumentType(typeof(DashboardData))]
			public HyperDocumentId? Dashboard { get; set; }

			public string View { get; set; }

			public Item()
			{
			}
		}

		public Item[] Items { get; set; } = new Item[] { };

		public HyperDocumentId? CurrentDashboard { get; private set; }

		public MultiDashboardContainerWidget()
		{
			this.Label = "Multi Dashboard container";
		}

		//public void ChangeCurrentDashboard(string view)
		//{
		//	this.CurrentDashboard = Items.FirstOrDefault(it => it.View == view)?.Dashboard;
		//	//this.RaiseNotify(nameof(CurrentDashboard));
		//}

		public override string GetIcon()
		{
			return "<svg version=\"1.1\" class=\"icon-container\" xmlns=\"http://www.w3.org/2000/svg\" xmlns:xlink=\"http://www.w3.org/1999/xlink\" x=\"0px\" y=\"0px\"   viewBox=\"0 0 150 150\" " +
				"style=\"enable-background:new 0 0 150 150;\" xml:space=\"preserve\"> " +
				"<path class=\"icon-container-item\" d=\"M138.2,39.3h-27.1V24.2c0-0.8-0.3-1.6-0.9-2.1L98.1,10c-0.6-0.6-1.3-0.9-2.1-0.9H53.8c-0.8,0-1.6,0.3-2.1,0.9  " +
				"L39.6,22.1c-0.6,0.6-0.9,1.3-0.9,2.1v15.1H11.6c-1.7,0-3,1.3-3,3v87.4c0,6.6,5.4,12.1,12.1,12.1h108.5c6.6,0,12.1-5.4,12.1-12.1  " +
				"V42.3C141.2,40.6,139.9,39.3,138.2,39.3z M99,19.4l6,6v13.8h-6V19.4z M56.8,15.1H93v6H56.8L56.8,15.1L56.8,15.1z M56.8,27.2H93v12.1  " +
			   "H56.8L56.8,27.2L56.8,27.2z M44.8,25.4l6-6v19.9h-6V25.4z M135.2,129.7c0,3.3-2.7,6-6,6H20.7c-3.3,0-6-2.7-6-6V45.3h120.6V129.7z\"/> " +
			   "<rect x=\"70\" y=\"29\" transform=\"matrix(-1.836970e-16 1 -1 -1.836970e-16 141.9732 -4.0309)\" class=\"icon-container-item\" width=\"6\" height=\"79.9\"/> " +
				"<rect x=\"70\" y=\"43.4\" transform=\"matrix(-1.836970e-16 1 -1 -1.836970e-16 156.3732 10.3691)\" class=\"icon-container-item\" width=\"6\" height=\"79.9\"/> " +
				"<rect x=\"70\" y=\"57.8\" transform=\"matrix(-1.836970e-16 1 -1 -1.836970e-16 170.7732 24.7691)\" class=\"icon-container-item\" width=\"6\" height=\"79.9\"/> <rect x=\"70\" y=\"72.2\" transform=\"matrix(-1.836970e-16 1 -1 -1.836970e-16 185.1732 39.1691)\" class=\"icon-container-item\" width=\"6\" height=\"79.9\"/> </svg>";
		}
	}
}
