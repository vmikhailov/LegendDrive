using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;

namespace LegendDrive
{
	public class TriggeredFuncCounter<TObject, TResult> : BaseCounter<TResult>, ITriggeredFuncCounter<TObject, TResult> 
		where TObject : INotifyPropertyChanged
	{
		IDictionary<object, List<string>> triggers = new Dictionary<object, List<string>>();
		TResult value;
		Func<TObject, TResult> getter;
		string format = "#,0";

		public override string ValueString
		{
			get
			{
				if (typeof(TResult) == typeof(double) || typeof(TResult) == typeof(double?))
				{
					var doubleValue = Convert.ToDouble(value);
					return doubleValue.ToString(format, NumberFormatInfo);
				}
				if (typeof(TResult) == typeof(TimeSpan))
				{
					var ts = (TimeSpan)Value;
					return string.Format(@"{1}{0:hh\:mm\:ss}", ts, ts.TotalSeconds < 0 ? "-" : "");
				}
				else
				{
					return string.Format(format, Value);
				}
			}
		}

		public override TResult TypedValue
		{
			get
			{
				EnsureInitialized();
				return value;
			}
		}

		public TObject BindingContext
		{
			get; set;
		}

		public TriggeredFuncCounter(string name)
			: base(name)
		{
		}

		public TriggeredFuncCounter(string name, string format)
			:base(name)
		{
			this.format = format;
		}

		public void BindTo(TObject value, Func<TObject, TResult> getter) 
		{
			this.getter = getter;
			this.BindingContext = value;
		}

		public void AddTrigger(string properties, INotifyPropertyChanged obj)
		{
			var listOfproperties = properties.Split(',').Select(x => x.Trim()).ToList();
			triggers[obj] = listOfproperties;
			obj.PropertyChanged += Value_PropertyChanged;
		}

		void Value_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (!IsInitialized) return;
			if (!triggers.ContainsKey(sender)) return;

			if (triggers[sender].Contains(e.PropertyName))
			{
				this.value = getter(BindingContext);
				if (AfterNewValue != null) AfterNewValue(this);
				OnPropertyChanged("Value");
			}
		}

		public override void Reset()
		{
			base.Reset();
			OnPropertyChanged("Value");
		}

		public Action<ITriggeredFuncCounter<TObject, TResult>> AfterNewValue
		{
			get;
			set;
		}
	}
}

