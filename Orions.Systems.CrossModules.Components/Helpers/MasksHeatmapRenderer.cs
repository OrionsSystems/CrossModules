﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Orions.Common;
using Orions.Common.Collections;
using Orions.Infrastructure.HyperMedia;
using Orions.Infrastructure.HyperSemantic;
using Orions.Node.Common;

using ReactNOW.ML;

using SkiaSharp;

using static ReactNOW.ML.HeatmapRenderHelper;

namespace Orions.Systems.CrossModules.Components.Helpers
{
	public class MasksHeatmapRenderer : IDisposable
	{
		public enum RenderingMode
		{
			Masks,
			RealImage,
			LowerPointOfGeometry
		}

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
		private ConcurrentFixedSizeQueue<List<HeatPoint>> _frames;
		private UniImage _lastImage;

		public MasksHeatmapRenderer(IHyperArgsSink hyperStore, HyperMetadataSet metadataSet, HeatmapSettings settings)
		{
			_hyperStore = hyperStore;
			_metadataSet = metadataSet;
			_settings = settings;

			_keepWorking = true;
			_ctSource = new CancellationTokenSource();
		}

		public ViewModelProperty<long> TotalCountProp { get; set; } = new ViewModelProperty<long>(0);
		public ViewModelProperty<string> StatusProp { get; set; } = new ViewModelProperty<string>();
		public ViewModelProperty<byte[]> ImageProp { get; set; } = new ViewModelProperty<byte[]>();
		public ViewModelProperty<long> ItemsProcessed { get; set; } = new ViewModelProperty<long>(0);
		public ViewModelProperty<string> PertcantageLabel { get; set; } = new ViewModelProperty<string>(string.Empty);
		public event Action<double> PertcantageProcessedUpdated;

		public class HeatmapSettings
		{
			[HelpText("If we should override normalization values in masks mode. If disabled, minimum and maximum number of detected overlaps per pixel is used.")]
			public bool UseCustomNormalizationSettings { get; set; } = false;
			[HelpText("Used only if custom normalization is enabled. Pixels that are overlappes less times than this value will not be covered with a mask.")]
			public uint MinimumNumberOfOverlaps { get; set; } = 1;

			[HelpText("Used only if custom normalization is enabled. Any number of overlaps that is equal to this or higher than this considered to be the hottest possible value.")]
			public uint MaximumNumberOfOverlaps { get; set; } = 100;
			public int NumberOfBatchesToProcess { get; set; } = int.MaxValue;

			public RenderingMode RenderingMode { get; set; } = RenderingMode.Masks;
			public string FabricServiceId { get; internal set; }
		}

		public void CancelGeneration()
		{
			_keepWorking = false;
			_ctSource.Cancel();
		}

		public async Task RunGenerationAsync(DateTime? from, DateTime? to)
		{
			_keepWorking = true;
			_ctSource = new CancellationTokenSource();

			var source = new TaskCompletionSource<bool>();
			_completionTask = source.Task;

			try
			{
				_frames = new ConcurrentFixedSizeQueue<List<HeatPoint>>(int.MaxValue);

				ProtectOriginalMetadataSet();

				if (from.HasValue)
				{
					_metadataSet.FromDate = from;
				}
				if (to.HasValue)
				{
					_metadataSet.ToDate = to;
				}

				await CountTagsTotalAsync();

				if (this.TotalCountProp.Value == 0)
				{
					_lastImage = await GetFirstImageForMetadatasetAssetAsync(from);
					ImageProp.Value = _lastImage.Data;
					PertcantageProcessedUpdated?.Invoke(100);
				}

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
					findArgs.DescriptorConditions.AddCondition(conditions.Result);

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

						await UpdateImageDataAsync(hyperId);

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

							var framePoints = new List<HeatPoint>();

							foreach (var tag in sliceTags)
							{
								// TODO: Reintegrate RealImage processing logic
								if (_settings.RenderingMode == RenderingMode.Masks || _settings.RenderingMode == RenderingMode.RealImage)
								{
									var tagExtraction = ProcessMaskedTag(tag);
									if (tagExtraction != null)
									{
										ProcessMaskOverlapsIntoMatrix(tagExtraction.Image, tagExtraction.Rect);
									}
								}
								else if (_settings.RenderingMode == RenderingMode.LowerPointOfGeometry)
								{
									ProcessGeometryTag(tag, framePoints);
								}
							}

							_frames.Enqueue(framePoints);
						}

						if (!_keepWorking || _ct.IsCancellationRequested)
							return;
					}

