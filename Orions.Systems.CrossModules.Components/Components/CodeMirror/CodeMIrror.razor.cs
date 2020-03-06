using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

using System;
using System.Threading.Tasks;

namespace Orions.Systems.CrossModules.Components
{
	public class CodeMirrorBase : BaseBlazorComponent<CodeMirrorVm>, IDisposable
	{
		[Parameter]
		public string Data 
		{ 
			get { return Config.Data; } 
			set { Config.Data = value; } 
		}

		protected IDisposable thisReference;
		protected CodeMirrorConfig Config { get; set; } = new CodeMirrorConfig();

		protected override async Task OnFirstAfterRenderAsync()
		{
			Config.Ref = Ref;

			thisReference = DotNetObjectReference.Create(this);

			await JsInterop.InvokeAsync<object>("Orions.CodeMirror.init", new object[] { thisReference, Config });
		}

		void IDisposable.Dispose()
		{
			thisReference?.Dispose();
		}
	}


	public class CodeMirrorConfig
	{
		public ElementReference Ref { get; set; }

		public string Name { get; set; }

		public string Data { get; set; }
	}
}
