using Microsoft.AspNetCore.Components.Rendering;

using Orions.Common;
using Orions.Systems.CrossModules.Components;

using System.Threading.Tasks;

namespace Orions.Systems.CrossModules.Portal.Components
{
	public class ViewModelControlSelectorControl : BaseBlazorComponent
	{

		public ViewModelControlSelectorControl()
		{

		}

		protected override void OnDataContextAssigned(object dataContext)
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

			var context = this.DataContext;
			if (context == null)
				return;

			//TODO add cache !! 

			var componentType = ViewModelAttribute.FindTypeByAttributeType<BaseBlazorComponent>(DataContext.GetType());

			// create an instance of this component
			builder.OpenComponent(0, componentType);

			// set the DataContext attribute
			builder.AddAttribute(1, nameof(DataContext), DataContext);

			// close
			builder.CloseComponent();
		}
	}
}
