using Orions.Common;
using Orions.Infrastructure.HyperMedia;
using Orions.Node.Common;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Orions.Infrastructure.HyperSemantic;
using System.Drawing.Imaging;

namespace Orions.Systems.CrossModules.Components
{
   public class HyperTagVm : BlazorVm
   {
      static object g_penSyncRoot = new object();
      static Pen g_pen = new Pen(Brushes.White, 2);

      private IHyperArgsSink _store;
      private HyperTag _hyperTag;
      private int _dashApiPort;
      private IHyperTagHyperIds HyperTagId;

      // Data props
      public ViewModelProperty<byte[]> Image { get; set; } = new ViewModelProperty<byte[]>();

      public ViewModelProperty<string> HyperTagLabel { get; set; } = new ViewModelProperty<string>();
      public ViewModelProperty<string> HyperTagFragmentSliceLabel { get; set; } = new ViewModelProperty<string>();
      public ViewModelProperty<List<string>> TagonomyLabels { get; private set; }

      // Control state
      public bool IsPlaying { get; private set; } = false;
      public bool IsExpanded { get; set; }

      // Computed props
      public string ImageBase64Url
      {
         get
         {
            if (Image.Value != null)
            {
               return $"data:image/jpg;base64, {Convert.ToBase64String(Image.Value)}";
            }

            return null;
         }
      }
      public string PlayerUri
      {
         get
         {
            var dnsSafehost = (_store as NetStore).CurrentConnection.Uri.DnsSafeHost;
            var assetId = HyperTagId.HyperId.AssetId.Value.Guid.ToString();
            return $"http://{dnsSafehost}:{_dashApiPort}/dash/{assetId}/asset.mpd";
         }
      }
      public string PlayerId
      {
         get
         {
            return $"Orions.JwPlayer-{HyperTagId.HyperId.AssetId.Value.Guid}";
         }
      }

      public int StartAt { get; set; }

      // Event callbacks
      public EventCallback<string> OnPlayButtonClicked { get; set; }

      public bool ShowFragmentAndSlice { get; set; }

      public bool ExtractMode { get; set; }

      public string FabricServiceId { get; set; }

      public HyperTagVm()
      {
      }

      public async Task Initialize(HyperTag tag, IHyperArgsSink store, int dashApiPort)
      {
         _store = store;

         this._hyperTag = tag;
         this._dashApiPort = dashApiPort;

         this.HyperTagLabel.Value = tag.GetElement<HyperTagLabel>()?.Label ?? "";

         var ids = tag.GetElements<IHyperTagHyperIds>().FirstOrDefault(e => e.HyperId.TrackId.Value.Type == HyperTrackTypes.Video);
         this.HyperTagId = ids;

         if (this.ShowFragmentAndSlice)
            this.HyperTagFragmentSliceLabel = $"@ {ids.HyperId.FragmentId}:{ids.HyperId.SliceId}";

         var startAtElement = tag.GetElements<HyperTagTime>().FirstOrDefault(t => t.TimeType == HyperTagTime.TimeTypes.StreamTime);
         this.StartAt = (int)(startAtElement.StreamTime_TimeSpan?.TotalSeconds ?? 0);

         var execution = tag.GetElement<TagonomyExecutionResultHyperTagElement>();
         if (execution != null && execution.Result != null)
         {
            var steps = execution.Result.FinishedPaths?.SelectMany(p => p.Steps).ToArray();
            if (steps != null && steps.Any())
            {
               var tagonomyLabels = new List<string>();

               foreach (TagonomyExecutionStep step in steps)
               {
                  // Do not print the initial Tagonomy start step, as this is simply the name of the Tagonomy, which is not informative here.
                  if (string.IsNullOrEmpty(step.ParentPathNodeElementId) == false)
                     tagonomyLabels.Add(step.OptionalTargetNodeName);

                  var checkLeafs = step.Actions.Where(it => it.NodeElementTypeName == typeof(CheckStateLeafNodeElement).FullName
                                                  && ((CheckStateLeafNodeElement.ElementUniformResult)it.Result).IsChecked);

                  foreach (var item in checkLeafs)
                  {
                     tagonomyLabels.Add(item.NodeName);
                  }

               }

               TagonomyLabels = tagonomyLabels;
            }
         }

         Image = await LoadImage(tag);
      }

