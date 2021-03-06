﻿using Orions.Common;
using Syncfusion.EJ2.Blazor;
using Syncfusion.EJ2.Blazor.Charts;

namespace Orions.Systems.CrossModules.Components
{
	public abstract class ReportSyncfusionBaseChartWidget : ActiveFilterReportChartWidget
	{
		public class TooltipConfiguration {

			[HelpText("Enables / Disables the visibility of the tooltip.")]
			public bool IsEnable { get; set; } = true;

			[HelpText("Format the ToolTip content.")]
			public string Format { get; set; }

			public double Opacity { get; set; } = 0.75;

			[HelpText(" Options to customize the ToolTip text.")]
			public ChartTooltipTextStyle TextStyle { get; set; }

			[HelpText("Header for tooltip.")]
			public string Header { get; set; }

			[HelpText("The fill color of the tooltip")]
			public string Fill { get; set; }

			[HelpText("Enables / Disables the visibility of the marker.")]
			public bool EnableMarker { get; set; } = true;

			[HelpText("Options to customize tooltip borders.")]
			public ChartTooltipBorder Border { get; set; }
		}

		public class ChartMargin 
		{
			public int Top { get; set; }
			public int Left { get; set; }
			public int Right { get; set; }
			public int Bottom { get; set; }
		}

		public class PrimaryXAxis
		{
			public bool Visible { get; set; } = true;

			public bool XAxisIsIndexed { get; set; } = true;

			public LabelPlacement LabelPlacement { get; set; } = LabelPlacement.OnTicks;

			public IntervalType IntervalType { get; set; } = IntervalType.Days;

			public string LabelFormat { get; set; } = "dd MMM HH:mm";

			public ChartAxisLabelStyle LabelStyle { get; set; } = new ChartAxisLabelStyle();

			public string Skeleton { get; set; } = "Ed";

			public EdgeLabelPlacement EdgeLabelPlacement { get; set; } = EdgeLabelPlacement.Shift;

			public ChartAxisMajorGridLines MajorGridLines { get; set; } = new ChartAxisMajorGridLines();

			public ValueType ValueType { get; set; } = ValueType.Category;

			//public int Minimum { get; set; }
			//public int Maximum { get; set; }

			public int Interval { get; set; } = 1;

			public AxisPosition TickPosition { get; set; } = AxisPosition.Inside;

			public AxisPosition LabelPosition { get; set; } = AxisPosition.Outside;

			public double LabelRotation { get; set; }

			public LabelIntersectAction LabelIntersectAction { get; set; } = LabelIntersectAction.Rotate45;
		}

		public class PrimaryYAxis {
			public int Minimum { get; set; }

			public bool Visible { get; set; } = true;

			//public int Interval { get; set; } = 1;
			//public int Maximum { get; set; }

			[HelpText("Used to format the axis label that accepts any global string format like 'C', 'n1', 'P' etc. It also accepts placeholder like '{value}°C' in which value represent the axis label, e.g, 20°C.")]
			public string LabelFormat { get; set; }

			public ChartAxisLineStyle LineStyle { get; set; } = new ChartAxisLineStyle();

			public ChartAxisMajorTickLines MajorTickLines { get; set; } = new ChartAxisMajorTickLines();

			public ChartAxisMinorTickLines MinorTickLines { get; set; } = new ChartAxisMinorTickLines();

			public ChartAxisMajorGridLines GridLineSettings { get; set; } = new ChartAxisMajorGridLines();
		}

		public class ChartMarkerConfiguration 
		{
			[HelpText("The different shape of a marker")]
			public ChartShape Shape { get; set; } = ChartShape.Circle;

			[HelpText("The opacity of the marker.")]
			public double Opacity { get; set; } = 1;

			[HelpText("The height of the marker in pixels.")]
			public double Height { get; set; }

			[HelpText("The fill color of the marker that accepts value in hex and rgba as a valid CSS color string. By default, it will take series' color.")]
			public string Fill { get; set; }

			[HelpText("The data label for the series.")]
			public ChartDataLabel DataLabel { get; set; }

			[HelpText("Options for customizing the border of a marker.")]
			public ChartMarkerBorder Border { get; set; }

			[HelpText("The width of the marker in pixels.")]
			public double Width { get; set; }

			[HelpText("If set to true the marker for series is rendered. This is applicable only for line and area type series.")]
			public bool Visible { get; set; }

			[HelpText("Specifies the position of the data label.")]
			public LabelPosition Position { get; set; } = LabelPosition.Auto;

			[HelpText("Option for customizing the data label text.")]
			public ChartDataLabelFont Font { get; set; }
		}

		public class ChartTitlesConfiguration 
		{
			public bool EnableTitle { get; set; } = false;
		
			public bool EnableSubTitle { get; set; } = false;

			[HelpText("Title of the chart")]
			public string Title { get; set; }

			[HelpText("Options for customizing the title of the Chart.")]
			public ChartTitleStyle TitleStyle { get; set; }

