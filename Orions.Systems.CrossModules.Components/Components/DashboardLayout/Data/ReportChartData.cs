using System;
using System.Collections.Generic;
using System.Linq;

namespace Orions.Systems.CrossModules.Components
{
	public class ReportPieChartData
	{
		public double Value { get; set; }
		public string Label { get; set; }
		public bool ShouldShowInLegend { get; set; } = true;
		public bool Explode { get; set; }
		public string LabelInfo { get { return $"{Label}: {Value}"; } }
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
	}

	public class ReportSeriesChartData
	{
		public string Name { get; set; }
		public string Stack { get; set; }
		public List<ReportSeriesChartDataItem> Data { get; set; } = new List<ReportSeriesChartDataItem>();
	}

	public class ReportSeriesChartDataItem
	{
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
