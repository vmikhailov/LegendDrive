using System;

namespace LegendDrive.Counters
{
	public class AvgSpeedKmhCounter : AvgSpeedCounter
	{
		public AvgSpeedKmhCounter() : this("AvgSpeedKmh", 10)
		{
		}

		public AvgSpeedKmhCounter(string name) : this(name, 10)
		{
		}

		public AvgSpeedKmhCounter(string name, int duration)
			:base(name, duration)
		{
		}

		public override string ValueString
		{
			get 
			{
				var rounded = Math.Round(TypedValue, 1);
				string valueStr;
				if (rounded < 10)
				{
					valueStr = rounded.ToString("0.#", NumberFormatInfo);
				}
				else
				{
					valueStr = rounded.ToString("#.", NumberFormatInfo);
				}
				return string.Format("{0}", valueStr); 
			}
		}

		public override double TypedValue
		{
			get
			{
				return base.TypedValue * 3.6;
			}
		}
	}
}
