﻿using System;

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

		public double Percentage { get; set; }

		public int PercentagePrecision { get; set; } = 2;

		public string SvgIcon { get; set; }
	}
}
