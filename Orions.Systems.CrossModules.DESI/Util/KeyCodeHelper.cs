namespace Orions.Systems.CrossModules.Desi.Util
{
	public static class KeyCodeHelper
	{
		private static readonly string[] _keys =
			{"Q", "W", "E", "R", "Y", "U", "I", "O", "P", "A", "S", "D", "F", "G", "H", "J", "K", "L", "Z", "X", "C", "V", "B", "N", "M"};

		public static string GetStringKey(int digitDelta)
		{
			var key = string.Empty;
			if (digitDelta > 9)
			{
				var charIndex = digitDelta - 10;
				if (charIndex < _keys.Length)
				{
					key = _keys[charIndex];
				}
			}
			else
			{
				key = digitDelta.ToString();
			}

			return key;
		}
	}
}
