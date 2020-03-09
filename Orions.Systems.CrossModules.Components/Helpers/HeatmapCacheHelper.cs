using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Orions.Node.Common;
using Orions.Infrastructure.HyperMedia;

namespace Orions.Systems.CrossModules.Components.Helpers
{
	public class HeatmapCacheHelper
	{
		private IHyperArgsSink _store;
		private MasksHeatmapRenderer.HeatmapSettings _heatmapSettings;
		private double _percentageCompleted = 0;

		public event Action<double> GenerationProgress;

		public HeatmapCacheHelper(IHyperArgsSink store, MasksHeatmapRenderer.HeatmapSettings heatmapSettings)
		{
			_store = store;
			_heatmapSettings = heatmapSettings;
		}

		public async Task<HeatmapStepCache> CreateCache(DateTime startDate, DateTime endDate, HyperMetadataSet metadataSet, TimeSpan stepLength)
		{
			_percentageCompleted = 0;
			GenerationProgress?.Invoke(0);

			var newCache = new HeatmapStepCache()
			{
				MetadataSetId = metadataSet.Id
			};

			var stepToTasksMappings = new Dictionary<HeatmapCacheStep, MasksHeatmapRenderer>();
			var tasks = new List<Task>();
			var tasksCompleted = 0;
			while(startDate < endDate)
			{
				var step = new HeatmapCacheStep
				{
					From = startDate,
					To = startDate + stepLength > endDate ? endDate : startDate + stepLength
				};

				var renderer = new MasksHeatmapRenderer(_store, metadataSet, _heatmapSettings);
				renderer.PertcantageProcessedUpdated += (percentageLambda) =>
				{
					_percentageCompleted += percentageLambda / tasks.Count;
					this.GenerationProgress?.Invoke(_percentageCompleted);
				};
				var task = renderer.RunGenerationAsync(step.From, step.To)
					.ContinueWith(t =>
					{
						tasksCompleted++;
					});
				stepToTasksMappings[step] = renderer;
				tasks.Add(task);

				startDate = startDate + stepLength;

				System.Diagnostics.Debug.WriteLine("Cache heatmap task added");
			}

			await Task.WhenAll(tasks);

			newCache.Steps.AddRange(stepToTasksMappings.Keys);
			foreach(var kv in stepToTasksMappings)
			{
				var step = kv.Key;
				step.ImageData = kv.Value.ImageProp.Value;
			}

			return newCache;
		}
	}

	public class HeatmapStepCache
	{
		public List<HeatmapCacheStep> Steps { get; set; } = new List<HeatmapCacheStep>();
		public string MetadataSetId { get; set; }
	}

	public class HeatmapCacheStep
	{
		public DateTime From { get; set; }
		public DateTime To { get; set; }
		public byte[] ImageData { get; set; }
	}
}