			[HelpText("SubTitle of the chart")]
			public string SubTitle { get; set; }

			[HelpText("Options for customizing the Subtitle of the Chart.")]
			public ChartSubTitleStyle SubTitleStyle { get; set; }
		}

		public class ChartSeriesConfiguration 
		{
			[HelpText("The opacity of the series.")]
			public double Opacity { get; set; } = 1;

			[HelpText("Minimum radius")]
			public double MinRadius { get; set; } = 1;

			[HelpText("Maximum radius")]
			public double MaxRadius { get; set; } = 3;

			[HelpText("If set true, the mean value for box and whisker will be visible.")]
			public bool ShowMean { get; set; } = true;

			[HelpText("The normal distribution of histogram series.")]
			public bool ShowNormalDistribution { get; set; }

			[HelpText(" Defines type of spline to be rendered.")]
			public SplineType SplineType { get; set; } = SplineType.Natural;

			[HelpText("This property allows grouping series in `stacked column / bar` charts.")]
			public string StackingGroup { get; set; }

			[HelpText("user can format now each series tooltip format separately.")]
			public string TooltipFormat { get; set; }

			[HelpText("The stroke width for the series that is applicable only for `Line` type series.")]
			public double Width { get; set; } = 1;

			[HelpText(" The shape of the legend. Each series has its own legend shape.")]
			public LegendShape LegendShape { get; set; }

			[HelpText("Options to customizing animation for the series.")]
			public ChartSeriesAnimation Animation { get; set; }

			[HelpText("Options to customizing the border of the series. This is applicable only for `Column` and `Bar` type series.")]
			public ChartSeriesBorder Border { get; set; }

			[HelpText("To render the column series points with particular column spacing. It takes value from 0 - 1.")]
			public double ColumnSpacing { get; set; } = 0;

			[HelpText("To render the column series points with particular rounded corner.")]
			public ChartCornerRadius CornerRadius { get; set; }

			[HelpText("The fill color for the series that accepts value in hex and rgba as a valid CSS color string.")]
			public string Fill { get; set; }

			[HelpText("Defines the pattern of dashes and gaps to stroke the lines in `Line` type series.")]
			public string DashArray { get; set; } = "0";

			[HelpText("Options to customize the drag settings for series")]
			public ChartDragSettings DragSettings { get; set; }
		}

		[HelpText("Options for chart series settings")]
		public ChartSeriesConfiguration SeriesSettings { get; set; } = new ChartSeriesConfiguration();

		[HelpText("Options for chart titles settings")]
		public ChartTitlesConfiguration ChartTitleSettings { get; set; } = new ChartTitlesConfiguration();

		[HelpText("Options for chart marker")]
		public ChartMarkerConfiguration ChartMarkerSettings { get; set; } = new ChartMarkerConfiguration();

		[HelpText("The height of the chart as a string accepts input both as '100px' or '100%'. If specified as '100%, chart renders to the full height of its parent element.")]
		public string Height { get; set; }

		[HelpText("The width of the chart as a string accepts input as both like '100px' or '100%'. If specified as '100%, chart renders to the full width of its parent element.")]
		public string Width { get; set; }

		[HelpText("To render the column series points with particular column spacing. It takes value from 0 - 1.")]
		public double ColumnSpacing { get; set; } = 0;

		public SelectionMode SelectionMode { get; set; } = SelectionMode.Series;

		public bool IsEnableTooltip { get { return TooltipSettings.IsEnable; } set { TooltipSettings.IsEnable = value; } }

		public TooltipConfiguration TooltipSettings { get; set; } = new TooltipConfiguration();

		[HelpText("Options for customizing the legend of the chart.")]
		public ChartLegendSettings LegendConfigurations { get; set; } = new ChartLegendSettings(){ Visible = true, Position=LegendPosition.Bottom, Alignment=Alignment.Near};

		[HelpText("Palette for the chart series.")]
		public string[] Palettes { get; set; }

		public ChartSeriesType ChartSeriesType { get; set; } = ChartSeriesType.StackingColumn;

		public bool IsShowMarker { get { return ChartMarkerSettings.Visible; } set { ChartMarkerSettings.Visible = value; } }

		public string ChartBackground { get; set; } = "transparent";

		public bool EnableMouseWheelZooming { get; set; }

		public bool EnablePinchZooming { get; set; }

		public bool EnableSelectionZooming { get; set; }

		public bool EnablePan { get; set; }

		public bool EnableScrollbarZooming { get; set; }

		public bool IsEnableCrosshair { get; set; }

		public PrimaryXAxis XAxisSettings { get; set; } = new PrimaryXAxis();

		public PrimaryYAxis YAxisSettings { get; set; } = new PrimaryYAxis();


		public ChartMargin Margin { get; set; } = new ChartMargin();

		public ChartAreaBorder Border { get; set; } = new ChartAreaBorder() { Width = 0 };

		public ReportSyncfusionBaseChartWidget()
		{
		}
	}
}
