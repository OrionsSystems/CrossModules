﻿using Telerik.Blazor;

namespace Orions.Systems.CrossModules.Components
{
	public abstract class ReportTelericBaseChartWidget : ReportBaseWidget
	{
		public bool IsShowChartTitle { get; set; } = false;
		public string ChartTitle { get; set; }

		public ChartTitlePosition ChartTitlePosition { get; set; } = ChartTitlePosition.Top;

		public virtual ChartSeriesType ChartSeriesType { get; set; } = ChartSeriesType.Column;

		public ChartSeriesStyle ChartSeriesStyle { get; set; } = ChartSeriesStyle.Normal;

		public int BaseUnitStep { get; set; }

		public ChartCategoryAxisBaseUnit ChartCategoryAxisBaseUnit { get; set; } = ChartCategoryAxisBaseUnit.Fit;

		public ChartLegendPosition ChartLegendPosition { get; set; } = ChartLegendPosition.Bottom;

		public string ChartCategoryAxisLabelsFormat { get; set; } = "{0:dd MMM HH:mm}";

		public bool IsChartSeriesLabelsVisible { get; set; } = false;

		public virtual bool IsChartSeriesStackEnabled { get; set; } = false;

		public bool IsLegenedVisible { get; set; } = true;
	}
}