					_lastHyperId = grouppedBySlice.Last().Key;

					await RerenderAsync();

					var tagDocsCount = tagDocs.Count();
					ItemsProcessed.Value += tagDocsCount;
					PertcantageLabel.Value = ((double)100 * ItemsProcessed.Value / TotalCountProp.Value).ToString("0.00", System.Globalization.CultureInfo.InvariantCulture) + "%";
					PertcantageProcessedUpdated?.Invoke(100 * tagDocsCount / (double)TotalCountProp.Value);
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

		private async Task<UniImage> GetFirstImageForMetadatasetAssetAsync(DateTime? from)
		{
			var retrieveAssetArgs = new RetrieveAssetsArgs
			{
				AssetsIds = _metadataSet.AssetIds
			};

			var assets = (await _hyperStore.ExecuteAsyncThrows(retrieveAssetArgs));
			HyperAsset assetToGetSliceFrom = null;
			foreach (var asset in assets)
			{
				var retrieveTrackArgs = new RetrieveTrackArgs
				{
					AssetId = asset.Id,
					TrackId = asset.DefaultVideoTrackId.Value
				};

				asset.AddTrack(await _hyperStore.RetrieveTrackAsync(asset.Id, asset.DefaultVideoTrackId.Value, null, false));

				if (from >= asset.TimeStampUTC && from <= asset.TimeStampUTC + asset.GetDuration().Value)
				{
					assetToGetSliceFrom = asset;
				}
			}

			if (assetToGetSliceFrom == null)
			{
				assetToGetSliceFrom = assets[0];
				from = assetToGetSliceFrom.TimeStampUTC;
			}

			var assetStartTime = assetToGetSliceFrom.TimeStampUTC;
			from = from ?? assetStartTime;

			var startingPosition = from - assetStartTime;

			var beginningHyperId = await assetToGetSliceFrom.DefaultVideoTrack.FindAtAsync(_hyperStore, assetToGetSliceFrom.Id, (ulong)startingPosition.Value.TotalMilliseconds, HyperSeek.SeekModes.Estimated, false);

			var args = new RetrieveFragmentFramesArgs
			{
				AssetId = _metadataSet.AssetIds.First(),
				SliceIds = new HyperSliceId[] { beginningHyperId.Value.SliceId }
			};

			var res = await _hyperStore.ExecuteAsyncThrows(args);

			return res.First().Image;
		}

