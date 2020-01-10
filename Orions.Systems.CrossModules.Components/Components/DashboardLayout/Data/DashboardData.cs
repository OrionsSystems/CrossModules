using Orions.Common;
using System.Collections.Generic;

namespace Orions.Systems.CrossModules.Components
{
   public class DashboardData : IdUnifiedBlob, IName
   {
      [DocumentDescriptor]
      public string Name { get { return Option.Name; } set { value = Option.Name; } } 

      public DashboardOption Option { get; set; } = new DashboardOption();

      public LinkedList<DashboardRow> Rows { get; set; }

      public DashboardData()
      {
         Rows = new LinkedList<DashboardRow>();
      }

      public override string ToString()
      {
         return this.Name;
      }
   }

   public class DashboardRow : IId
   {
      public string Id { get; set; } = IdGeneratorHelper.Generate("db-row-");

      public LinkedList<DashboardColumn> Columns { get; set; }

      public bool ShowCommands { get; set; }

      public DashboardRow()
      {
         Columns = new LinkedList<DashboardColumn>();
      }
   }

   public class DashboardColumn : IId
   {
      public string Id { get; set; } = IdGeneratorHelper.Generate("db-col-");
      public int Size { get; set; }
      public int Order { get; set; }
      public bool ShowCommands { get; set; }

      public bool ShowBetweenCommands { get; set; }

      public IDashboardWidget Widget { get; set; }
   }
}
