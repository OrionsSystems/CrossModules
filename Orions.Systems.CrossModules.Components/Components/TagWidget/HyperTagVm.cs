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

        //protected 

        public async Task Initialize(HyperTag tag, NetStore store)
        {
            _store = store;

            this.HyperTagLabel.Value = tag.GetElement<HyperTagLabel>()?.Label ?? "";
            this.HyperTagId = tag.GetElements<IHyperTagHyperIds>()?.FirstOrDefault(e => e.HyperId.TrackId.Value.Type == HyperTrackTypes.Video);

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


            #region not implemented yet
            //Bitmap defaultBitmap = null;
            //Bitmap maskedBitmap = null;

            //Func<UniImage, Bitmap> getBitmap = delegate (UniImage image)
            //{
            //    var data = image.Data;
            //    if (data == null)
            //        return null;

            //    if (image.Format != ImageFormats.RGB)
            //    {
            //        var b = new Bitmap(new MemoryStream(data));
            //        return b;
            //    }
            //    else
            //    {
            //        throw new Exception("RGB data is super raw, so we need to create it in a special way.");
            //    }
            //};

            //defaultBitmap = getBitmap(sliceResult[0].Image);

            //using (var g = Graphics.FromImage(defaultBitmap))
            //{
            //    var textBackgroundBrush = new SolidBrush(System.Drawing.Color.FromArgb(140, 255, 255, 255));
            //    var font = new Font("Tahoma", 12);
            //    System.Drawing.Pen g_pen = new System.Drawing.Pen(System.Drawing.Brushes.Red, 2);

            //    var geometryMask = tag.GetElement<HyperTagGeometryMask>();
            //    if (geometry != null && geometryMask != null)
            //    {// Mask render.
            //        //throw new NotSupportedException("See HyperTagReviewControlVm.FramesCallback");
            //    }
            //    else if (geometry != null)
            //    {// Geometry render.
            //        //if (_tagDisplayMode == TagDisplayMode.Extract)
            //        //{
            //        //    //bitmap = RasterHelper.Instance.ExtractHyperTagToBitmap(value[0].Image, this.HyperTag);
            //        //    //defaultBitmap = value[0].Image.AsBitmap();
            //        //}
            //        //else
            //        //{
            //            foreach (var component in tag.GetElements<HyperTagGeometry>())
            //            {
            //                var rect = component.GeometryItem.BoundingBox;
            //                if (component.GeometryItem.SpaceMode == GeometryItem.SpaceModes.XSpace)
            //                    rect = component.GeometryItem.ConvertFromAbsoluteXSpaceToRealWorldSpace(rect, defaultBitmap.Width, defaultBitmap.Height);

            //                if (string.IsNullOrEmpty(tag.GetHumandReadableLabel()) == false)
            //                {
            //                    var x = rect.X + 5;
            //                    var y = rect.Y + 5;

            //                    var measured = g.MeasureString(tag.GetHumandReadableLabel(), font);
            //                    g.FillRectangle(textBackgroundBrush, rect.X + 3, rect.Y + 3, measured.Width + 6, measured.Height + 6);

            //                    g.DrawString(tag.GetHumandReadableLabel(), font, System.Drawing.Brushes.Red, new PointF(x, y));

            //                }

            //                g.DrawRectangle(g_pen, new Rectangle((int)rect.X, (int)rect.Y, (int)rect.Width, (int)rect.Height));
            //            }
            //        //}
            //    }
            //    else
            //    {
            //        //defaultBitmap = value[0].Image.AsBitmap();
            //    }

            //    textBackgroundBrush.Dispose();
            //    font.Dispose();
            //}

            //byte[] imageBytes;
            //using (var stream = new MemoryStream())
            //{
            //    defaultBitmap.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
            //    imageBytes = stream.ToArray();
            //}
            #endregion

            return sliceResult[0].Image.Data;
        }
    }
}
