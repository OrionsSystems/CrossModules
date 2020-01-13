using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Orions.Common;
using Orions.Node.Common;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Orions.Systems.CrossModules.Components
{
	public class WidgetDynamicComponent : DashboardComponent
	{
		static readonly object g_syncRoot = new object();

		static Type[] _widgetComponentTypes = null;

		public WidgetDynamicComponent()
		{
		}

		protected override void OnDataContextAssigned(BaseVm dataContext)
		{
			base.OnDataContextAssigned(dataContext);
		}

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
					_widgetComponentTypes = ReflectionHelper.Instance.GatherTypeChildrenTypesFromAssemblies(typeof(IDashboardComponent)).ToArray();
				}

				componentTypes = _widgetComponentTypes;
			}

			// Deduce compoennt type from Widget type.
			var componentType = componentTypes.First(it => it.BaseType.GetGenericArguments().Length == 2 && it.BaseType.GetGenericArguments()[1] == WidgetRaw.GetType());

			// get the component to view the product with, based on the Config attribute system.
			//Type componentType = ConfigAttribute.FindTypeByConfigType<WidgetVm>(Widget.GetType());

			// create an instance of this component
			builder.OpenComponent(0, componentType);

			// set the `Widget` attribute of the component
			builder.AddAttribute(1, nameof(DataContext), DataContext);

			// set the `Widget` attribute of the component
			builder.AddAttribute(1, nameof(WidgetRaw), WidgetRaw);

			// set the `HyperStore` attribute of the component
			builder.AddAttribute(1, nameof(HyperStore), HyperStore);

			// set the `HyperStore` attribute of the component
			builder.AddAttribute(1, nameof(DashboardVm), DashboardVm);

			// close
			builder.CloseComponent();
		}
	}
}
