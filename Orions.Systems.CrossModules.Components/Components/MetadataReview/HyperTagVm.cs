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

namespace Orions.Systems.CrossModules.Components
{
    public class HyperTagVm : BlazorVm
    {
        private IHyperTagHyperIds HyperTagId;
        public ViewModelProperty<byte[]> Image { get; set; } = new ViewModelProperty<byte[]>();
        private NetStore _store;

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

        public ViewModelProperty<string> Id { get; set; } = new ViewModelProperty<string>();

        public ViewModelProperty<string> HyperTagLabel { get; set; } = new ViewModelProperty<string>();
        public ViewModelProperty<string> HyperTagFragmentSliceLabel { get; set; } = new ViewModelProperty<string>();

        //protected 

        public async Task Initialize(HyperTag tag, NetStore store)
        {
            _store = store;

            this.HyperTagLabel.Value = tag.GetElement<HyperTagLabel>()?.Label ?? "";

            var ids = tag.GetElements<IHyperTagHyperIds>().FirstOrDefault(e => e.HyperId.TrackId.Value.Type == HyperTrackTypes.Video);
            this.HyperTagId = ids;
            this.HyperTagFragmentSliceLabel = $"@ {ids.HyperId.FragmentId}:{ids.HyperId.SliceId}";

            Image = await LoadImage(tag);
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
                GeometryItem = geometry?.GeometryItem
            };

            var sliceResult = await _store.ExecuteAsync<RetrieveFragmentFramesArgs.SliceResult[]>(args2);

            return sliceResult[0].Image.Data;
        }
    }
}
