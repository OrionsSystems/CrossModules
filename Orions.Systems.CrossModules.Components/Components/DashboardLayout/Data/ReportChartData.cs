using Orions.Infrastructure.Common;
using Orions.Infrastructure.HyperMedia;
using Orions.Node.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Orions.Systems.CrossModules.Components
{
	public class ReportPieChartData
	{
		public string Label { get; set; }
		public double Value { get; set; }
		public double Percentage { get; set; }
		public int PercentagePrecision { get; set; } = 2;
		public bool ShouldShowInLegend { get; set; } = true;
	}

	public class ReportChartData
	{
		public string[] Categories => _categories.ToArray();

		private List<string> _categories = new List<string>();
		public List<ReportSeriesChartData> Series { get; private set; } = new List<ReportSeriesChartData>();

		public bool IsDateAxis { get; set; } = true;

		public void AddCategory(string category)
		{
			if (!_categories.Contains(category))
				_categories.Add(category);
		}

		public void AddCategoryRange(string[] categories)
		{
			foreach (var category in categories ?? Enumerable.Empty<string>())
			{
				AddCategory(category);
			}
		}

		public void Clean()
		{
			_categories.Clear();
			Series.Clear();
		}

		public void MapIcons(IconMapping mapping) 
		{
			if (mapping == null)
				return;

			foreach (var item in Series) 
			{
				var documentId = mapping.TryMap(item.Name);
				if (documentId != null) {
					item.IconDocument = documentId;
				}
			}
		}
	}

	public class ReportSeriesChartData
	{
		public string Name { get; set; }
		public string Stack { get; set; }
		public List<ReportSeriesChartDataItem> Data { get; set; } = new List<ReportSeriesChartDataItem>();

		public UniIconResource Icon { get; set; }

		public HyperDocumentId? IconDocument { get; set; }

		public string SvgIcon() {
			if (Icon == null || Icon.Type != UniIconResource.Types.Svg) return String.Empty;

			return Encoding.UTF8.GetString(Icon.Data);
		}
	}

	public class ReportSeriesChartDataItem
	{
		public string CategoryName { get; set; }
		public string Value { get; set; }
		public string Label { get; set; }

		public int? IntValue
		{
			get
			{
				int value;
				var result = int.TryParse(Value, out value);
				if (!result) return null;
				return value;
			}
		}

		public DateTime? DateValue
		{
			get
			{
				DateTime value;
				var result = DateTime.TryParse(Value, out value);
				if (!result) return null;
				return value;
			}
		}

		public double? DoubleValue
		{
			get
			{
				double value;
				var result = double.TryParse(Value, out value);
				if (!result) return null;
				return value;
			}
		}

		public DateTime? DatePosition { get; set; }
		public string StreamPosition { get; set; }
	}
}