      public async Task OnPlayButtonClickedHandler()
      {
         this.IsPlaying = true;
         await OnPlayButtonClicked.InvokeAsync(this.HyperTagId.HyperId.AssetId.Value.Value);
      }

      public void OnClosePlayer()
      {
         IsPlaying = false;
      }

      public async Task OnExpandButtonClickedHandler()
      {
         this.IsExpanded = true;
         await OnPlayButtonClicked.InvokeAsync(this.HyperTagId.HyperId.AssetId.Value.Value);
      }

      public void OnCloseExpanded()
      {
         IsExpanded = false;
      }

      protected byte[] RenderTag(HyperTag tag, byte[] imageData)
      {
         if (imageData == null || imageData.Length == 0)
            return imageData;

         using (var bitmapStream = new MemoryStream(imageData))
         {
            using (Bitmap bitmap = new Bitmap(bitmapStream))
            {
               using (var g = Graphics.FromImage(bitmap))
               {
                  foreach (var component in tag.GetElements<HyperTagGeometry>())
                  {
                     var rect = component.GeometryItem.BoundingBox;
                     if (component.GeometryItem.SpaceMode == GeometryItem.SpaceModes.XSpace)
                        rect = component.GeometryItem.ConvertFromAbsoluteXSpaceToRealWorldSpace(rect, bitmap.Width, bitmap.Height);

                     //DrawLabel(g, tag, rect);

                     lock (g_penSyncRoot) // Pen is not thread safe
                     {
                        var geometry = tag.GetElement<HyperTagGeometry>();
                        if (geometry.GeometryItem.Shape is UniPolygon2f polygonX)
                        {
                           UniPolygon2f polygon = polygonX;
                           if (geometry.GeometryItem.SpaceMode == GeometryItem.SpaceModes.XSpace)
                           {
                              polygon = geometry.GeometryItem.ConvertFromAbsoluteXSpaceToRealWorldSpace(polygonX, bitmap.Width, bitmap.Height);
                              g.DrawPolygon(g_pen, polygon.Points.Select(it => new PointF(it.X, it.Y))?.ToArray());

                           }

                           g.DrawPolygon(g_pen, polygon.Points.Select(it => new PointF(it.X, it.Y))?.ToArray());
                        }
                        else
                        {
                           g.DrawRectangle(g_pen, new Rectangle((int)rect.X, (int)rect.Y, (int)rect.Width, (int)rect.Height));
                        }
                     }
                  }
               }

               using (var writeStream = new MemoryStream())
               {
                  bitmap.Save(writeStream, ImageFormat.Jpeg);
                  return writeStream.ToArray();
               }
            }
         }
      }

      private async Task<byte[]> LoadImage(HyperTag tag)
      {
         var ids = this.HyperTagId;

         var geometry = tag.GetElement<HyperTagGeometry>();
         var args2 = new RetrieveFragmentFramesArgs()
         {
            AssetId = ids.HyperId.AssetId.Value,
            FragmentId = ids.HyperId.HasFullFragmentData ? ids.HyperId.FragmentId.Value : new HyperFragmentId(0),
            SliceIds = new HyperSliceId[] { ids.HyperId.HasFullSliceData ? ids.HyperId.SliceId.Value : new HyperSliceId(0) },
            GeometryItem = ExtractMode ? geometry?.GeometryItem : null,
            FabricServiceId = this.FabricServiceId,
         };

         var sliceResult = await _store.ExecuteAsync(args2);

         if (sliceResult == null || sliceResult.Length == 0 || sliceResult[0].Image?.Data == null)
            return null;

         byte[] imageData = sliceResult[0].Image.Data;
         if (ExtractMode == false)
         {
            imageData = RenderTag(tag, imageData);
         }


         return imageData;
      }
   }
}
