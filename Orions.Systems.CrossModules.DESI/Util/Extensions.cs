using System;

namespace Orions.Systems.CrossModules.Desi.Util
{
	public static class Extensions
	{
		public static string GetImgSource(this byte[] imageBytes) => $"data:image/jpg;base64, {Convert.ToBase64String(imageBytes)}";
		public static bool IsNull<T>(this T? nullable)
			where T: struct
			=> !nullable.HasValue;
	}
}
