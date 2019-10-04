using System;
using System.Collections.Generic;
using System.Text;

namespace Orions.Systems.CrossModules.Blazor.Common
{
	public class TreeMapItem
	{
		public string Name { get; set; }
		public float Value { get; set; }
		public ICollection<TreeMapItem> Items { get; set; }


		public static TreeMapItem[] GenerateTestData()
		{
			var treeMapItems = new List<TreeMapItem>();

			var items = new List<TreeMapItem>();

			items.Add(new TreeMapItem() { Name = "Alabama", Value = 4833722 });
			items.Add(new TreeMapItem() { Name = "Alaska", Value = 735132 });

			treeMapItems.Add(new TreeMapItem()
			{
				Name = "Population in USA",
				Value = 316128839,
				Items = items

			});

			return treeMapItems.ToArray();
		}
	}
}
