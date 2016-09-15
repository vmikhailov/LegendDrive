using System;
using System.Linq;
using System.Threading;
using LegendDrive;
using LegendDrive.Counters.Interfaces;
using Xamarin.Forms;

namespace LegendDrive.Model
{
	public class Simulator
	{
		GlobalModel model;
		public Simulator(GlobalModel model)
		{
			this.model = model;
		}

		public void Start()
		{
			model.Race.ParseAndAddNewSegments("441 35 271 30 348 15 210 20 410 13 760 30 870 35 860 40 1120 28 830 40 680 35 90 13 430 15 80 13 660 30 210 25 210 20");
			model.Race.ParseAndAddNewSegments("210 8 5");
			model.Race.ParseAndAddNewSegments("680 34 1540 30");

			prevLoc = new LocationData()
			{
				Latitude = 50 + r.Next(200) / 1000000.0,
				Longitude = 30 + r.Next(200) / 1000000.0,
			};

			var rspeed = model.CountersGroup.All
			                  .OfType<IRaceCounter<double?>>()
			                  .Where(x => x.Name == "Recommended speed value")
			                  .FirstOrDefault();

			new Timer(Worker, rspeed, 1000, 1000);
		}


		private Random r = new Random();
		private long tick = 0;
		private LocationData prevLoc;
		int samples = 0;

		private void Worker(object state)
		{
			var rspeed = state as IRaceCounter<double?>;
			if (rspeed == null) return;
			if (!model.Race.IsRunning) return;
			samples = ++samples % 60;


			var speed = rspeed?.TypedValue;

			if (speed.HasValue)
			{
				speed = speed > 80 ? 80 : speed;
				speed += r.Next(16) - 8;
			}
			else
			{
				speed = r.Next(10) + 15;
			}
			if (samples > 30) speed = 5;
			if (samples > 50) return;
			//speed = 20;

			var k = LocationData.DistanceBetween(
				new LocationData() { Longitude = 50.0001, Latitude = 30.0001 },
				new LocationData() { Longitude = 50.0002, Latitude = 30.0002 });

			var loc = new LocationData()
			{
				Latitude = prevLoc.Latitude + 0.0001 / k * (speed.Value / 3.6),
				Longitude = prevLoc.Longitude + 0.0001 / k * (speed.Value / 3.6),
				Speed = speed.Value / 3.6,
				Time = DateTime.Now
			};

			prevLoc = loc;
			MessagingCenter.Send(loc, "raceEvent_NewLocation");

			tick += 1000;
		}

	}
}
