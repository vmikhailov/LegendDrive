using System;
using System.Diagnostics;
using System.Globalization;
using System.Threading;
using LegendDrive.Counters.Interfaces;
using LegendDrive.Persistance;
using Newtonsoft.Json.Linq;

namespace LegendDrive.Counters
{
	[DebuggerDisplay("{Name} = {ValueString}")]
	public abstract class BaseCounter<T> : BaseBindingObject, IRaceCounter<T>, IDisposable, ISupportStatePersistance
	{
		private string name;
		private bool important;
		private bool critical;
		private bool running;
		private CounterSize size;
		private CounterColor color;
		private Timer timer;
		private NumberFormatInfo nfi;
		private bool initialized;

		protected BaseCounter(string name)
		{
			this.name = name;
			nfi = (NumberFormatInfo)CultureInfo.InvariantCulture.NumberFormat.Clone();
			nfi.NumberGroupSeparator = " ";
		}

		protected NumberFormatInfo NumberFormatInfo
		{
			get { return nfi; }
		}

		public abstract string ValueString
		{
			get;
		}

		public object ValueObject
		{
			get { return Value; }
		}

		public abstract T Value
		{
			get;
		}

		public virtual void Init()
		{
			initialized = true;
		}

		public void EnsureInitialized()
		{
			if (!IsInitialized) throw new Exception($"Counter {Name} is not initialized");
		}

		public virtual void Reset()
		{
		}

		public virtual bool IsInitialized
		{
			get { return initialized; }
		}

		public string Name
		{
			get
			{
				return name;
			}
		}

		public void Flash()
		{
			Flash(500);
		}

		public void Flash(int msec)
		{
			if (critical)
			{
				if (timer != null) timer.Dispose();
			}
			SetCritical(true);
			timer = new Timer(TurnOffFlash, this, msec, 0);
		}

		private void TurnOffFlash(object state)
		{
			SetCritical(false);
		}

		public virtual CounterSize Size
		{
			get { return size; }
			set
			{
				if (size != value)
				{
					size = value;
					OnPropertyChanged("Size");
				}
			}
		}

		public virtual CounterColor Color
		{
			get { return color; }
			set
			{
				if (color != value)
				{
					color = value;
					OnPropertyChanged("Color");
				}
			}
		}

		public virtual bool IsImportant
		{
			get { return important; }
		}

		public virtual bool IsCritical
		{
			get { return critical; }
		}

		public virtual bool IsRunning
		{
			get { return running; }
		}

		public virtual void Start()
		{
			EnsureInitialized();
			if (!running)
			{
				running = true;
				OnPropertyChanged("IsRunning");
			}
		}

		public virtual void Stop()
		{
			if (running)
			{
				running = false;
				OnPropertyChanged("IsRunning");
			}
		}

		public virtual void Dispose()
		{
			if (timer != null) timer.Dispose();
		}

		public void SetImportant(bool value)
		{
			if (important != value)
			{
				important = value;
				OnPropertyChanged("IsImportant");
			}
		}

		public void SetCritical(bool value)
		{
			if (critical != value)
			{
				critical = value;
				OnPropertyChanged("IsCritical");
			}
		}

		public virtual JObject GetState()
		{
			var obj = new JObject();
			obj.AddValue(nameof(running), running);
			return obj;
		}

		public virtual void LoadState(JObject obj)
		{
			running = obj.GetValue<bool>(nameof(running));
			OnPropertyChanged("IsRunning");
		}
	}

}
