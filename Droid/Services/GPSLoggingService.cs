using System;
using System.IO;
using Xamarin.Forms;
using System.Xml.Serialization;
using System.Globalization;
using LegendDrive.Model;
using LegendDrive.Library.GPXLib;
using LegendDrive.Messaging;

namespace LegendDrive.Droid.Services
{
	public class GPSLoggingService
	{
		GPX gpx;

		XmlSerializer serializer;

		static GPSLoggingService _instance;
		public static GPSLoggingService Instance
		{
			get
			{
				if (_instance == null)
				{
					_instance = new GPSLoggingService();
				}
				return _instance;
			}
		}

		public GPSLoggingService()
		{
		}

		public void Init()
		{
			MessagingHub.Subscribe<LocationData>(this, QueueType.Location, (loc) => ProcessNewLocation(loc));
			MessagingHub.Subscribe<RaceEvent>(this, QueueType.Race, (evt) => ProcessRaceEvent(evt));
			serializer = new XmlSerializer(typeof(GPX));
		}

		protected GPX GetGpx(LocationData loc)
		{
			if (gpx != null) return gpx;
			gpx = new GPX()
			{
				Metadata = new Metadata()
				{
					Time = DateTime.Now,
					TimeSpecified = true
				}
			};
			gpx.WptList.Add(CreateWpt("Start", loc));
			return gpx;
		}

		private Wpt CreateWpt(string name, LocationData loc)
		{
			return new Wpt((decimal)loc.Latitude, (decimal)loc.Longitude)
			{
				Name = name,
				Time = loc.Time,
				Cmt = $"Speed {loc.SpeedKmh}"
			};
		}

		void ProcessRaceEvent(RaceEvent evt)
		{
			GetGpx(evt.Location).AddWayPoint(CreateWpt(evt.Type.ToString(), evt.Location));
			if (evt.Type == RaceEventTypes.Finish)
			{
				SaveGpx();
			}
		}

		void ProcessNewLocation(LocationData loc)
		{
			GetGpx(loc).AddTrackPoint("default", 1, CreateWpt("z", loc));
		}

		void SaveGpx()
		{
			lock(this)
			{
				var fileName = gpx.Metadata.Time.ToString("yyyy-MM-dd-HH-mm-ss", CultureInfo.InvariantCulture);
				var sdCardPath = Android.OS.Environment.ExternalStorageDirectory.Path;
				var folderName = System.IO.Path.Combine(sdCardPath, "LegendDrive/Tracks");
				var fullFileName = $"{folderName}/gpx_{fileName}.gpx";
				var fullFileName2 = $"{folderName}/gpx_{fileName}.xml";
				try
				{
					var directory = new DirectoryInfo(folderName);
					if (!directory.Exists)
					{
						directory.Create();
					}

					using (var file = File.Create(fullFileName))
					{
						serializer.Serialize(file, gpx);
					}

					using (var file = File.Create(fullFileName2))
					{
						serializer.Serialize(file, gpx);
					}
				}
				catch (Exception ex)
				{
					var s = ex.Message;
				}
				gpx = null;
			}
		}


	}
}