		/// <summary>
		/// Generates a heatmap based on subset of tags using Fixed Camera Preset with homography.
		/// </summary>
		/// <param name="tags">Subset of tags to use.</param>
		/// <param name="fixedCameraPresetId">Id of Fixed Camera Preset to take homography configuration from.</param>
		/// <param name="renderingMode">If enabled, heatmap masks will be rendered, otherwise real pixels from corresponding frames will be rendered in overlap.</param>
		/// <param name="cutOff">If we should cutoff the image by homography or just render a polygon line over.</param>
		public async Task<UniImage> GenerateFromTagsAsync(List<HyperTag> tags, HyperDocumentId fixedCameraPresetId, bool cutOff = false, int tagGroupsLimit = 30)
		{
			if (tags == null || tags.Count < 1)
				return null;

			var configuration = await RetrieveHyperDocumentArgs.RetrieveAsyncThrows<FixedCameraEnhancedData>(_hyperStore, fixedCameraPresetId);

			if (configuration == null)
				return null;

			var tagsWithHyperId = tags.Select(x => new
			{
				Tag = x,
				HyperId = x.GetElement<IHyperTagHyperIds>().HyperId
			}).ToArray();

			tagsWithHyperId = tagsWithHyperId.OrderBy(x => x.HyperId).ToArray();

			var hyperId = tagsWithHyperId.First().HyperId;

			if (!_keepWorking || _ct.IsCancellationRequested)
				return null;

			await UpdateImageDataAsync(hyperId);

			if (!_keepWorking || _ct.IsCancellationRequested)
				return null;

			SKImage maskedImage = null;

			if (_settings.RenderingMode == RenderingMode.Masks)
			{
				_globalMatrix = new uint[_height, _width];

				foreach (var tag in tags)
				{
					var tagExtraction = ProcessMaskedTag(tag);
					if (tagExtraction != null)
					{
						ProcessMaskOverlapsIntoMatrix(tagExtraction.Image, tagExtraction.Rect);
					}
				}

				maskedImage = RenderMask(_lastImage);
			}
			else if (_settings.RenderingMode == RenderingMode.LowerPointOfGeometry)
			{
				var framePoints = new List<HeatmapRenderHelper.HeatPoint>();

				foreach (var tag in tags)
				{
					ProcessGeometryTag(tag, framePoints);
				}

				var mask = HeatmapRenderHelper.RenderToSkImage(framePoints, _width, _height);
				maskedImage = BlendTwoImages(SKBitmap.Decode(_lastImage.Data), mask, SKBlendMode.Plus);
			}
			else if (_settings.RenderingMode == RenderingMode.RealImage)
			{
				var tagsBySlice = tagsWithHyperId.GroupBy(x => x.HyperId).OrderBy(x => x.Key).ToArray();

				var sourceBitmap = SKBitmap.Decode(_lastImage.Data);

				foreach (var tagsForSlice in tagsBySlice.Take(tagGroupsLimit))
				{
					if (!_keepWorking || _ct.IsCancellationRequested)
						return null;

					var args = new RetrieveFragmentFramesArgs
					{
						AssetId = tagsForSlice.Key.AssetId.Value,
						TrackId = tagsForSlice.Key.TrackId.Value,
						FragmentId = tagsForSlice.Key.FragmentId.Value,
						SliceIds = new HyperSliceId[] { tagsForSlice.Key.SliceId.Value },
						FabricServiceId = this._settings.FabricServiceId
					};

					var result = await _hyperStore.ExecuteAsync(args) ?? new RetrieveFragmentFramesArgs.SliceResult[0];

					if (result.Length == 0)
						continue;

					var frame = result[0].Image;
					var frameSkBitmap = SKBitmap.Decode(frame.Data);

					_globalMatrix = new uint[_height, _width];

					foreach (var tag in tagsForSlice.Select(x => x.Tag))
					{
						if (!_keepWorking || _ct.IsCancellationRequested)
							return null;

						var tagExtraction = ProcessMaskedTag(tag);
						if (tagExtraction != null)
						{
							ProcessMaskTransparencyIntoMatrix(tagExtraction.Image, tagExtraction.Rect);
						}
					}

					var sourcePixelsAddr = sourceBitmap.GetPixels();
					var destPixelsAddr = frameSkBitmap.GetPixels();

					byte MixColors(byte color1, byte color2, byte transparencyValue)
					{
						var transparency2 = (double)1 / 255 * transparencyValue;
						var transparency1 = 1 - transparency2;

						var result = (byte)((color1 * transparency1) + (color2 * transparency2));
						return result;
					}

					unsafe
					{
						byte* sourcePtr = (byte*)sourcePixelsAddr.ToPointer();
						byte* destPtr = (byte*)destPixelsAddr.ToPointer();

						for (int row = 0; row < sourceBitmap.Height; row++)
							for (int col = 0; col < sourceBitmap.Width; col++)
							{
								var transpValue = _globalMatrix[row, col];

								if (transpValue == 0) // Do not modify anything
								{
									sourcePtr += 4;
									destPtr += 4;
								}
								else
								{
									var destBlue = *destPtr++; // blue
									var destGreen = *destPtr++; // green
									var destRed = *destPtr++; // red
									destPtr++; // skip alpha

									var sourceBlue = *sourcePtr;
									var sourceGreen = *(sourcePtr + 1);
									var sourceRed = *(sourcePtr + 2);

									var mixedBlue = MixColors(sourceBlue, destBlue, (byte)transpValue);
									var mixedGreen = MixColors(sourceGreen, destGreen, (byte)transpValue);
									var mixedRed = MixColors(sourceRed, destRed, (byte)transpValue);

									*sourcePtr++ = mixedBlue; // blue
									*sourcePtr++ = mixedGreen; // green
									*sourcePtr++ = mixedRed; // red
									sourcePtr++; // skip alpha
								}
							}
					}
				}

				maskedImage = SKImage.FromBitmap(sourceBitmap);
			}

			if (maskedImage == null)
				return null;

			if (cutOff)
			{
				return RenderWithCrop(maskedImage, configuration);
			}
			else
			{
				return RenderWithNoCutoff(maskedImage, configuration);
			}
		}

