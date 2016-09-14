using System;
using Xamarin.Forms;

namespace LegendDrive
{
	//public static class ViewExtension
	//{
	//	public static T With<T>(this T view, Action<T> action) where T : View
	//	{
	//		action(view);
	//		return view;
	//	}
	//}

	public static class AnyObjectExtension
	{
		public static T With<T>(this T x, Action<T> action)
		{
			action(x);
			return x;
		}
	}
}

