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
using Orions.Systems.CrossModules.Components.Helpers;

namespace Orions.Systems.CrossModules.Components
{
   public class HyperTagVm : BlazorVm
   {
      private HyperTag _hyperTag;
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
            var dnsSafehost = (Context?.HyperStore as NetStore).CurrentConnection.Uri.DnsSafeHost;

            var assetId = HyperTagId.HyperId.AssetId.Value.Guid.ToString();
            return $"https://{dnsSafehost}:{Context.DashApiPort}/dash/{assetId}/asset.mpd";
         }
      }

      public string PlayerId
      {
         get
         {
            return $"Orions.ShakaPlayer-{HyperTagId.HyperId.AssetId.Value.Guid}";
         }
      }

      public int StartAt { get; set; }

      // Event callbacks
      public EventCallback<string> OnPlayButtonClicked { get; set; }

      public ITagReviewContext Context { get; set; }

      public HyperTagVm()
      {
      }

      public async Task Initialize(HyperTag tag)
      {
         this._hyperTag = tag;

         this.HyperTagLabel.Value = tag.GetElement<HyperTagLabel>()?.Label ?? "";

         var ids = tag.GetElements<IHyperTagHyperIds>().FirstOrDefault(e => e.HyperId.TrackId.Value.Type == HyperTrackTypes.Video);
         this.HyperTagId = ids;

         if (this.Context?.ShowFragmentAndSlice == true)
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

      private async Task<byte[]> LoadImage(HyperTag tag)
      {
         if (Context?.HyperStore == null)
            return null;

         var ids = this.HyperTagId;

         var geometry = tag.GetElement<HyperTagGeometry>();
         var args2 = new RetrieveFragmentFramesArgs()
         {
            AssetId = ids.HyperId.AssetId.Value,
            FragmentId = ids.HyperId.HasFullFragmentData ? ids.HyperId.FragmentId.Value : new HyperFragmentId(0),
            SliceIds = new HyperSliceId[] { ids.HyperId.HasFullSliceData ? ids.HyperId.SliceId.Value : new HyperSliceId(0) },
            GeometryItem = this.Context?.ExtractMode == true ? geometry?.GeometryItem : null,
            FabricServiceId = this.Context?.FabricServiceId,
         };

         var sliceResult = await Context.HyperStore.ExecuteAsync(args2);

         if (sliceResult == null || sliceResult.Length == 0 || sliceResult[0].Image?.Data == null)
            return null;

         byte[] imageData = sliceResult[0].Image.Data;
         if (this.Context?.ExtractMode != true)
         {
            imageData = TagRenderHelper.RenderTag(tag, imageData);
         }


         return imageData;
      }
   }
}
