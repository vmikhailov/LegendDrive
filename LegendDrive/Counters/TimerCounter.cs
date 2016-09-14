using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace LegendDrive
{
	public class TimerCounter : BaseCounter<TimeSpan?>, ISupportHistory
	{
		private Timer _timer;
		private long _elapsed;
		private int _accuracy = 500;
		Stack<long> _history;

		public TimerCounter() : this("Timer")
		{
		}

		public TimerCounter(string name)
			: base(name)
		{
			_timer = new Timer(x => NewTick(), null, 0, _accuracy);
			_history = new Stack<long>();
		}

		public override string ValueString
		{
			get { return string.Format(@"{0:hh\:mm\:ss}", TypedValue); }
		}

		public override TimeSpan? TypedValue
		{
			get
			{
				EnsureInitialized();
				return TimeSpan.FromMilliseconds(_elapsed);
			}
		}

		private void NewTick()
		{
			if (IsRunning)
			{
				_elapsed += _accuracy;
			}
			OnPropertyChanged("Value");
		}

		public override void Reset()
		{
			_elapsed = 0;
		}

		public override void Dispose()
		{
			base.Dispose();
			if (_timer != null) _timer.Dispose();
		}

		public void Push()
		{
			_history.Push(_elapsed);
			_elapsed = 0;
			OnPropertyChanged("Value");
		}

		public void Pop()
		{
			if (_history.Any())
			{
				_elapsed = _elapsed + _history.Pop();
				OnPropertyChanged("Value");
			}
		}
	}
}
