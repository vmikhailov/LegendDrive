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
			get { return string.Format(@"{1}{0:hh\:mm\:ss}", TypedValue, TypedValue?.TotalMilliseconds < 0?"-":""); }
		}

		protected override void Recalc()
		{
			base.Recalc();
			if (IsRunning)
			{
				SetImportant(TypedValue?.TotalSeconds <= 10);
				SetCritical(TypedValue?.TotalSeconds <= 0);
			}
		}

		protected override TimeSpan? Subtract(TimeSpan? v1, TimeSpan? v2)
		{
			return v1 - v2;
		}
	}
}
