using System.Threading.Tasks;
using Microsoft.JSInterop;

namespace Orions.Systems.CrossModules.Components
{
	public class ExampleJsInterop
	{
		public static ValueTask<string> Prompt(IJSRuntime jsRuntime, string message)
		{
			// Implemented in exampleJsInterop.js
			return jsRuntime.InvokeAsync<string>(
				 "exampleJsFunctions.showPrompt",
				 message);
		}
	}
}
