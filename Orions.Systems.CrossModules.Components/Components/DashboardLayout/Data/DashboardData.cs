using System.Collections.Generic;

namespace Orions.Systems.CrossModules.Components
{
    public class DashboardData
    {
        public LinkedList<DashboardRow> Rows { get; set; }

        public DashboardData()
        {
            Rows = new LinkedList<DashboardRow>();
        }
    }

    public class DashboardRow
    {
        public string Id { get; set; } = IdGeneratorHelper.Generate("db-row-");
        public LinkedList<DashboardColumn> Columns { get; set; }

        public DashboardRow()
        {
            Columns = new LinkedList<DashboardColumn>();
        }
    }

    public class DashboardColumn
    {
        public string Id { get; set; } = IdGeneratorHelper.Generate("db-col-");
        public int Size { get; set; }
        public int Order { get; set; }
        public bool ShowCommands { get; set; }

        public bool ShowBetweenCommands { get; set; }

        public IDashboardWidget Widget { get; set; }
    }
}
