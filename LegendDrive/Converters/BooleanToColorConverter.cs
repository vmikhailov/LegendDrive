using System;
using System.Globalization;
using Xamarin.Forms;

namespace LegendDrive
{
	public class BooleanToColorConverter : IValueConverter
	{
		public BooleanToColorConverter()
		{
		}

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			var x = (bool)value;
			return x ? Color.Red : Color.White;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}

