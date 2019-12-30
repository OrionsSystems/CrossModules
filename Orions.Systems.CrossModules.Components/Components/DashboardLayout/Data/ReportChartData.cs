using System;
using System.Collections.Generic;
using System.Text;

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

    public class ReportLineChartData
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
    }
}