		private SKImage BlendTwoImages(SKBitmap image1, SKImage image2, SKBlendMode blendMode)
		{
			using (var tempSurface = SKSurface.Create(new SKImageInfo(_width, _height)))
			{
				//get the drawing canvas of the surface
				var canvas = tempSurface.Canvas;

				//set background color
				canvas.Clear(SKColors.Transparent);

				var skPaint = new SKPaint
				{
					BlendMode = blendMode
				};

				canvas.DrawBitmap(image1, 0, 0);

				canvas.DrawImage(image2, 0, 0, skPaint);

				// return the surface as a manageable image
				var res = tempSurface.Snapshot();

				//using (var data = res.Encode(SKEncodedImageFormat.Png, 80))
				//{
				//	// save the data to a stream
				//	using (var stream = System.IO.File.OpenWrite(@$"C:/temp/__{(blendMode.ToString())}.png"))
				//	{
				//		data.SaveTo(stream);
				//	}
				//}

				return res;
			}
		}

		private UniImage RenderWithCrop(SKImage maskedImage, FixedCameraEnhancedData configuration)
		{
			var mask = GenerateHomographyMask(configuration, _width, _height);

			SKImage maskedCroppedImage;

			using (var tempSurface = SKSurface.Create(new SKImageInfo(_croppedSize.Width, _croppedSize.Height)))
			{
				//get the drawing canvas of the surface
				var canvas = tempSurface.Canvas;

				//set background color
				canvas.Clear(SKColors.Transparent);

				canvas.DrawImage(maskedImage, SKRect.Create(_minX.Value, _minY.Value, _croppedSize.Width, _croppedSize.Height), SKRect.Create(0, 0, _croppedSize.Width, _croppedSize.Height));

				canvas.DrawImage(mask, 0, 0);

				// return the surface as a manageable image
				maskedCroppedImage = tempSurface.Snapshot();
			}

			//

			var image = SKImageIntoUniImage(maskedCroppedImage);

			maskedCroppedImage.Dispose();

			return image;
		}

		private UniImage RenderWithNoCutoff(SKImage maskedImage, FixedCameraEnhancedData configuration)
		{
			var polygons = GetPolygons(configuration, _width, _height);

			SKImage maskedCroppedImage;

			using (var tempSurface = SKSurface.Create(new SKImageInfo(maskedImage.Width, maskedImage.Height)))
			{
				//get the drawing canvas of the surface
				var canvas = tempSurface.Canvas;

				//set background color
				canvas.Clear(SKColors.Transparent);

				canvas.DrawImage(maskedImage, 0, 0);

				var fillPaint = new SKPaint
				{
					Style = SKPaintStyle.Stroke,
					Color = SKColors.Red,
					BlendMode = SKBlendMode.Src,
					StrokeWidth = 3
				};

				using (fillPaint)
				{
					var path = new SKPath();
					foreach (var points in polygons)
					{
						path.MoveTo(points.First());
						for (var i = 1; i < points.Length; i++)
						{
							path.LineTo(points[i]);
						}
						path.LineTo(points[0]);
					}

					canvas.DrawPath(path, fillPaint);
				}

				// return the surface as a manageable image
				maskedCroppedImage = tempSurface.Snapshot();
			}

			//

			var image = SKImageIntoUniImage(maskedCroppedImage);

			maskedCroppedImage.Dispose();

			return image;
		}

		private async Task UpdateImageDataAsync(HyperId hyperId)
		{
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
		}

		private async Task<UniImage> GetImageAsync(HyperId hyperId)
		{
			var args = new RetrieveFragmentFramesArgs
			{
				AssetId = hyperId.AssetId.Value,
				FragmentId = hyperId.FragmentId.Value,
				SliceIds = new HyperSliceId[] { hyperId.SliceId.Value },
				FabricServiceId = this._settings.FabricServiceId
			};

			var res = await _hyperStore.ExecuteAsyncThrows(args);

			return res.First().Image;
		}

		private UniImage SKImageIntoUniImage(SKImage skImage)
		{
			UniImage image;

			using (skImage)
			{
				using (var skData = skImage.Encode(SKEncodedImageFormat.Jpeg, 70))
				{
					var bytes = skData.ToArray();
					image = new UniImage(ImageFormats.Jpeg, bytes, skImage.Width, skImage.Height);
				}
			}

			return image;
		}

		private async Task RerenderAsync()
		{
			StatusProp.Value = "Rendering heatmap...";

			_lastImage = await GetImageAsync(_lastHyperId);

			var skImage = RenderMask(_lastImage);

			var image = SKImageIntoUniImage(skImage);

			//SkImageProp.Value = result;
			ImageProp.Value = image.Data;
			StatusProp.Value = "Finished updating heatmap.";
		}

