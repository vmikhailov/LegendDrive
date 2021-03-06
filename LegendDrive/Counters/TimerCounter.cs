using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using LegendDrive.Counters.Interfaces;
using LegendDrive.Persistance;
using Newtonsoft.Json.Linq;

namespace LegendDrive.Counters
{
	public class TimerCounter : BaseCounter<TimeSpan?>, ISupportHistory, ISupportStatePersistance
	{
	    Timer timer;
		long elapsed;
		int accuracy = 500;
		Stack<long> history;
		int calccount;

		public TimerCounter() : this("Timer")
		{
		}

		public TimerCounter(string name)
			: base(name)
		{
			timer = new Timer(x => NewTick(), null, 0, accuracy);
			history = new Stack<long>();
		}

		public override string ValueString
		{
			get { return string.Format(@"{0:hh\:mm\:ss}", Value); }
		}

		public override string DebugString
		{
			get
			{
				return $"{calccount}";
			}
		}

		public override TimeSpan? Value
		{
			get
			{
				EnsureInitialized();
				return TimeSpan.FromMilliseconds(elapsed);
			}
		}

		private void NewTick()
		{
			if (IsRunning)
			{
				if (++calccount >= 1000)
				{
					calccount = 0;
				}
				elapsed += accuracy;
				RaisePropertyChanged(nameof(Value));
			}
		}

		public override void Reset()
		{
			elapsed = 0;
			history.Clear();
		}

		public override void Dispose()
		{
			base.Dispose();
			if (timer != null) timer.Dispose();
		}

		public void Push()
		{
			history.Push(elapsed);
			elapsed = 0;
			RaisePropertyChanged(nameof(Value));
		}

		public void Pop()
		{
			if (history.Any())
			{
				elapsed = elapsed + history.Pop();
				RaisePropertyChanged(nameof(Value));
			}
		}

		public override JObject GetState()
		{
			var obj = new JObject();
			obj.AddValue("base", base.GetState());
			obj.AddValue(nameof(elapsed), elapsed);
			obj.AddValue(nameof(history), history);
			return obj;
		}

		public override void LoadState(JObject obj)
		{
			elapsed = obj.GetValue<long>(nameof(elapsed));
			history = obj.GetValue<Stack<long>>(nameof(history));
			base.LoadState(obj.GetValue<JObject>("base"));
			RaisePropertyChanged(nameof(Value));
		}
	}
}
