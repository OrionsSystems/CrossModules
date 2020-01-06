using System;
using System.Collections.Generic;

namespace Orions.Systems.CrossModules.Components
{
    public class ReportPieChartData
    {
        public int Count { get; set; }
        public string Label { get; set; }
        public bool ShouldShowInLegend { get; set; } = true;
        public bool Explode { get; set; }
        public string LabelInfo
        {
            get
            {
                return $"{Label}: {Count}";
            }
        }
    }

    public class ReportChartData
    {
        public List<string> Categories { get; private set; } = new List<string>();
        public List<ReportSeriesChartData> Series { get; private set; } = new List<ReportSeriesChartData>();

        public void Clean()
        {
            Categories.Clear();
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
        public int Count { get; set; }
        public string Time { get; set; }

        public DateTime DatePosition { get; set; }
        public string StreamPosition { get; set; }
    }
}
