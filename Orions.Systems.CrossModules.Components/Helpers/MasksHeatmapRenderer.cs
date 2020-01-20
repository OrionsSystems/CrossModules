using Orions.Common;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Text;
using System.Linq;
using SkiaSharp;
using System.Threading.Tasks;
using System.Threading;
using Orions.Infrastructure.HyperMedia;
using Orions.Node.Common;
using Orions.Infrastructure.HyperSemantic;

namespace Orions.Systems.CrossModules.Components.Helpers
{
	public class MasksHeatmapRenderer : IDisposable
	{
		private IHyperArgsSink _hyperStore;
		private HyperMetadataSet _metadataSet;
		private AsyncManualResetEvent _pauseResetEvent = new AsyncManualResetEvent(true);
		private HyperId _lastHyperId;
		private bool _keepWorking;
		private HeatmapSettings _settings { get; set; } = new HeatmapSettings();

		private CancellationTokenSource _ctSource;
		private CancellationToken _ct => _ctSource.Token;

		private Task<bool> _completionTask;

		private int _width = 0;
		private int _height = 0;
		private uint[,] _globalMatrix;
		private UniImage _lastImage;

		public MasksHeatmapRenderer(IHyperArgsSink hyperStore, HyperMetadataSet metadataSet, HeatmapSettings settings)
		{
			_hyperStore = hyperStore;
			_metadataSet = metadataSet;
			_settings = settings;
		}

		public ViewModelProperty<long> TotalCountProp { get; set; } = new ViewModelProperty<long>(0);
		public ViewModelProperty<string> StatusProp { get; set; } = new ViewModelProperty<string>();
		public ViewModelProperty<byte[]> ImageProp { get; set; } = new ViewModelProperty<byte[]>();
		public ViewModelProperty<long> ItemsProcessed { get; set; } = new ViewModelProperty<long>(0);
		public ViewModelProperty<string> PertcantageLabel { get; set; } = new ViewModelProperty<string>(string.Empty);

		public class HeatmapSettings
		{
			[HelpText("If we should override normalization values in masks mode. If disabled, minimum and maximum number of detected overlaps per pixel is used.")]
			public bool UseCustomNormalizationSettings { get; set; } = false;
			[HelpText("Used only if custom normalization is enabled. Pixels that are overlappes less times than this value will not be covered with a mask.")]
			public uint MinimumNumberOfOverlaps { get; set; } = 1;

			[HelpText("Used only if custom normalization is enabled. Any number of overlaps that is equal to this or higher than this considered to be the hottest possible value.")]
			public uint MaximumNumberOfOverlaps { get; set; } = 100;
			public int NumberOfBatchesToProcess { get; set; } = 5;
		}

		public void CancelGeneration()
		{
			_keepWorking = false;
			_ctSource.Cancel();
		}

