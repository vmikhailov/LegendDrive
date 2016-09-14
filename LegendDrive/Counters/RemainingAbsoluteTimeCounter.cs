using System;
using System.ComponentModel;

namespace LegendDrive
{
	public class RemainingAbsoluteTimeCounter : BaseDifferenceCounter<TimeSpan>
	{
		public RemainingAbsoluteTimeCounter():this("Remaining time")
		{
		}

		public RemainingAbsoluteTimeCounter(string name):base(name)
		{
		}

		public override string ValueString
		{
			get { return string.Format(@"{1}{0:hh\:mm\:ss}", Value, Value.TotalMilliseconds < 0?"-":""); }
		}

		protected override void Recalc()
		{
			base.Recalc();
			SetImportant(Value.TotalMilliseconds < 0);
		}
	}
}