		private SKImage RenderMask(UniImage sourceImage)
		{
			using (var skBitmap = SKBitmap.Decode(sourceImage.Data))
			{
				SKImage result;

				SKImage heatmap = null;

				if (_settings.RenderingMode == RenderingMode.Masks)
				{
					heatmap = GenerateMaskFromValuesMatrix(_globalMatrix, _width, _height,
						_settings.UseCustomNormalizationSettings ? _settings.MinimumNumberOfOverlaps : (uint?)null,
						_settings.UseCustomNormalizationSettings ? _settings.MaximumNumberOfOverlaps : (uint?)null);
				}
				else if (_settings.RenderingMode == RenderingMode.LowerPointOfGeometry)
				{
					var points = _frames.SelectMany(it => it).ToArray();
					heatmap = Render(points, _width, _height).ToSKImage();
				}

				if (heatmap == null)
				{
					return SKImage.FromBitmap(skBitmap);
				}

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

				return result;
			}
		}

		private class TagMaskExtraction
		{
			public SKBitmap Image { get; set; }
			public UniRectangle2f Rect { get; set; }
		}

		private void ProcessGeometryTag(HyperTag tag, List<HeatmapRenderHelper.HeatPoint> heatpoints)
		{
			var geometryItems = tag.GetElements<HyperTagGeometry>().Select(x => x.GeometryItem).ToList();

			geometryItems.AddRange(tag.GetElements<HyperTagTrackingData>().SelectMany(x => x.TrackingSequence.Select(ts => ts.GeometryItem)));

			var classification = tag.GetElements<HyperTagLabel>().Where(x => x.Type == HyperTagLabel.Types.Classification).Select(x => x.Label).FirstOrDefault();

			if (classification == null)
			{
				classification = tag.GetElement<TagonomyExecutionResultHyperTagElement>()?.GetCombinedLabel();
			}

			//var setting = Settings.Classifications.FirstOrDefault(x => x.Classification == classification);

			//if (setting == null)
			//{
			//	setting = new ClassificationSettings
			//	{
			//		Classification = classification,
			//		IsEnabled = true,
			//		Color = Settings.Classifications.First().Color
			//	};
			//	Settings.Classifications.Add(setting);
			//}

			//setting.DebugCount++;

			foreach (var geo in geometryItems)
			{
				var rect = geo.BoundingBox;
				if (geo.SpaceMode != GeometryItem.SpaceModes.Normal)
				{
					rect = geo.ConvertFromXSpace(rect);
				}

				heatpoints.Add(new HeatmapRenderHelper.HeatPoint((int)(rect.Center.X * _width), (int)(rect.Bottom * _height), 255, classification));
			}
		}

		private TagMaskExtraction ProcessMaskedTag(HyperTag tag)
		{
			var geometry = tag.GetElement<HyperTagGeometry>();
			var geometryMask = tag.GetElement<HyperTagGeometryMask>();

			if (geometryMask == null || geometry == null)
				return null;

			//var classification = tag.GetElements<HyperTagLabel>().Where(x => x.Type == HyperTagLabel.Types.Classification).Select(x => x.Label).FirstOrDefault();

			//if (classification == null)
			//{
			//	classification = tag.GetElement<TagonomyExecutionResultHyperTagElement>()?.GetCombinedLabel();
			//}

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

			return new TagMaskExtraction { Image = skBitmap, Rect = rect };
		}

