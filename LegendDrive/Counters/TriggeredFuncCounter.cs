﻿using System;
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
		LinkedList<TResult> approximationList = new LinkedList<TResult>();


		public TriggeredFuncCounter(string name)
			: base(name)
		{
		}

		public TriggeredFuncCounter(string name, string format)
			: base(name)
		{
			this.format = format;
		}

		public int ApproximationCount { get; set; } = 1;
		public Func<IEnumerable<TResult>, TResult> ApproximationFunction { get; set; }

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
					if (ts.Days == 1)
					{
						return "∞";
					}
					else
					{
						return string.Format(@"{1}{0:hh\:mm\:ss}", ts, ts.TotalSeconds < 0 ? "-" : "");
					}
				}
				else
				{
					return string.Format(format, ValueObject);
				}
			}
		}

		public override TResult Value
		{
			get
			{
				return ApproximationCount <= 1 ? base.Value : GetApproximatedValue(base.Value);
			}
		}

		public TObject BindingContext
		{
			get; set;
		}

		private TResult GetApproximatedValue(TResult newValue)
		{
			approximationList.AddLast(newValue);
			while (approximationList.Count() > ApproximationCount)
			{
				approximationList.RemoveFirst();
			}
			return ApproximationFunction(approximationList);
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

		public override void Start()
		{
			base.Start();
			//if (!(Name.Contains("race") || Name.Contains("Race") || Name.Contains("time") || Name.Contains("speed")))
			//if(!Name.Contains("GPS"))
			//{
				Invalidate();
			//}
		}

		protected override void Invalidate()
		{
			base.Invalidate();
			//if (AfterNewValue != null) AfterNewValue(this);
		}

		protected override TResult Calculate()
		{
			var value = getter(BindingContext);
			return value;
		}

		//public Action<ITriggeredFuncCounter<TObject, TResult>> AfterNewValue
		//{
		//	get;
		//	set;
		//}
	}
}

