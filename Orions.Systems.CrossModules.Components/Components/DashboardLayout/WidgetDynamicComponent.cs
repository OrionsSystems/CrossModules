using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Orions.Node.Common;
using System;
using System.Threading.Tasks;

namespace Orions.Systems.CrossModules.Components
{
	public class WidgetDynamicComponent : BaseBlazorComponent
	{
		/// <summary>
		/// The product we want to render
		/// </summary>
		[Parameter]
		public IDashboardWidget Widget { get; set; }

		[Parameter]
		public IHyperArgsSink HyperStore { get; set; }

		protected override Task OnInitializedAsync()
		{
			return base.OnInitializedAsync();
		}

		/// <summary>
		/// Render the component
		/// </summary>
		/// <param name="builder"></param>
		protected override void BuildRenderTree(RenderTreeBuilder builder)
		{
			base.BuildRenderTree(builder);
			// get the component to view the product with
			Type componentType = Widget.GetViewComponent();
			// create an instance of this component
			builder.OpenComponent(0, componentType);
			// set the `Widget` attribute of the component
			builder.AddAttribute(1, "Widget", Widget);
			// set the `HyperStore` attribute of the component
			builder.AddAttribute(1, "HyperStore", HyperStore);
			// close
			builder.CloseComponent();
		}
	}
}
