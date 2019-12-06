using Microsoft.AspNetCore.Components.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Orions.Systems.CrossModules.Components
{
	public class DashboardVm : BlazorVm
	{
		public DashboardData Source { get; set; } = new DashboardData();

		public DashboardVm() { 
		
		}

        public void OnAddRow()
        {
            var row = new DashboardRow();
            row.Columns.AddLast(new DashboardColumn { Size = 12 });
            Source.Rows.Add(row);
        }

        public void SplitColumn(MouseEventArgs e, DashboardRow row, DashboardColumn column)
        {
            var size = column.Size % 2;
            var sizeF = column.Size / 2;

            column.Size = sizeF;
            column.ShowCommands = false;
            var newColumn = new DashboardColumn { Size = sizeF + size };

            var n = row.Columns.Find(column);
            row.Columns.AddAfter(n, newColumn);
        }

        public void DeleteColumn(MouseEventArgs e, DashboardRow row, DashboardColumn column)
        {
            var columnSize = column.Size;

            if (row.Columns.Count == 1)
            {
                Source.Rows.RemoveAll(it => it.Id == row.Id);
                return;
            }

            var n = row.Columns.Find(column);
            var prevColumn = n.Previous;

            row.Columns.Remove(column);

            if (prevColumn == null)
            {
                var firstCol = row.Columns.FirstOrDefault();
                firstCol.Size += columnSize;
                return;
            }

            prevColumn.Value.Size += columnSize;
        }

        public void ShowCommands(MouseEventArgs e, DashboardColumn column)
        {
            column.ShowCommands = true;
        }

        public void HideCommands(MouseEventArgs e, DashboardColumn column)
        {
            column.ShowCommands = false;
        }

        public void OnMouseDownDraging()
        {
            //TODO
        }

        public void OnMouseMoveDraging()
        {
            //TODO
        }

        public void OnMouseUpDraging()
        {
            //TODO
        }
    }
}
