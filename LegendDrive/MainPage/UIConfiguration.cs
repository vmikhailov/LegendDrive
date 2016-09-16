using System.Collections.Generic;
using LegendDrive.Counters.Interfaces;
using Xamarin.Forms;

namespace LegendDrive
{
	public static class UIConfiguration
	{
		public const int ButtonWidth = 96;
		public const int ButtonHeight = 64;
		public const int ButtonFontSize = ButtonHeight / 2;

		public const int LargeButtonWidth = 96 * 2;
		public const int LargeButtonHeight = 64 + 8;
		public const int LargeButtonFontSize = LargeButtonHeight / 5;
		public const int SegmentListNoFontSize = (int)(ButtonFontSize * 0.5);
		public const int SegmentListLengthFontSize = (int)(ButtonFontSize * 0.7);
		public const int SegmentListSpeedFontSize = (int)(ButtonFontSize * 0.7);
		public const int SegmentListTimeoutFontSize = (int)(ButtonFontSize * 0.5);

		public const int SegmentListNoSize = (int)(ButtonWidth * 0.5);
		public const int SegmentListLengthSize = (int)(ButtonWidth * 1.2);
		public const int SegmentListSpeedSize = (int)(ButtonWidth * 0.7);
		public const int SegmentListTimeoutSize = (int)(ButtonWidth * 0.6);

		public const int CounterWidth = 196;
		public const int CounterHeight = 64;

		public const int PanelWidth = ButtonWidth * 3;
		public static Color ButtonColor = Xamarin.Forms.Color.Green.WithSaturation(0.25);
		public static Color EnabledCounterBackground = Color.FromHex("2a3872").WithLuminosity(0.2);
		public static Color DisabledCounterBackground = Color.Gray.WithLuminosity(0.1);

		public static Color EnabledCounterBorder = Color.FromHex("cccccc");
		public static Color DisabledCounterBorder = Color.Gray;
		public static Color ImportantCounterBorder = Color.Yellow;
		public static Color CriticalCounterBorder = Color.FromHex("e20026");//Color.Red;

		public static IDictionary<CounterColor, Color> CounterColors = new Dictionary<CounterColor, Color>
		{
			{ CounterColor.White, Color.FromHex("cccccc") },
			{ CounterColor.Blue, Color.FromHex("0072bc") },
			{ CounterColor.Green, Color.FromHex("00cc22") },
			{ CounterColor.Orange, Color.FromHex("f26522") },
			{ CounterColor.Red, Color.FromHex("e20026") },//Color.FromHex("ed1c24") },
			{ CounterColor.Magenta, Color.FromHex("92278f") },
			{ CounterColor.Yellow, Color.FromHex("fff200") }
		};

		public static IDictionary<CounterSize, double> CounterFontSizes = new Dictionary<CounterSize, double>
		{
			{ CounterSize.XXL, 80},
			{ CounterSize.XL, 60},
			{ CounterSize.L, 45},
			{ CounterSize.M, 35},
			{ CounterSize.S, 25},
			{ CounterSize.XS, 18},
			{ CounterSize.XXS, 10}
		};
	}
}

