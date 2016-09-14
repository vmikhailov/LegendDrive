using System;
using System.Globalization;
using Xamarin.Forms;

namespace LegendDrive
{
	public class CounterToStringConverter : IValueConverter
	{
		public CounterToStringConverter()
		{
		}

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value == null) return "<null value>";
			var counter = (IRaceCounter)value;
			//var cv = counter.Value;
			//if (cv == null) return "<empty value>";

			return counter.ValueString;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}

