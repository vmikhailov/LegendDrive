using System;
using System.ComponentModel;
using System.Globalization;

namespace LegendDrive.Counters
{
	public class RemainingDistanceCounter : BaseDifferenceCounter<double?>
	{
		public RemainingDistanceCounter():this("Remaining distance")
		{
		}

		public RemainingDistanceCounter(string name)
			:base(name)
		{

		}

		public override string ValueString
		{
			get { return TypedValue?.ToString("#,0", NumberFormatInfo); }
		}

		protected override void Recalc()
		{
			base.Recalc();
			if (IsRunning)
			{
				SetImportant(TypedValue < 100);
				SetCritical(TypedValue <= 0);
			}
		}

		public override void Start()
		{
			base.Start();
			Recalc();
		}

		public override void Reset()
		{
			Recalc();
		}

		protected override double? Subtract(double? v1, double? v2)
		{
			return v1 - v2;
		}
	}
}
