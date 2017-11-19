using System;
using System.Linq.Expressions;
using Xamarin.Forms;

namespace LegendDrive
{
	public static class FuncBinding
	{
		public static Binding Create<TValue, TResult>(string path, Func<TValue, TResult> map)
		{
			return Create<TValue, object, TResult>(path, (value, context) => map(value));
		}

		public static Binding Create<TValue, TContext, TResult>(string path, Func<TValue, TContext, TResult> map, TContext parameter = default(TContext))
		{
			return new Binding(path, BindingMode.OneWay, new FuncValueConverter<TValue, TContext, TResult>(map), parameter, null, null);
		}
	}
}
