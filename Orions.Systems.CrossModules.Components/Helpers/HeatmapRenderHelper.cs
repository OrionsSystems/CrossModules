using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

using Orions.Common;

using SkiaSharp;

namespace ReactNOW.ML
{
	public static class HeatmapRenderHelper
	{
		public class HeatPoint
		{
			public int X { get; set; }
			public int Y { get; set; }

			public string PointClassification { get; set; }

			public byte Intensity { get; set; }

			public HeatPoint(int iX, int iY, byte bIntensity, string classification = null)
			{
				X = iX;
				Y = iY;
				Intensity = bIntensity;
				PointClassification = classification;
			}
		}

		public class ClassificationSettings : IUniInfo
		{
			public string Classification { get; set; }
			public bool IsEnabled { get; set; } = true;
			public UniColor Color { get; set; }

			/// <summary>
			/// Debugging only, used to help us see how many items in each category when setting up (if available)
			/// </summary>
			public int DebugCount { get; set; }

			public string Info => $"Class {Classification}, Count {DebugCount}, {(IsEnabled ? "Enabled" : "Disabled")}, R: {Color.R}, G: {Color.G}, B: {Color.B}";
		}

		private static readonly ColorMap[] Colors = CreatePaletteIndex();

		public static Bitmap Render(IEnumerable<HeatPoint> points, int width, int height)
		{
			// Create new memory bitmap the same size as the picture box
			var map = new Bitmap(width, height);

			// Call CreateIntensityMask, give it the memory bitmap, and store the result back in the memory bitmap
			map = CreateIntensityMask(map, points);

			return Colorize(map, 255, Colors);
		}

		public static SKImage RenderToSkImage(IEnumerable<HeatPoint> points, int width, int height)
		{
			var bitmap = Render(points, width, height);
			return bitmap.ToSKImage();
		}

		public static SKImage ToSKImage(this Bitmap bitmap)
		{
			SKImage sKImage = SKImage.Create(new SKImageInfo(bitmap.Width, bitmap.Height));
			using (SKPixmap pixmap = sKImage.PeekPixels())
			{
				bitmap.ToSKPixmap(pixmap);
				return sKImage;
			}
		}

		public static void ToSKPixmap(this Bitmap bitmap, SKPixmap pixmap)
		{
			if (pixmap.ColorType == SKImageInfo.PlatformColorType)
			{
				SKImageInfo info = pixmap.Info;
				using (Bitmap image = new Bitmap(info.Width, info.Height, info.RowBytes, PixelFormat.Format32bppPArgb, pixmap.GetPixels()))
				{
					using (Graphics graphics = Graphics.FromImage(image))
					{
						graphics.Clear(Color.Transparent);
						graphics.DrawImageUnscaled(bitmap, 0, 0);
					}
				}
			}
			else
			{
				using (SKImage sKImage = bitmap.ToSKImage())
				{
					sKImage.ReadPixels(pixmap, 0, 0);
				}
			}
		}

		private static Bitmap CreateIntensityMask(Bitmap bSurface, IEnumerable<HeatPoint> heatPoints)
		{
			// Create new graphics surface from memory bitmap
			using (Graphics drawSurface = Graphics.FromImage(bSurface))
			{
				//drawSurface.Clear(Color.FromArgb(50, 0, 0, 0));
				drawSurface.Clear(Color.Black);

				// Traverse heat point data and draw masks for each heat point
				foreach (HeatPoint dataPoint in heatPoints)
				{
					// Render current heat point on draw surface
					DrawHeatPoint(drawSurface, dataPoint, 15);
				}

				return bSurface;
			}
		}

