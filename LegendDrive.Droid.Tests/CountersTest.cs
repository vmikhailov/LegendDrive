using NUnit.Framework;
using System;
using LegendDrive.Counters;

namespace LegendDrive.Droid.Tests
{
	[TestFixture()]
	public class CountersTests
	{

		[Test()]
		public void Test_AvgSpeedCounter_BasicCalculation_SmallBuffer()
		{
			var c = new AvgSpeedCounter();

			c.SetLocation(new LocationData() { Speed = 5, Time = new DateTime(0) });
			c.SetLocation(new LocationData() { Speed = 10, Time = new DateTime(1000) });
			c.SetLocation(new LocationData() { Speed = 15, Time = new DateTime(2000) });
			c.SetLocation(new LocationData() { Speed = 15, Time = new DateTime(3000) });
			c.SetLocation(new LocationData() { Speed = 10, Time = new DateTime(4000) });
			c.SetLocation(new LocationData() { Speed = 5, Time = new DateTime(5000) });

			Assert_AreEqual((double)c.Value, 10);
		}


		[Test()]
		public void Test_AvgSpeedCounter_BasicCalculation_BigBuffer()
		{
			var c = new AvgSpeedCounter();

			for (int i = 0; i < 100; i++)
			{
				c.SetLocation(new LocationData() { Speed = i, Time = new DateTime(i*1000) });
			}
			//should take only last 30

			Assert.IsTrue(((double)c.Value).IsEqual(84f));

			var s = 84f * 31;
			for (int i = 0; i < 30; i++)
			{
				s += (100 - i) - (69 + i);
				c.SetLocation(new LocationData() { Speed = 100-i, Time = new DateTime((100+i) * 1000) });

				Assert.IsTrue(((double)c.Value).IsEqual(s / 31));
			}
		}

		public static void Assert_AreEqual(double v1, double v2)
		{
			
		}

	}

	public static class DoubleExtensions
	{
		public static bool IsEqual(this double v1, double v2)
		{
			return Math.Abs(v1 - v2) < 0.000001;
		}
	}
}

