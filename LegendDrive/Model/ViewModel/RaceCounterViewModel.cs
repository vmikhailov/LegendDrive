using System;
using System.Collections.Generic;
using System.Windows.Input;
using LegendDrive.Counters;
using LegendDrive.Counters.Interfaces;
using Xamarin.Forms;

namespace LegendDrive.Model.ViewModel
{
	public class RaceCounterViewModel : BaseBindingObject
	{
		ICommand tapCommand;

		public IRaceCounter counter;
		public RaceCounterViewModel(IRaceCounter counter)
		{
			this.counter = counter;
			counter.PropertyChanged += Counter_PropertyChanged;
			tapCommand = new Command(TapCommandHandler);
		}

		List<string> colorDependencies = new List<string> { "IsRunning", "IsCritical", "IsImportant", "Value" };
		List<string> propertiesToBypass = new List<string> { "Name", "Value", "DebugString" };

		void TapCommandHandler(object parameter)
		{
			if (counter.IsRunning) counter.Stop(); counter.Start();
		}

		void Counter_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if (colorDependencies.Contains(e.PropertyName) || e.PropertyName == ".")
			{
				OnPropertyChanged("Color");
			}

			if (propertiesToBypass.Contains(e.PropertyName))
			{
				OnPropertyChanged(e.PropertyName);
			}

			if (e.PropertyName == "Size")
			{
				OnPropertyChanged("FontSize");
				OnPropertyChanged("FontAttributes");
			}

			if (e.PropertyName == "IsRunning")
			{
				OnPropertyChanged("BackgroundColor");
			}
		}

		public string Value => counter.ValueString;
		public string DebugString => counter.DebugString;
		public string Name => counter.Name;

		public Color Color
		{
			get
			{
				if (!counter.IsRunning) return UIConfiguration.DisabledCounterBorder;
				if (counter.IsCritical) return UIConfiguration.CriticalCounterBorder;
				if (counter.IsImportant) return UIConfiguration.ImportantCounterBorder;
				return UIConfiguration.CounterColors[counter.Color];
			}
		}

		public Color BackgroundColor => counter.IsRunning ?
											   UIConfiguration.EnabledCounterBackground :
											   UIConfiguration.DisabledCounterBackground;

		public double FontSize => UIConfiguration.CounterFontSizes[counter.Size];

		public FontAttributes FontAttributes => counter.Size >= CounterSize.XXL ? FontAttributes.Bold : FontAttributes.None;

		public ICommand TapCommand => tapCommand;
	}
}
