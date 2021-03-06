﻿using System;
using System.Threading;

namespace LegendDrive.Counters
{
	public class CurrentTimeCounter : BaseCounter<DateTime>
	{
		private Timer timer;


		public CurrentTimeCounter() : this("Current time")
		{
		}

		public CurrentTimeCounter(string name)
			: base(name)
		{
			timer = new Timer(x => RaisePropertyChanged(nameof(Value)), null, 0, 1000);
		}

		public override bool IsRunning
		{
			get
			{
				return true;
			}
		}

		public override void Start()
		{
		}

		public override void Stop()
		{
		}

		public override string ValueString
		{
			get { return string.Format(@"{0:H\:mm\:ss}", ValueObject); }
		}

		public override DateTime Value
		{
			get 
			{
				//EnsureInitialized();
				return DateTime.Now; 
			}
		}

		public override void Dispose()
		{
			base.Dispose();
			if (timer != null) timer.Dispose();
		}
	}
}
