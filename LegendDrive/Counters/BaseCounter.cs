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
		private string _name;
		private bool _important;
		private bool _critical;
		private bool _running;
		private CounterSize _size;
		private CounterColor _color;
		private Timer _timer;
		private NumberFormatInfo _nfi;
		private bool _initialized;

		protected BaseCounter(string name)
		{
			_name = name;
			_nfi = (NumberFormatInfo)CultureInfo.InvariantCulture.NumberFormat.Clone();
			_nfi.NumberGroupSeparator = " ";
		}

		protected NumberFormatInfo NumberFormatInfo
		{
			get { return _nfi; }
		}

		public abstract string ValueString
		{
			get;
		}

		public object Value
		{
			get { return TypedValue; }
		}

		public abstract T TypedValue
		{
			get;
		}

		public virtual void Init()
		{
			_initialized = true;
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
			get { return _initialized; }
		}

		public string Name
		{
			get
			{
				return _name;
			}
		}

		public void Flash()
		{
			Flash(500);
		}

		public void Flash(int msec)
		{
			if (_critical)
			{
				if (_timer != null) _timer.Dispose();
			}
			_critical = true;
			OnPropertyChanged("IsCritical");
			_timer = new Timer(TurnOffFlash, this, msec, 0);
		}

		private void TurnOffFlash(object state)
		{
			_critical = false;
			OnPropertyChanged("IsCritical");
		}

		public virtual CounterSize Size
		{
			get { return _size; }
			set
			{
				_size = value;
				OnPropertyChanged("Size");
			}
		}

		public virtual CounterColor Color
		{
			get { return _color; }
			set
			{
				_color = value;
				OnPropertyChanged("Color");
			}
		}

		public virtual bool IsImportant
		{
			get { return _important; }
		}

		public virtual bool IsCritical
		{
			get { return _critical; }
		}

		public virtual bool IsRunning
		{
			get { return _running; }
		}

		public virtual void Start()
		{
			EnsureInitialized();
			_running = true;
			OnPropertyChanged("IsRunning");
		}

		public virtual void Stop()
		{
			_running = false;
			OnPropertyChanged("IsRunning");
		}

		public virtual void Dispose()
		{
			if (_timer != null) _timer.Dispose();
		}

		public void SetImportant(bool value)
		{
			_important = value;
			OnPropertyChanged("IsImportant");
		}

		public void SetCritical(bool value)
		{
			_critical = value;
			OnPropertyChanged("IsCritical");
		}

		public virtual JObject GetState()
		{
			var obj = new JObject();
			obj.AddValue(nameof(_running), _running);
			return obj;
		}

		public virtual void LoadState(JObject obj)
		{
			_running = obj.GetValue<bool>(nameof(_running));
			OnPropertyChanged("IsRunning");
		}
	}
	
}
