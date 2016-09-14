using System;
using System.Collections.Generic;
using System.Threading;

namespace LegendDrive
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
			DurationOfCalculation = duration * 1000;
			_samples = new LinkedList<LocationData>();
			_timer = new Timer(x => FireOnValueChanged(), null, 0, 100);
			//_samples.AddLast(new SpeedSample() { Speed = 100, Time = DateTime.Now.Ticks });
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
			}
		}

		protected void Calc()
		{
			lock(this)
			{
				long duration = 0;
				double distance = 0;

				//going from last to first
				var node = _samples.Last;
				var lastSampleTime = DateTime.Now;

				while (duration < DurationOfCalculation && node != null)
				{
					var thisSampleTime = node.Value.Time;
					var dt = (long)(lastSampleTime - thisSampleTime).TotalMilliseconds;
					distance += node.Value.Speed/1000d * dt;
					duration += dt;
					lastSampleTime = thisSampleTime;
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

				_value = duration != 0 ? distance / (duration / 1000d) : 0;
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
