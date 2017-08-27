using System;
using System.ComponentModel;
using System.Globalization;
using LegendDrive.Counters.Interfaces;
using LegendDrive.Persistance;
using Newtonsoft.Json.Linq;

namespace LegendDrive.Counters
{
	public abstract class BaseDifferenceCounter<T> : 
		BaseCalculatedCounter<T>,
		IDifferenceCounter<T>, 
		ISupportStatePersistance 
	{
		T baseValue;
		IRaceCounter<T> baseCounter;

		T target;
		IRaceCounter<T> targetCounter;
		int baseChangesCounter;
		int targetChangesCounter;

		public BaseDifferenceCounter(string name):base(name)
		{
		}

		protected override T Calculate()
		{
			var bv = baseCounter != null ? baseCounter.Value : baseValue;
			var tv = targetCounter != null ? targetCounter.Value : target;
			return Subtract(tv, bv);
		}

		protected abstract T Subtract(T v1, T v2);

		public override string DebugString
		{
			get
			{
				if (baseChangesCounter >= 1000)
				{
					baseChangesCounter = 0;
				}
				if (targetChangesCounter >= 1000)
				{
					targetChangesCounter = 0;
				}
				return $"{baseChangesCounter}/{targetChangesCounter}";
			}
		}

		public void SetTarget(T value)
		{
			target = value;
			targetChangesCounter++;
			Invalidate();
		}

		public void SetTarget(IRaceCounter<T> counter)
		{
			targetCounter = counter;
			if (targetCounter is INotifyPropertyChanged)
			{
				var notify = targetCounter as INotifyPropertyChanged;
				notify.PropertyChanged += (sender, e) =>
				{
					if (e.PropertyName == "Value")
					{
						targetChangesCounter++;
						Invalidate();
					}
				};
			}
		}

		public void SetBase(T value)
		{
			baseValue = value;
			baseChangesCounter++;
			Invalidate();
		}

		public void SetBase(IRaceCounter<T> counter)
		{
			baseCounter = counter;
			if (baseCounter is INotifyPropertyChanged)
			{
				var notify = baseCounter as INotifyPropertyChanged;
				notify.PropertyChanged += (sender, e) =>
				{
					if (e.PropertyName == "Value")
					{
						baseChangesCounter++;
						Invalidate();
					}
				};
			}
			Invalidate();
		}
	}
}
