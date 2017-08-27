using System;
using System.Linq;
using System.Threading;
using LegendDrive;
using LegendDrive.Counters.Interfaces;
using LegendDrive.Messaging;
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
			//model.Race.ParseAndAddNewSegments("441 35 271 30 348 15 210 20 410 13 760 30 870 35 860 40 1120 28 830 40 680 35 90 13 430 15 80 13 660 30 210 25 210 20");
			//model.Race.ParseAndAddNewSegments("210 8 5");
			//model.Race.ParseAndAddNewSegments("680 34 1540 30");

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
		double speed;
		private void Worker(object state)
		{
			var rspeed = state as IRaceCounter<double?>;
			if (rspeed == null) return;
			if (!model.Race.IsRunning) return;
			samples = ++samples % 60;


			var targetSpeed = rspeed?.Value;
			targetSpeed = targetSpeed > 80 ? 80 : targetSpeed;
			var delta = Math.Round(Math.Abs(targetSpeed.GetValueOrDefault() - speed),0);
			speed += targetSpeed > (speed + delta/2) ? delta/2 : (targetSpeed < (speed - delta/2) ? -delta/2: 0);
			//if (speed.HasValue)
			//{
			//	speed = speed > 80 ? 80 : speed;
			//	speed += r.Next(16) - 8;
			//}
			//else
			//{
			//	speed = r.Next(10) + 15;
			//}
			//if (samples > 30) speed = 5;
			if (samples > 50) speed = 0;
			//speed = 8;

			var k = LocationData.DistanceBetween(
				new LocationData() { Longitude = 50.0001, Latitude = 30.0001 },
				new LocationData() { Longitude = 50.0002, Latitude = 30.0002 });

			var loc = new LocationData()
			{
				Latitude = prevLoc.Latitude + 0.0001 / k * (speed/ 3.6),
				Longitude = prevLoc.Longitude + 0.0001 / k * (speed / 3.6),
				Speed = speed / 3.6,
				Time = DateTime.Now.ToUniversalTime()
			};

			prevLoc = loc;
			MessagingHub.Send(QueueType.Location, loc);

			tick += 1000;
		}

	}
}
