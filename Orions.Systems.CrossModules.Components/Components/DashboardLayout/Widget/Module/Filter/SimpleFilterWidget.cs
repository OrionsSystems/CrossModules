using System;
using Orions.Infrastructure.HyperMedia;
using Orions.Infrastructure.Reporting;

namespace Orions.Systems.CrossModules.Components
{
    public class SimpleFilterWidget : ReportBaseWidget
    {
        public string[] Filters { get; set; }

        public ReportInstruction.Targets FilterTarget { get; set; }

        public SimpleFilterWidget()
        {
            this.Label = "Simple Filter";
        }
    }
}
