using System;
using System.Collections.Generic;
using Xamarin.Forms;

namespace LegendDrive
{
	public static class CountersPanelBuilderHelper
	{
		public static View BuildAdaptivePanel(IReadOnlyList<View> views, int minWidth = UIConfiguration.CounterWidth)
		{
			var relative = new RelativeLayout()
			{

			};

			int count = views.Count;
			for (int i = 0; i < count; i++)
			{
				var ii = i;
				relative.Children.Add(views[i],
								Constraint.RelativeToParent((parent) => GetCounterX(ii, parent, minWidth, count)),
								Constraint.RelativeToParent((parent) => GetCounterY(ii, parent, minWidth, count)),
								Constraint.RelativeToParent((parent) => GetCounterWidth(ii, parent, minWidth, count)),
								Constraint.RelativeToParent((parent) => GetCounterHeight(ii, parent, minWidth, count)));
			}
			return relative;
		}

		private static double GetCounterX(int no, View parent, int minControlWidth, int cc)
		{
			return (no % CalcCounterPerRow(parent, minControlWidth, cc)) * GetCounterWidth(no, parent, minControlWidth, cc);
		}


		private static double GetCounterY(int no, View parent, int minControlWidth, int cc)
		{
			return (no / CalcCounterPerRow(parent, minControlWidth, cc)) * GetCounterHeight(no, parent, minControlWidth, cc);
		}

		private static double GetCounterWidth(int no, View parent, int minControlWidth, int cc)
		{
			return parent.Width / CalcCounterPerRow(parent, minControlWidth, cc);
		}

		private static double GetCounterHeight(int no, View parent, int minControlWidth, int cc)
		{
			var perRow = (double)CalcCounterPerRow(parent, minControlWidth, cc);
			var height = parent.Height / Math.Ceiling(cc / perRow);
			height = Math.Min(height, UIConfiguration.CounterHeight * 3);
			if ((parent.Height - height) < parent.Height * 0.1)
			{
				height = parent.Height;
			}
			return height;
		}

		private static int CalcCounterPerRow(View parent, int minControlWidth, int cc)
		{
			var c = (int)Math.Truncate(parent.Width / minControlWidth);
			return (c > cc) ? cc : c;
		}
	}
}

