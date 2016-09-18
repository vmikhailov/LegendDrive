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

		public double CriticalThreshold { get; set; } = 0.5;

		public double ImportantThreshold { get; set; } = 100;

		public override bool IsCritical => IsRunning && Value < CriticalThreshold;

		public override bool IsImportant => IsRunning && Value < ImportantThreshold;

		protected override double? Subtract(double? v1, double? v2)
		{
			return v1 - v2;
		}
	}
}
