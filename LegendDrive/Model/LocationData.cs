using System;
using Android.Locations;

namespace LegendDrive.Model
{
	public class LocationData
	{
		public static LocationData Offline { get; } = new LocationData() { GpsOn = false };

		public bool GpsOn { get; set; } = true;

		public double Latitude { get; set; }

		public double Longitude { get; set; }

		public double Accuracy { get; set; }

		public double Speed { get; set; }

		public double SpeedKmh
		{
			get { return Speed * 3.6f; }
		}

		public DateTime Time { get; set; }

		public long Nanos { get; set; }

		public static double DistanceBetween(LocationData p1, LocationData p2)
		{
			var result = new float[5];
			Location.DistanceBetween(p1.Latitude, p1.Longitude, p2.Latitude, p2.Longitude, result);
			return result[0];
		}

		public LocationData RoundCoords()
		{
			return new LocationData()
			{
				Latitude = Math.Round(this.Latitude, 5),
				Longitude = Math.Round(this.Longitude, 5),
				Speed = this.Speed,
				Accuracy = this.Accuracy,
				Nanos = this.Nanos,
				Time = this.Time
			};
		}

		public double AdvancedDistanceTo(LocationData to)
		{
			var time = (Nanos - to.Nanos) / 1000000000.0d;
			var delta = (Speed + to.Speed) / 2 * time;
			return delta;
		}

		public double DistanceTo(LocationData to)
		{
			return DistanceBetween(this, to);
		}

		private static DateTime FromUnixTime(long unixTime)
		{
			var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
			return epoch.AddSeconds(unixTime/1000d);
		}

		public static LocationData CreateFrom(Location location)
		{
			if (location == null) return null;
			var loc = new LocationData()
			{
				Latitude = location.Latitude,
				Longitude = location.Longitude,
				Speed = location.Speed, 
				Accuracy = location.Accuracy,
				Time = FromUnixTime(location.Time),
				Nanos = location.ElapsedRealtimeNanos
			};
			return loc;
		}
	}
}

