using Orions.Common;
using Orions.Infrastructure.HyperMedia;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;

namespace Orions.Systems.CrossModules.Components.Helpers
{
	public static class TagRenderHelper
	{
        private static object g_penSyncRoot = new object();
        private static Pen g_pen = new Pen(Brushes.White, 2);

        public static byte[] RenderTag(HyperTag tag, byte[] imageData)
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
    }
}
