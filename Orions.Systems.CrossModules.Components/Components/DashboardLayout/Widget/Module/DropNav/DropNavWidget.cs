using Orions.Common;
using System.Collections.Generic;

namespace Orions.Systems.CrossModules.Components
{
   public class DropNavWidget : DashboardWidget
   {
      [HelpText("Navigation items")]
      public List<NavigationItem> Navigations { get; set; } = new List<NavigationItem>();

      [HelpText("Gets or sets the index of the selected item in the component.")]
      public string Placeholder { get; set; }

      [HelpText("When allowFiltering is set to true, show the filter bar (search box) of the component.  The filter action retrieves matched items through the `filtering` event based  on the characters typed in the search TextBox.")]
      public bool AllowFiltering { get; set; }

      [HelpText("Specifies the height of the popup list.")]
      public string PopupHeight { get; set; } = "auto";

      [HelpText("Specifies the width of the popup list.")]
      public string PopupWidth { get; set; } = "100%";

      [HelpText("Sets CSS classes to the root element of the component that allows customization of appearance.")]
      public string CssClass { get; set; }

      [HelpText(" Specifies whether to show or hide the clear button. ")]
      public bool ShowClearButton { get; set; } = true;

      [HelpText(" When set to true, the user interactions on the component are disabled.")]
      public bool Readonly { get; set; }

      public class NavigationItem
      {
         [HelpText("Linked view")]
         public string View { get; set; }

         [HelpText("Navigation label")]
         public string Label { get; set; }
      }

      public DropNavWidget()
      {
         this.Label = "Dropdown Navigation";
      }
   }
}
