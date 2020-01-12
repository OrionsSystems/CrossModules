using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Orions.Common;
using Orions.Node.Common;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Orions.Systems.CrossModules.Components
{
	public class WidgetDynamicComponent : BaseBlazorComponent
	{
		static readonly object g_syncRoot = new object();

		static Type[] _widgetComponentTypes = null;

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

			Type[] componentTypes;
			lock (g_syncRoot)
			{
				if (_widgetComponentTypes == null)
				{// Cache this as it scans very many types.
					_widgetComponentTypes = ReflectionHelper.Instance.GatherTypeChildrenTypesFromAssemblies(typeof(IWidgetComponent)).ToArray();
				}

				componentTypes = _widgetComponentTypes;
			}

			var componentType = componentTypes.First(it => it.BaseType.GetGenericArguments().Length == 2 && it.BaseType.GetGenericArguments()[1] == Widget.GetType());

			// get the component to view the product with, based on the Config attribute system.
			//Type componentType = ConfigAttribute.FindTypeByConfigType<WidgetVm>(Widget.GetType());

			// create an instance of this component
			builder.OpenComponent(0, componentType);

			// set the `Widget` attribute of the component
			// we are using fake generic prototypes here, but it does not matter for the property name
			builder.AddAttribute(1, nameof(WidgetComponent<WidgetVm<DashboardWidgetBase>, DashboardWidgetBase>.Widget), Widget);

			// set the `HyperStore` attribute of the component
			// we are using fake generic prototypes here, but it does not matter for the property name
			builder.AddAttribute(1, nameof(WidgetComponent<WidgetVm<DashboardWidgetBase>, DashboardWidgetBase>.HyperStore), HyperStore);

			// close
			builder.CloseComponent();
		}
	}
}
