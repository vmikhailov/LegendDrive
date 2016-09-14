using System;
using System.ComponentModel;
using System.Globalization;

namespace LegendDrive
{
	public abstract class BaseDifferenceCounter<T> : BaseCounter<T>, IDifferenceCounter<T>
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
					return default(T);
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
	}
}
