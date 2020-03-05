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

      public override string GetIcon()
      {
         return "<svg version=\"1.1\" class=\"icon-dropnav\" xmlns=\"http://www.w3.org/2000/svg\" xmlns:xlink=\"http://www.w3.org/1999/xlink\" x=\"0px\" y=\"0px\"   " +
            "viewBox=\"0 0 150 150\" style=\"enable-background:new 0 0 150 150;\" xml:space=\"preserve\"> " +
            "<path class=\"icon-dropnav-item-0\" d=\"M95.7,39c-25,4.5-32.2,12.5-33.9,18.2c-1,3.2,0,6.6,2.4,8.9c3.6,3.5,11.5,9.8,22.9,12.2c9.6,2,2.2,6-5.7,8.9  " +
            "c-5.5,2.1-11.2,3.6-17,4.8c-22.3,4.3-101.1,21.9-43.3,47c0.3,0.1,0.5,0.2,0.8,0.2h113.5c2,0,2.7-2.6,1.1-3.6  " +
            "c-9.6-6.4-24-20.8,3.2-37.7c34.2-21.2-30.2-34.9-42.3-36.6c-2.6-0.4-5-1.1-7.3-2.5c-5.6-3.3-10.2-10,13.1-19.7L95.7,39L95.7,39  L95.7,39z\"/> " +
            "<path class=\"icon-dropnav-item-1\" d=\"M39.7,90.3c-4.9,8.4-8.3,17.5-10.2,23.5c-0.6,1.9-3.2,1.9-3.8,0c-1.9-5.9-5.3-15.1-10.2-23.5 " +
            " c-5.3-9.1,0.2-21,10.7-21.8c0.5,0,0.9-0.1,1.4-0.1s0.9,0,1.4,0.1C39.5,69.3,45,81.3,39.7,90.3z\"/> " +
            "<circle class=\"icon-dropnav-item-2\" cx=\"27.6\" cy=\"82.4\" r=\"10\"/> " +
            "<path class=\"icon-dropnav-item-3\" d=\"M139.3,57.5C134.4,65.9,131,75,129.1,81c-0.6,1.9-3.2,1.9-3.8,0c-1.9-5.9-5.3-15.1-10.2-23.5  c-5.3-9.1,0.2-21,10.7-21.8c0.5,0,0.9-0.1,1.4-0.1s0.9,0,1.4,0.1C139.1,36.5,144.6,48.4,139.3,57.5z\"/> " +
            "<circle class=\"icon-dropnav-item-2\" cx=\"127.2\" cy=\"49.5\" r=\"10\"/> " +
            "<path class=\"icon-dropnav-item-4\" d=\"M83.8,35.1c-4.5,7.7-7.6,16-9.3,21.5c-0.5,1.7-2.9,1.7-3.5,0c-1.8-5.5-4.8-13.8-9.3-21.5  c-4.8-8.3,0.2-19.2,9.8-20c0.4,0,0.9-0.1,1.3-0.1c0.4,0,0.9,0,1.3,0.1C83.6,15.9,88.6,26.8,83.8,35.1z\"/> " +
            "<circle class=\"icon-dropnav-item-2\" cx=\"72.7\" cy=\"27.9\" r=\"9.1\"/> <g>  " +
            "<path class=\"icon-dropnav-item-5\" d=\"M114.3,99.4l-3.8-2.4c3.4-2.2,5.1-4.6,5.2-6.9c0.2-3.6-3.3-6.7-4.4-7.5l1.7-1.4c0.2,0.1,5.7,3.5,6,8.9   C119,93.8,118.7,95.8,114.3,99.4z\"/>  " +
            "<path class=\"icon-dropnav-item-5\" d=\"M101.4,76.6c-1.3-0.4-11.4-3.5-12.9-7.1l2.6-1.4c1,1.6,8.7,5.1,12.4,6L101.4,76.6z\"/>  " +
            "<path class=\"icon-dropnav-item-5\" d=\"M73.5,119.3l0.6-3.6c15.5,0.1,24.3-9.5,24.4-9.6l2.8,1.7C101,108.2,91.6,119.3,73.5,119.3z\"/> </g> </svg>";
      }
   }
}
