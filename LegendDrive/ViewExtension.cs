using System;
using Xamarin.Forms;

namespace LegendDrive
{
	public static class AnyObjectExtension
	{
		public static T With<T>(this T x, Action<T> action)
		{
			action(x);
			return x;
		}
	}
}

