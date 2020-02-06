using System;

namespace Orions.Systems.CrossModules.Components
{
	public class CardItem
	{
		public string Title { get; set; }

		public string Value { get; set; }

		public int? IntValue
		{
			get
			{
				var value = DoubleValue;
				if (value == null) return null;

				return Convert.ToInt32(value);
			}
		}

		public double? DoubleValue
		{
			get
			{
				double value;
				var result = double.TryParse(Value, out value);
				if (!result) return null;
				return value;
			}
		}

		public int Percentage { get; set; }

		public string IconHtml { get; set; }
	}
}
