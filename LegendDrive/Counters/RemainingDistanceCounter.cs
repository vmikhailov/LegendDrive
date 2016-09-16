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
			get { return Value?.ToString("#,0", NumberFormatInfo); }
		}

		protected override double? Calculate()
		{
			var x = base.Calculate();

			return x;
		}

		protected override void OnValueChanged()
		{
			if (IsRunning)
			{
				SetImportant(Value < 100);
				SetCritical(Value <= 0);
			}
		}

		protected override double? Subtract(double? v1, double? v2)
		{
			return v1 - v2;
		}
	}
}
