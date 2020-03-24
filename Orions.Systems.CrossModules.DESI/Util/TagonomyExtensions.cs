using System.Collections.Generic;
using System.Linq;
using Orions.Infrastructure.HyperSemantic;

namespace Orions.Systems.CrossModules.Desi.Util
{
	public static class TagonomyExtensions
	{
		public static string GetTitle(this TagonomyExecutionStep tagonomyExecutionStep, IEnumerable<TagonomyExecutionStep> allSteps)
		{
            var last = allSteps?.LastOrDefault();
            var text = tagonomyExecutionStep?.OptionalTargetNodeName ?? string.Empty;

            if (tagonomyExecutionStep?.Actions?.FirstOrDefault(i => i.Result is UserActionNodeElement.ElementUniformResult) is
                    TagonomyNodeElementAction action
                && action.Result is UserActionNodeElement.ElementUniformResult result
                && !string.IsNullOrEmpty(result.Text))
            {
                text += $": {result.Text}";
            }

            if (tagonomyExecutionStep != last)
            {
                text += ":";
            }

            return text;
        }
	}
}
