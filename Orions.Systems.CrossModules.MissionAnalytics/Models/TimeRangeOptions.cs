namespace Orions.Systems.CrossModules.MissionAnalytics.Model
{
	public struct TimeRangeOptions
	{
		public const double LastHour = 0.0417;
		public const double Last2Hours = 0.0833;
		public const double Last3Hours = 0.125;
		public const double Last6Hours = 0.25;
		public const double Last12Hours = 0.5;
		public const double LastDay = 1;
		public const double Last3Days = 3;
		public const double LastWeek = 7;
		public const double LastMonth = 30;
		public const double Last3Months = 90;
		public const double Last6Months = 180;
		public const double LastYear = 365;
		public const double Ever = 0;
	}
}