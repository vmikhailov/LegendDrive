using System;
using System.Globalization;
using LegendDrive.Counters.Interfaces;
using Xamarin.Forms;

namespace LegendDrive
{
	public class CounterToStringConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value == null) return "<null value>";
			var counter = (IRaceCounter)value;

			return counter.ValueString;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}

