using System;
using System.Collections.Generic;
using System.Threading;
using LegendDrive.Counters.Interfaces;
using LegendDrive.Model;
using Newtonsoft.Json.Linq;

namespace LegendDrive.Counters
{
	public class AvgSpeedCounter : BaseCalculatedCounter<double>, ILocationProcessor
	{
		LinkedList<LocationData> samples;
		int calccount;

		public AvgSpeedCounter() : this("AvgSpeed", 10)
		{
		}

		public AvgSpeedCounter(string name) : this(name, 10)
		{
		}

		public AvgSpeedCounter(string name, int duration)
			:base(name)
		{
			DurationOfCalculation = duration;
			samples = new LinkedList<LocationData>();
		}

		public long DurationOfCalculation
		{
			get;
			set;
		}

		public override string ValueString
		{
			get { return string.Format("{0:F0} m/s", Value); }
		}

		protected override double Calculate()
		{
			lock(this)
			{
				calccount++;
				double duration = 0;
				double distance = 0;

				//going from last to first
				var node = samples.Last;
				var t1 = DateTime.Now.ToUniversalTime();
				//...*(v1)....*(v2)....*(v3)....now
				//v(now-t3) == 0
				//v(t3-t2) == v3
				//v(t2-t1) == v2

				double v = 0;
				while (duration < DurationOfCalculation && node != null)
				{
					var t0 = node.Value.Time;
					var dt = (long)(t1 - t0).TotalSeconds;
					distance += v * dt;
					duration += dt;
					t1 = t0;
					v = node.Value.Speed;
					node = node.Previous;
				}

				if (node != null)
				{
					//we have reached max duration. need to clean up samples
					node = node.Next;
					while (samples.First != node)
					{
						samples.RemoveFirst();
					}
				}

				var value = duration > 0 ? distance / duration : 0;
				return value;
			}
		}

		public virtual void SetLocation(LocationData location)
		{
			if (!location.GpsOn) return;
			if (IsRunning)
			{
				lock(this)
				{
					samples.AddLast(location);
				}
				Invalidate();
			}
		}

		public override void Reset()
		{
			base.Reset();
			lock(this)
			{
				samples.Clear();
			}
			Invalidate();
		}
	}
}
