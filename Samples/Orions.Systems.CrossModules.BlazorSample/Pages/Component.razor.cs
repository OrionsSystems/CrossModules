using Orions.Common;
using Orions.Systems.CrossModules.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Orions.Systems.CrossModules.BlazorSample.Pages
{
    public class ComponentBase : BaseBlazorComponent<ComponentVm>
    {
        protected override Task OnInitializedAsync()
        {
            this.DataContext.Initialize();

            return base.OnInitializedAsync();
        }
    }

    public class ComponentVm : BlazorVm
    {
        public ViewModelProperty<List<string>> SomeList { get; set; }
        public ViewModelProperty<string> SomeStr { get; set; } = "lalala";

        internal void Initialize()
        {
            SomeList = new List<string>() { "one", "two" };

            Task.Run(() =>
            {
                Thread.Sleep(5000);

                SomeStr.Value = "new str";
                SomeList.Value = new List<string>() { "one", "two", "three" };
            });
        }
    }
}
