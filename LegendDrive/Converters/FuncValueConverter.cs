using System;
using System.Globalization;
using Xamarin.Forms;

namespace LegendDrive
{
	public class FuncValueConverter<TValue, TContext, TResult>  : IValueConverter
	{
		Func<TValue, TContext, TResult> map; 
		public FuncValueConverter(Func<TValue, TContext, TResult> map)
		{
			this.map = map;
		}

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			var k = (TValue)value;
			return map(k, (TContext)parameter);
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}

