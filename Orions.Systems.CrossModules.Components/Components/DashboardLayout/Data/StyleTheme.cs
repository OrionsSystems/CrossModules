using Orions.Common;
using System;
using System.Collections.Generic;

namespace Orions.Systems.CrossModules.Components
{
   public class StyleTheme : IdUnifiedBlob, IName, IGroup
   {
      [DocumentDescriptor]
      public string Name { get; set; } = "New Theme";

      [DocumentDescriptor]
      public string Group { get; set; }

      [DocumentDescriptor]
      public string Tag { get; set; }

      [HelpText("Apply css styles to the bottom of the page")]
      [UniBrowsable(UniBrowsableAttribute.EditTypes.MultiLineText)]
      public string Styles { get; set; }

      public StyleTheme()
      {
      }

      public override string ToString()
      {
         return this.Name;
      }
   }
}
