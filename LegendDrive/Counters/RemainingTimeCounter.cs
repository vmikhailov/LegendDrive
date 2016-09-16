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

		protected override TimeSpan? Calculate()
		{
			var x = base.Calculate();

			return x;
		}

		protected override void OnValueChanged()
		{
			if (IsRunning)
			{
				SetImportant(Value.GetValueOrDefault().TotalSeconds <= 10);
				SetCritical(Value.GetValueOrDefault().TotalSeconds <= 0);
			}
		}

		protected override TimeSpan? Subtract(TimeSpan? v1, TimeSpan? v2)
		{
			return v1 - v2;
		}
	}
}
