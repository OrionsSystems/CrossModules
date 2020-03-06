using System;
using System.Collections.Generic;

namespace Orions.Systems.CrossModules.Components
{
    public class Carousels
    {
        public string Source { get; set; }
        public string Alt { get; set; }
        public string Caption { get; set; }
        public string Header { get; set; }
        public string ActionLink { get; set; }
        public string ActionLinkTarget { get; set; }
    }

    public class ReportData
    {
        public int RowCount { get; set; }
        public List<ReportSeriesData> Series { get; private set; } = new List<ReportSeriesData>();

        public void Clean()
        {
            Series.Clear();
        }
    }

    public class ReportSeriesData
    {
        public string HeaderTitle { get; set; }
        public List<ReportSeriesDataItem> Data { get; set; } = new List<ReportSeriesDataItem>();
    }

    public class ReportSeriesDataItem
    {
        public string Name { get; set; }
        public string Value { get; set; }
        public ReportSeriesDataItem()
        {
        }

    }
}
