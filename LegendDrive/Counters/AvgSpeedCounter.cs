using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
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
		int lastTick;
		IDictionary<object, List<string>> triggers = new Dictionary<object, List<string>>();

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

		public override string DebugString
		{
			get
			{
				return $"{samples.Count.ToString()}/{calccount}";
			}
		}

		protected double Calculate2()
		{
			lock(this)
			{
				if (++calccount >= 1000)
				{
					calccount = 0;
				}
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
				lastTick = Environment.TickCount;
				var value = duration > 0 ? distance / duration : 0;
				return value;
			}
		}

		protected override double Calculate()
		{
			lock (this)
			{
				if (++calccount >= 1000)
				{
					calccount = 0;
				}
				double duration = 0;
				double distance = 0;

				//going from last to first
				var node = samples.Last;
				var now = DateTime.Now.ToUniversalTime();
				var t1 = now;
				//...*(v1)....*(v2)....*(v3)....now
				//v(now-t3) == 0
				//v(t3-t2) == v3
				//v(t2-t1) == v2

				while (duration < DurationOfCalculation && node != null)
				{
					var t0 = node.Value.Time;
					var v = node.Value.Speed;
					var dt = (long)(t1 - t0).TotalSeconds;
					distance += v * dt;
					duration += dt;
					t1 = t0;
					node = node.Previous;
				}

				while (samples.First != null && (now - samples.First.Value.Time).TotalSeconds > DurationOfCalculation)
				{
					samples.RemoveFirst();
				}
				lastTick = Environment.TickCount;
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

		public void AddTrigger(string properties, INotifyPropertyChanged obj)
		{
			var listOfproperties = properties.Split(',').Select(x => x.Trim()).ToList();
			triggers[obj] = listOfproperties;
			obj.PropertyChanged += Value_PropertyChanged;
			Invalidate();
		}

		void Value_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (!IsInitialized) return;
			if (!triggers.ContainsKey(sender)) return;

			if (triggers[sender].Contains(e.PropertyName))
			{
				if (Environment.TickCount - lastTick >= 2000) //refresh speed in case GPS stuck
				{
					Invalidate();
				}
			}
		}
	}
}