		private static void DrawHeatPoint(Graphics canvas, HeatPoint heatPoint, int radius)
		{
			// Create points generic list of points to hold circumference points
			var circumferencePointsList = new List<Point>();
			// Create an empty point to predefine the point struct used in the circumference loop
			Point circumferencePoint;
			// Create an empty array that will be populated with points from the generic list
			Point[] circumferencePointsArray;

			// Calculate ratio to scale byte intensity range from 0-255 to 0-1
			float fRatio = 1F / Byte.MaxValue;
			// Precalulate half of byte max value
			byte bHalf = Byte.MaxValue / 2;
			// Flip intensity on it's center value from low-high to high-low
			int iIntensity = (byte)(heatPoint.Intensity - ((heatPoint.Intensity - bHalf) * 2));
			// Store scaled and flipped intensity value for use with gradient center location
			float fIntensity = iIntensity * fRatio;

			// Loop through all angles of a circle
			// Define loop variable as a double to prevent casting in each iteration
			// Iterate through loop on 10 degree deltas, this can change to improve performance
			for (double i = 0; i <= 360; i += 10)
			{
				// Replace last iteration point with new empty point struct
				circumferencePoint = new Point();
				// Plot new point on the circumference of a circle of the defined radius
				// Using the point coordinates, radius, and angle
				// Calculate the position of this iterations point on the circle
				circumferencePoint.X = Convert.ToInt32(heatPoint.X + radius * Math.Cos(ConvertDegreesToRadians(i)));
				circumferencePoint.Y = Convert.ToInt32(heatPoint.Y + radius * Math.Sin(ConvertDegreesToRadians(i)));
				// Add newly plotted circumference point to generic point list
				circumferencePointsList.Add(circumferencePoint);
			}

			// Populate empty points system array from generic points array list
			// Do this to satisfy the datatype of the PathGradientBrush and FillPolygon methods
			circumferencePointsArray = circumferencePointsList.ToArray();

			// Create new PathGradientBrush to create a radial gradient using the circumference points
			var gradientShaper = new PathGradientBrush(circumferencePointsArray);
			// Create new color blend to tell the PathGradientBrush what colors to use and where to put them
			var gradientSpecifications = new ColorBlend(3);
			// Define positions of gradient colors, use intesity to adjust the middle color to
			// show more mask or less mask
			gradientSpecifications.Positions = new float[3] { 0, fIntensity, 1 };

			// Define gradient colors and their alpha values, adjust alpha of gradient colors to match intensity
			gradientSpecifications.Colors = new Color[3]
			{
				Color.FromArgb(0, Color.Black),
				Color.FromArgb(heatPoint.Intensity, Color.White),
				Color.FromArgb(heatPoint.Intensity, Color.White)
			};

			// Pass off color blend to PathGradientBrush to instruct it how to generate the gradient
			gradientShaper.InterpolationColors = gradientSpecifications;

			// Draw polygon (circle) using our point array and gradient brush
			canvas.FillPolygon(gradientShaper, circumferencePointsArray);
		}

		private static double ConvertDegreesToRadians(double degrees)
		{
			double radians = (Math.PI / 180) * degrees;
			return (radians);
		}

		public static Bitmap Colorize(Bitmap mask, byte Alpha, ColorMap[] colorMap)
		{
			// Create new bitmap to act as a work surface for the colorization process
			Bitmap output = new Bitmap(mask.Width, mask.Height, PixelFormat.Format32bppArgb);
			output.MakeTransparent();

			// Create a graphics object from our memory bitmap so we can draw on it and clear it's drawing surface
			using (Graphics surface = Graphics.FromImage(output))
			{
				surface.Clear(Color.Transparent);

				// Build an array of color mappings to remap our greyscale mask to full color
				// Accept an alpha byte to specify the transparancy of the output image
				// Create new image attributes class to handle the color remappings
				// Inject our color map array to instruct the image attributes class how to do the colorization
				var remapper = new ImageAttributes();
				remapper.SetRemapTable(colorMap);

				// Draw our mask onto our memory bitmap work surface using the new color mapping scheme
				surface.DrawImage(mask, new Rectangle(0, 0, mask.Width, mask.Height), 0, 0, mask.Width, mask.Height, GraphicsUnit.Pixel, remapper);
			}

			// Send back newly colorized memory bitmap
			return output;
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

	}
}