		private void ProcessMaskOverlapsIntoMatrix(SKBitmap skBitmap, UniRectangle2f rect)
		{
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

		private void ProcessMaskTransparencyIntoMatrix(SKBitmap skBitmap, UniRectangle2f rect)
		{
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

							if (alpha != 0)
							{
								alpha = blue;
							}

							_globalMatrix[realY, realX] = alpha;
						}
					}
			}
		}

		private class ExtractedTagImage
		{
			public SKImage Image { get; set; }
			public Point TopLeftPoint { get; set; }
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

		private DateTime[] _timeRange;
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
			mainArgs.DescriptorConditions.AddCondition(conditions.Result);

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
			//if (tagFilterConditions != null)
			//{
			//	mainArgs.DocumentConditions.AddCondition(tagFilterConditions);
			//}
		}

		public static SKImage GenerateMaskFromValuesMatrix(uint[,] matrix, int width, int height, uint? overrideNormalizationMin, uint? overrideNormalizationMax)
		{
			var maxValue = overrideNormalizationMax ?? matrix.Cast<uint>().Max();

			if (maxValue == 0)
				return null;

			var minValue = overrideNormalizationMin ?? matrix.Cast<uint>().Where(x => x > 0).Min();

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

			return image;
		}

		public static void GenerateMaskFromTransparencyMatrix(SKBitmap sourceImage, uint[,] matrix)
		{
			var pixelsAddr = sourceImage.GetPixels();

			unsafe
			{
				byte* ptr = (byte*)pixelsAddr.ToPointer();

				for (int row = 0; row < sourceImage.Height; row++)
					for (int col = 0; col < sourceImage.Width; col++)
					{
						var originalValue = matrix[row, col];

						ptr += 3;

						*ptr++ = (byte)originalValue;
					}
			}
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


		////
		// TODO: Merge this functionality with AOI component, extract it as a helper
		////

		int? _minX = null;
		int? _minY = null;
		int? _maxX = null;
		int? _maxY = null;
		Size _croppedSize;

		private SKImage GenerateHomographyMask(FixedCameraEnhancedData configuration, int sourceWidth, int sourceHeight)
		{
			var polygons = GetPolygons(configuration, sourceWidth, sourceHeight);

			using (var tempSurface = SKSurface.Create(new SKImageInfo(sourceWidth, sourceHeight)))
			{
				//get the drawing canvas of the surface
				var canvas = tempSurface.Canvas;

				canvas.Clear(SKColors.Black);

				var fillPaint = new SKPaint
				{
					Style = SKPaintStyle.Fill,
					Color = SKColors.Transparent,
					BlendMode = SKBlendMode.Clear
				};

				using (fillPaint)
				{
					var path = new SKPath();
					foreach (var points in polygons)
					{
						path.MoveTo(points.First());
						for (var i = 1; i < points.Length; i++)
						{
							path.LineTo(points[i]);
						}
					}

					canvas.DrawPath(path, fillPaint);
				}

				using (var image = tempSurface.Snapshot())
				{
					_croppedSize = new Size(_maxX.Value - _minX.Value, _maxY.Value - _minY.Value);

					var croppingRectI = SKRectI.Create(_minX.Value, _minY.Value, _croppedSize.Width, _croppedSize.Height);

					var mask = image.Subset(croppingRectI);

					return mask;
				}
			}
		}

		private SKPoint[][] GetPolygons(FixedCameraEnhancedData configuration, int sourceWidth, int sourceHeight)
		{
			var polygons = configuration.Layers.OfType<HyperTagFixedCameraEnhancedDataLayer>()
					.SelectMany(x => x.Tags ?? new HyperTag[0])
					.Select(x => ProcessHyperTag(x, sourceWidth, sourceHeight))
					.Where(x => x != null)
					.ToArray();
			return polygons;
		}

		public SKPoint[] ProcessHyperTag(HyperTag tag, int width, int height)
		{
			var geometry = tag.GetElement<HyperTagGeometry>();
			var homographyEl = tag.GetElement<HomographyTagElement>(); // Ensure this is an AOI, not something else, like measuremenet etc.

			if (geometry == null || homographyEl == null)
				return null;

			var polygon = (UniPolygon2f)geometry.GeometryItem.Shape;

			var convertedPoints = polygon.Points.Select(it => geometry.GeometryItem.ConvertFromXSpace(it)).Select(it => new Point((int)(it.X * width), (int)(it.Y * height))).ToArray();

			// Calcuate the min and max positions of the extracted regions globally for all polygons (if many).
			_minX = convertedPoints.Min(x => x.X);
			_maxX = convertedPoints.Max(x => x.X);
			_minY = convertedPoints.Min(x => x.Y);
			_maxY = convertedPoints.Max(x => x.Y);

			if (_minX.HasValue && _minX.Value % 2 == 1)
				_minX = _minX.Value + 1;

			if (_maxX.HasValue && _maxX.Value % 2 == 1)
				_maxX = _maxX.Value + 1;

			if (_minY.HasValue && _minY.Value % 2 == 1)
				_minY = _minY.Value + 1;

			if (_maxY.HasValue && _maxY.Value % 2 == 1)
				_maxY = _maxY.Value + 1;

			return convertedPoints.Select(x => new SKPoint(x.X, x.Y)).ToArray();
		}


		////


		public void Dispose()
		{
			CancelGeneration();
		}
	}
}