		public async Task RunGenerationAsync()
		{
			_keepWorking = true;
			_ctSource = new CancellationTokenSource();

			var source = new TaskCompletionSource<bool>();
			_completionTask = source.Task;

			try
			{
				ProtectOriginalMetadataSet();

				TryAddTagType(nameof(HyperTagGeometry), nameof(HyperTagGeometryMask));

				await CountTagsTotalAsync();

				var batchSize = 100;
				var batchesNum = Math.Ceiling((double)TotalCountProp.Value / batchSize);
				batchesNum = Math.Min(batchesNum, _settings.NumberOfBatchesToProcess);

				var hyperIds = new List<HyperId>();

				for (int i = 0; i < batchesNum; i++)
				{
					StatusProp.Value = $"Processing batch {(i + 1)} out of {batchesNum}...";

					if (!_keepWorking || _ct.IsCancellationRequested)
						return;

					var skip = batchSize * i;

					var findArgs = new FindHyperDocumentsArgs(typeof(HyperTag))
					{
						Skip = skip,
						Limit = batchSize
					};

					var conditions = await MetaDataSetHelper.GenerateFilterFromMetaDataSetAsync(_hyperStore, _metadataSet);
					findArgs.DescriptorConditions.AddCondition(conditions);

					//hyper tag ids condition
					var hyperTagIdscondition = new MultiScopeCondition(AndOr.Or);
					foreach (var id in _metadataSet.HyperTagIds ?? new HyperDocumentId[] { })
					{
						hyperTagIdscondition.AddCondition("_id", id.Id);
					}
					if (hyperTagIdscondition.ConditionsCount > 0)
					{
						findArgs.DocumentConditions.AddCondition(hyperTagIdscondition);
					}

					var tagDocs = await _hyperStore.ExecuteAsync(findArgs) ?? new HyperDocument[0];

					if (!_keepWorking || _ct.IsCancellationRequested)
						return;

					var tags = tagDocs.Select(it => it.GetPayload<HyperTag>()).ToArray();
					var grouppedBySlice = tags.GroupBy(it =>
					{
						var item = it.GetElement<IHyperTagHyperIds>();
						return item.HyperId;
					}).ToArray();

					if (!_keepWorking || _ct.IsCancellationRequested)
						return;

					if (_width == 0 || _height == 0)
					{
						var hyperId = grouppedBySlice.First().Key;

						_lastImage = await GetImageAsync(hyperId);

						if (_lastImage.Width > 0 && _lastImage.Height > 0)
						{
							_width = _lastImage.Width.Value;
							_height = _lastImage.Height.Value;
						}
						else
						{
							using (var bitmap = SKBitmap.Decode(_lastImage.Data))
							{
								_width = bitmap.Width;
								_height = bitmap.Height;
							}
						}

						_globalMatrix = new uint[_height, _width];

						ImageProp.Value = _lastImage.Data;

						if (!_keepWorking || _ct.IsCancellationRequested)
							return;
					}

					grouppedBySlice = grouppedBySlice.OrderBy(x => x.Key).ToArray();

					// Group by asset first?

					var groupedByFragment = grouppedBySlice.GroupBy(x => x.Key.FragmentId.Value).ToArray();

					foreach (var group in groupedByFragment)
					{
						var sliceGroupsOfFragment = group.ToArray();

						foreach (var sliceTags in sliceGroupsOfFragment)
						{
							await _pauseResetEvent.WaitAsync();

							foreach (var tag in sliceTags)
							{
								ProcessMaskedTag(tag);
							}
						}

						if (!_keepWorking || _ct.IsCancellationRequested)
							return;
					}

					_lastHyperId = grouppedBySlice.Last().Key;

					await RerenderAsync();

					ItemsProcessed.Value += tagDocs.Count();
					PertcantageLabel.Value = ((double)100 * ItemsProcessed.Value / TotalCountProp.Value).ToString("0.00", System.Globalization.CultureInfo.InvariantCulture) + "%";
				}
			}
			catch (Exception ex)
			{
				Logger.Instance.Error(this.GetType(), nameof(RunGenerationAsync), ex.Message);
			}
			finally
			{
				source.SetResult(true);
			}
		}

		private async Task<UniImage> GetImageAsync(HyperId hyperId)
		{
			var args = new RetrieveFragmentFramesArgs
			{
				AssetId = hyperId.AssetId.Value,
				FragmentId = hyperId.FragmentId.Value,
				SliceIds = new HyperSliceId[] { hyperId.SliceId.Value }
			};

			var res = await _hyperStore.ExecuteAsyncThrows(args);

			return res.First().Image;
		}

		private async Task RerenderAsync()
		{
			StatusProp.Value = "Rendering heatmap...";

			_lastImage = await GetImageAsync(_lastHyperId);

			using (var skBitmap = SKBitmap.Decode(_lastImage.Data))
			{
				SKImage heatmap;
				SKImage result;

				heatmap = GenerateMaskFromValuesMatrix(_globalMatrix, _width, _height,
						_settings.UseCustomNormalizationSettings ? _settings.MinimumNumberOfOverlaps : (uint?)null,
						_settings.UseCustomNormalizationSettings ? _settings.MaximumNumberOfOverlaps : (uint?)null);

				using (var tempSurface = SKSurface.Create(new SKImageInfo(heatmap.Width, heatmap.Height)))
				using (heatmap)
				{
					//get the drawing canvas of the surface
					var canvas = tempSurface.Canvas;

					var skPaint = new SKPaint
					{
						BlendMode = SKBlendMode.Plus
					};

					//set background color
					canvas.Clear(SKColors.Transparent);

					canvas.DrawBitmap(skBitmap, 0, 0, skPaint);
					canvas.DrawImage(heatmap, 0, 0, skPaint);

					// return the surface as a manageable image
					result = tempSurface.Snapshot();
				}

				UniImage image;

				using (result)
				{
					using (var skData = result.Encode(SKEncodedImageFormat.Jpeg, 70))
					{
						var bytes = skData.ToArray();
						image = new UniImage(ImageFormats.Jpeg, bytes, result.Width, result.Height);
					}
				}

				//SkImageProp.Value = result;
				ImageProp.Value = image.Data;
				StatusProp.Value = "Finished updating heatmap.";
			}
		}

