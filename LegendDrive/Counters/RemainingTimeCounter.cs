using System;
using System.ComponentModel;

namespace LegendDrive.Counters
{
	public class RemainingTimeCounter : BaseDifferenceCounter<TimeSpan?>
	{
		public RemainingTimeCounter():this("Remaining timer")
		{
		}

		public RemainingTimeCounter(string name):base(name)
		{
		}

		public override string ValueString
		{
			get { return string.Format(@"{1}{0:hh\:mm\:ss}", Value, Value?.TotalMilliseconds < 0?"-":""); }
		}

		public int CriticalThresholdSec { get; set; } = 1;

		public int ImportantThresholdSec { get; set; } = 11;

		public override bool IsCritical => IsRunning && TotalSeconds < CriticalThresholdSec;

		public override bool IsImportant => IsRunning && TotalSeconds < ImportantThresholdSec;

		private int TotalSeconds => (int)Value.GetValueOrDefault().TotalSeconds;

		protected override TimeSpan? Subtract(TimeSpan? v1, TimeSpan? v2)
		{
			return v1 - v2;
		}
	}
}
