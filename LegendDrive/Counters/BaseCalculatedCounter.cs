using System;
using System.ComponentModel;
using System.Globalization;
using System.Threading;
using LegendDrive.Counters.Interfaces;
using LegendDrive.Persistance;
using Newtonsoft.Json.Linq;

namespace LegendDrive.Counters
{
	public abstract class BaseCalculatedCounter<T> : BaseCounter<T>, ISupportStatePersistance 
	{
		T value;
		bool isUpToDate;
		int calccount;

		public BaseCalculatedCounter(string name):base(name)
		{
		}

		public override T Value
		{
			get
			{
				if (IsRunning)
				{
					EnsureInitialized();
					if (RecalcNeeded())
					{
						SuppressEvent();
						calccount++;
						isUpToDate = true;
						value = Calculate();
						ResumeEvents();
						OnValueChanged();
					}
					return value;
				}
				else
				{
					return value;
				}
			}
		}

		protected virtual bool RecalcNeeded()
		{
			return !isUpToDate;
		}

		public override string DebugString
		{
			get
			{
				var chr = isUpToDate ? "+" : "";
				return $"{chr}{calccount}";
			}
		}


		protected abstract T Calculate();
		protected virtual void OnValueChanged()
		{
		}

		protected virtual void Invalidate()
		{
			isUpToDate = false;
			OnPropertyChanged("Value");
		}

		public override void Reset()
		{
			base.Reset();
			Invalidate();
		}

		public override JObject GetState()
		{
			var obj = new JObject();
			obj.AddValue("base", base.GetState());
			obj.AddValue(nameof(value), value);
			return obj;
		}

		public override void LoadState(JObject obj)
		{
			value = obj.GetValue<T>(nameof(value));
			base.LoadState(obj.GetValue<JObject>("base"));
			Invalidate();
		}
	}
}
