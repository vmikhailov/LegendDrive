using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using LegendDrive.Counters.Interfaces;
using LegendDrive.Persistance;
using Newtonsoft.Json.Linq;

namespace LegendDrive.Counters
{
	public class TriggeredFuncCounter<TObject, TResult> : 
		BaseCalculatedCounter<TResult>, 
		ITriggeredFuncCounter<TObject, TResult>
		where TObject : INotifyPropertyChanged
	{
		IDictionary<object, List<string>> triggers = new Dictionary<object, List<string>>();
		Func<TObject, TResult> getter;
		string format = "#,0";

		public TriggeredFuncCounter(string name)
			: base(name)
		{
		}

		public TriggeredFuncCounter(string name, string format)
			:base(name)
		{
			this.format = format;
		}

		public override string ValueString
		{
			get
			{
				if (typeof(TResult) == typeof(double) || typeof(TResult) == typeof(double?))
				{
					var doubleValue = Convert.ToDouble(Value);
					return doubleValue.ToString(format, NumberFormatInfo);
				}
				if (typeof(TResult) == typeof(TimeSpan))
				{
					var ts = (TimeSpan)ValueObject;
					return string.Format(@"{1}{0:hh\:mm\:ss}", ts, ts.TotalSeconds < 0 ? "-" : "");
				}
				else
				{
					return string.Format(format, ValueObject);
				}
			}
		}

		public TObject BindingContext
		{
			get; set;
		}

		public void BindTo(TObject value, Func<TObject, TResult> getter) 
		{
			this.getter = getter;
			this.BindingContext = value;
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
				Invalidate();
			}
		}

		protected override void Invalidate()
		{
			base.Invalidate();
			if (AfterNewValue != null) AfterNewValue(this);
		}

		protected override TResult Calculate()
		{
			return getter(BindingContext);
		}

		public Action<ITriggeredFuncCounter<TObject, TResult>> AfterNewValue
		{
			get;
			set;
		}
	}
}