		private void ProcessMaskedTag(HyperTag tag)
		{
			var geometry = tag.GetElement<HyperTagGeometry>();
			var geometryMask = tag.GetElement<HyperTagGeometryMask>();

			var classification = tag.GetElements<HyperTagLabel>().Where(x => x.Type == HyperTagLabel.Types.Classification).Select(x => x.Label).FirstOrDefault();

			if (classification == null)
			{
				classification = tag.GetElement<TagonomyExecutionResultHyperTagElement>()?.GetCombinedLabel();
			}

			var rect = geometry.GeometryItem.BoundingBox;
			rect = geometry.GeometryItem.ConvertFromAbsoluteXSpaceToRealWorldSpace(rect, _width, _height);

			var skBitmap = SKBitmap.Decode(geometryMask.RawData);

			if (skBitmap.Width != geometryMask.Width || skBitmap.Height != geometryMask.Height)
			{
				var newSkBitmap = new SKBitmap((int)rect.Width, (int)rect.Height);
				using (skBitmap)
				{
					SKBitmap.Resize(newSkBitmap, skBitmap, SKBitmapResizeMethod.Lanczos3);
				}
				skBitmap = newSkBitmap;
			}

			IntPtr pixelsAddr = skBitmap.GetPixels();

			var topOffset = rect.Top;
			var bottomOffset = rect.Left;

			unsafe
			{
				byte* ptr = (byte*)pixelsAddr.ToPointer();
				//uint* ptr = (uint*)pixelsAddr.ToPointer();

				for (int row = 0; row < skBitmap.Height; row++)
					for (int col = 0; col < skBitmap.Width; col++)
					{
						//var pixel = *ptr++;
						var blue = *ptr++;   // blue
						var green = *ptr++;             // green
						var red = *ptr++;   // red
						var alpha = *ptr++;          // alpha
						if (!(alpha == 255 && blue == 0 && green == 0 && red == 0) || alpha == 0 /*pixel != 0*/)
						{
							var realY = row + (int)topOffset;
							var realX = col + (int)bottomOffset;

							_globalMatrix[realY, realX]++;
						}
					}
			}
		}

		/// <summary>
		/// Tries to duplicate metadata set object to avoid modifying original one with local changes.
		/// </summary>
		private void ProtectOriginalMetadataSet()
		{
			_metadataSet = _metadataSet.ShallowCopy();
			_metadataSet.TagTypes = (string[])_metadataSet.TagTypes.Clone();
		}

		/// <summary>
		/// Adds new tag types to metadata set if they are not added yet.
		/// </summary>
		private void TryAddTagType(params string[] tagNames)
		{
			foreach (var tagName in tagNames)
			{
				if (!_metadataSet.TagTypes.Any(x => x == tagName))
				{
					_metadataSet.TagTypesEx.Add(tagName);
				}
			}
		}

		/// <summary>
		/// Calculates total amount of tags according to metadata set.
		/// </summary>
		/// <returns></returns>
		private async Task CountTagsTotalAsync()
		{
			StatusProp.Value = "Calculating total tags count...";
			var countArgs = new CountHyperDocumentsArgs(typeof(HyperTag));
			await ApplyFilterConditions(countArgs);
			var totalTags = await CountHyperDocumentsArgs.CountAsync<HyperTag>(_hyperStore, countArgs);
			TotalCountProp.Value = totalTags;
			StatusProp.Value = string.Empty;
		}

		private async Task ApplyFilterConditions<T>(ScopeHyperDocumentsArgs<T> mainArgs)
		{
			var conditions = await MetaDataSetHelper.GenerateFilterFromMetaDataSetAsync(_hyperStore, _metadataSet);
			mainArgs.DescriptorConditions.AddCondition(conditions);

			//hyper tag ids condition
			var hyperTagIdscondition = new MultiScopeCondition(AndOr.Or);
			foreach (var id in _metadataSet.HyperTagIds ?? new HyperDocumentId[] { })
			{
				hyperTagIdscondition.AddCondition("_id", id.Id);
			}
			if (hyperTagIdscondition.ConditionsCount > 0)
			{
				mainArgs.DocumentConditions.AddCondition(hyperTagIdscondition);
			}
		}

