﻿using Orions.Common;
using Orions.Node.Common;
using System;
using System.Collections.Generic;

namespace Orions.Systems.CrossModules.Components
{
	public class DashboardData : IdUnifiedBlob, IName, IGroup
	{
		[DocumentDescriptor]
		public string Name { get; set; } = "New Dashboard";

		[DocumentDescriptor]
		public string Group { get; set; }

		[HelpText("Enable client access to dashboard")]
		[DocumentDescriptor]
		public bool Published { get; set; }

		[DocumentDescriptor]
		public bool IsHideTitle { get; set; } = true;

		[DocumentDescriptor]
		public string Tag { get; set; }

		[HelpText("Apply css styles to the bottom of the page")]
		[UniBrowsable(UniBrowsableAttribute.EditTypes.MultiLineText)]
		public string Styles { get; set; }


		[HelpText("Attach theme to dashboard", HelpTextAttribute.Priorities.Optional)]
		[HyperDocumentId.DocumentType(typeof(StyleTheme))]
		public HyperDocumentId? Theme { get; set; }

		[DocumentDescriptor]
		public bool EnableStyles { get; set; } = true;

		public LinkedList<DashboardRow> Rows { get; set; } = new LinkedList<DashboardRow>();

		[UniBrowsable(false)]
		public string DefaultGroup
		{
			get
			{
				if (string.IsNullOrWhiteSpace(Group)) 
					return "Default";

				return Group;
			}
		}

		public DashboardData()
		{
		}

		public override string ToString()
		{
			return this.Name;
		}
	}

	public class DashboardRow : IId
	{
		public string Id { get; set; } = IdGeneratorHelper.Generate("db-row-");

		public LinkedList<DashboardColumn> Columns { get; set; }

		public bool ShowCommands { get; set; }

		public DashboardRow()
		{
			Columns = new LinkedList<DashboardColumn>();
		}
	}

	public class DashboardColumn : IId
	{
		public string Id { get; set; } = IdGeneratorHelper.Generate("db-col-");
		public int Size { get; set; }
		public int Order { get; set; }
		public bool ShowCommands { get; set; }

		public LinkedList<DashboardRow> InnerRows { get; set; } = new LinkedList<DashboardRow>();

		public bool ShowBetweenCommands { get; set; }

		public DashboardWidget Widget { get; set; }
	}
}
