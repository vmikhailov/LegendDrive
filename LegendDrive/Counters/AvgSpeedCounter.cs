using System;
using System.Collections.Generic;
using System.Threading;
using LegendDrive.Counters.Interfaces;
using LegendDrive.Model;
using Newtonsoft.Json.Linq;

namespace LegendDrive.Counters
{
	public class AvgSpeedCounter : BaseCounter<double>, ILocationProcessor
	{
		LinkedList<LocationData> _samples;
		Timer _timer;
		double _value;
		int _lastEventFire; 

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
			_samples = new LinkedList<LocationData>();
			_timer = new Timer(x => FireOnValueChanged(), null, 0, 100);
		}

		public long DurationOfCalculation
		{
			get;
			set;
		}

		public override string ValueString
		{
			get { return string.Format("{0:F0} m/s", TypedValue); }
		}


		public override double TypedValue
		{
			get
			{
				EnsureInitialized();
				return _value;
				//return _samples.Count/3.6;
			}
		}

		protected void Calc()
		{
			lock(this)
			{
				double duration = 0;
				double distance = 0;

				//going from last to first
				var node = _samples.Last;
				var t1 = DateTime.Now;
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
					while (_samples.First != node)
					{
						_samples.RemoveFirst();
					}
				}

				_value = duration > 0 ? distance / duration : 0;
			}
		}

		private void FireOnValueChanged()
		{
			var tick = System.Environment.TickCount/1000;
			if ((tick - _lastEventFire) > 0)
			{
				Calc();
				OnPropertyChanged("Value");
				_lastEventFire = tick;
			}
		}

		public virtual void SetLocation(LocationData location)
		{
			if (!location.GpsOn) return;
			if (IsRunning)
			{
				lock(this)
				{
					_samples.AddLast(location);
				}
				FireOnValueChanged();
			}
		}

		public override void Reset()
		{
			lock(this)
			{
				_samples.Clear();
			}
			FireOnValueChanged();
		}

		public override void Dispose()
		{
			base.Dispose();
			if(_timer !=null) _timer.Dispose();
		}
	}
}
