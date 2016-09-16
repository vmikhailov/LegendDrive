using System;
using System.ComponentModel;
using System.Globalization;
using LegendDrive.Counters.Interfaces;
using LegendDrive.Persistance;
using Newtonsoft.Json.Linq;

namespace LegendDrive.Counters
{
	public abstract class BaseDifferenceCounter<T> : 
		BaseCounter<T>, 
		IDifferenceCounter<T>, 
		ISupportStatePersistance 
	{
		T _base;
		IRaceCounter<T> _baseCounter;

		T _target;
		IRaceCounter<T> _targetCounter;

		T _difference;

		public BaseDifferenceCounter(string name):base(name)
		{
		}

		public override T TypedValue
		{
			get
			{
				if (IsRunning)
				{
					EnsureInitialized();
					return _difference;
				}
				else
				{
					return _difference;
				}
			}
		}

		protected virtual void Recalc()
		{
			if (IsRunning)
			{
				var baseValue = _baseCounter != null ? _baseCounter.TypedValue : _base;
				var targetValue = _targetCounter != null ? _targetCounter.TypedValue : _target;
				_difference = Subtract(targetValue, baseValue);
				OnPropertyChanged("Value");
			}
		}

		protected abstract T Subtract(T v1, T v2);

		public override void Reset()
		{
			Recalc();
		}

		public void SetTarget(T value)
		{
			_target = value;
		}

		public void SetTarget(IRaceCounter<T> counter)
		{
			_targetCounter = counter;
			if (_targetCounter is INotifyPropertyChanged)
			{
					var notify = _targetCounter as INotifyPropertyChanged;
					notify.PropertyChanged += (sender, e) => Recalc();
			}
			Recalc();
		}

		public void SetBase(T value)
		{
			_base = value;
		}

		public void SetBase(IRaceCounter<T> counter)
		{
			_baseCounter = counter;
			if (_baseCounter is INotifyPropertyChanged)
			{
				var notify = _baseCounter as INotifyPropertyChanged;
				notify.PropertyChanged += (sender, e) => Recalc();
			}
			Recalc();
		}

		public override JObject GetState()
		{
			var obj = new JObject();
			obj.AddValue("base", base.GetState());
			obj.AddValue(nameof(_difference), _difference);
			return obj;
		}

		public override void LoadState(JObject obj)
		{
			_difference = obj.GetValue<T>(nameof(_difference));
			base.LoadState(obj.GetValue<JObject>("base"));
			OnPropertyChanged("Value");
		}
	}
}
