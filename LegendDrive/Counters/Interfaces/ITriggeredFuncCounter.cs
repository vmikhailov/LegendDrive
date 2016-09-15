using System;
using System.ComponentModel;

namespace LegendDrive.Counters.Interfaces
{
	public interface ITriggeredFuncCounter<TObject, TResult> : IRaceCounter<TResult> where TObject : INotifyPropertyChanged
	{
		void BindTo(TObject value, Func<TObject, TResult> getter);
		void AddTrigger(string property, INotifyPropertyChanged obj);
		Action<ITriggeredFuncCounter<TObject, TResult>> AfterNewValue { get; set;}
		TObject BindingContext { get; set; }
	}
}