		public static SKImage GenerateMaskFromValuesMatrix(uint[,] matrix, int width, int height, uint? overrideNormalizationMin, uint? overrideNormalizationMax)
		{
			var minValue = overrideNormalizationMin ?? matrix.Cast<uint>().Where(x => x > 0).Min();
			var maxValue = overrideNormalizationMax ?? matrix.Cast<uint>().Max();

			var steps = maxValue - minValue + 1;

			var pallete = GenerateGradient(new GradientStop[]
			{
				new GradientStop { Color = Color.FromArgb(122, 0, 0, 255), Position = 0 },
					new GradientStop { Color = Color.FromArgb(255, 0, 0, 255), Position = 0.16 },
					new GradientStop { Color = Color.Green, Position = 0.33 },
					new GradientStop { Color = Color.Yellow, Position = 0.66 },
					new GradientStop { Color = Color.Red, Position = 1 } },
					steps);

			var skBitmap = new SKBitmap(width, height);
			var pixelsAddr = skBitmap.GetPixels();

			unsafe
			{
				byte* ptr = (byte*)pixelsAddr.ToPointer();

				for (int row = 0; row < height; row++)
					for (int col = 0; col < width; col++)
					{
						var originalValue = matrix[row, col];

						if (originalValue < minValue)
						{
							*ptr++ = 0;   // blue
							*ptr++ = 0;             // green
							*ptr++ = 0;   // red
							*ptr++ = 0;          // alpha
						}
						else
						{
							if (originalValue > maxValue)
								originalValue = maxValue;

							var gradientNumber = originalValue - minValue;
							var color = pallete[gradientNumber];

							*ptr++ = color.B;   // blue
							*ptr++ = color.G;             // green
							*ptr++ = color.R;   // red
							*ptr++ = color.A;          // alpha
						}
					}
			}

			var image = SKImage.FromBitmap(skBitmap);

			skBitmap.Dispose();

			//using (var data = image.Encode(SKEncodedImageFormat.Png, 80))
			//{
			//	// save the data to a stream
			//	using (var stream = File.OpenWrite(@"C:/temp/mask.png"))
			//	{
			//		data.SaveTo(stream);
			//	}
			//}

			return image;
		}

		public class GradientStop
		{
			/// <summary>
			/// 0 to 1
			/// </summary>
			public double Position { get; set; }

			public Color Color { get; set; }
		}

		private static Color[] GenerateGradient(GradientStop[] stops, uint steps)
		{
			Color[] results = new Color[steps];

			for (int i = 0; i < stops.Length - 1; i++)
			{
				var stop = stops[i];
				var stopNext = stops[i + 1];

				int startStep = (int)(stop.Position * steps);
				int endStep = (int)(stopNext.Position * steps);

				int range = endStep - startStep;
				for (int x = 0; x < range; x++)
				{
					double blendFactor1 = (double)x / (double)range;
					double blendFactor2 = 1 - blendFactor1;

					double r = stop.Color.R * blendFactor2 + stopNext.Color.R * blendFactor1;
					double g = stop.Color.G * blendFactor2 + stopNext.Color.G * blendFactor1;
					double b = stop.Color.B * blendFactor2 + stopNext.Color.B * blendFactor1;

					r = Math.Min(255, r);
					g = Math.Min(255, g);
					b = Math.Min(255, b);

					results[x + startStep] = Color.FromArgb(255, (int)r, (int)g, (int)b);
				}
			}

			return results;
		}

		private static ColorMap[] CreatePaletteIndex(UniColor? startColor = null)
		{
			if (startColor == null)
				startColor = UniColor.Red;

			var pallete = GenerateGradient(new GradientStop[]
			{
					new GradientStop() { Color = Color.FromArgb(0, 0, 0, 0), Position = 0 },
					new GradientStop() { Color = Color.Blue, Position = 0.44 },
					new GradientStop() { Color = Color.Green, Position = 0.6 },
					new GradientStop() { Color = Color.Yellow, Position = 0.8 },
					new GradientStop() { Color = Color.FromArgb(255, startColor.Value.R, startColor.Value.G,startColor.Value.B), Position = 1 } },
					256);

			// The pallete from the JS lib - http://mourner.github.io/simpleheat/demo/
			//			var pallete = GenerateGradient(new GradientStop[]
			//{
			//				new GradientStop() { Color = Color.Black, Position = 0 },
			//				new GradientStop() { Color = Color.Blue, Position = 0.4 },
			//				new GradientStop() { Color = Color.Cyan, Position = 0.6 },
			//				new GradientStop() { Color = Color.Lime, Position = 0.7 },
			//				new GradientStop() { Color = Color.Yellow, Position = 0.8 },
			//				new GradientStop() { Color = Color.Red, Position = 1 } }, 256);

			var outputMap = new ColorMap[256];

			// Loop through each pixel and create a new color mapping
			for (int X = 0; X <= 255; X++)
			{
				outputMap[X] = new ColorMap();
				outputMap[X].OldColor = Color.FromArgb(X, X, X);
				outputMap[X].NewColor = pallete[X];
			}

			return outputMap;
		}

		public void Dispose()
		{
			CancelGeneration();
		}
	}
}
