using Orions.Common;

namespace Orions.Systems.CrossModules.Components
{
	public class MarginConfig
	{
		[HelpText("Assign top margin to an element in pixels.")]
		public int Top { get; set; }

		[HelpText("Assign right margin to an element in pixels.")]
		public int Right { get; set; }

		[HelpText("Assign bottom margin to an element in pixels.")]
		public int Bottom { get; set; }

		[HelpText("Assign left margin to an element in pixels.")]
		public int Left { get; set; }

		[HelpText("Enable or disable margin options")]
		public bool Enable { get; set; }

		public string GetInlineStyle()
		{
			if (!Enable) return string.Empty;

			return $"padding: {Top}px {Right}px {Bottom}px {Left}px";
		}

	}
}
