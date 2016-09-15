using System;
using System.IO;
using Xamarin.Forms;
using MKCoolsoft.GPXLib;
using GPX = MKCoolsoft.GPXLib.GPXLib;
using System.Xml.Serialization;
using System.Globalization;
using Polenter.Serialization;
using LegendDrive.Model;

namespace LegendDrive
{
	public class GPSLoggingService
	{
		GPX gpx;

		SharpSerializer serializer;

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
			MessagingCenter.Subscribe<LocationData>(this, "raceEvent_Start", (loc) => Process_Start(loc));
			MessagingCenter.Subscribe<LocationData>(this, "raceEvent_Finish", (loc) => Process_Finish(loc));
			MessagingCenter.Subscribe<LocationData>(this, "raceEvent_Turn", (loc) => Process_Turn(loc));
			MessagingCenter.Subscribe<LocationData>(this, "raceEvent_Back", (loc) => Process_Back(loc));
			MessagingCenter.Subscribe<LocationData>(this, "raceEvent_NewLocation", (loc) => Process_NewLocation(loc));
			serializer = new SharpSerializer();
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

		void Process_Start(LocationData loc)
		{
			GetGpx(loc);
		}

		void Process_Back(LocationData loc)
		{
			GetGpx(loc).WptList.Add(CreateWpt("Back", loc));
		}

		void Process_Turn(LocationData loc)
		{
			GetGpx(loc).WptList.Add(CreateWpt("Turn", loc));
		}

		void Process_Finish(LocationData loc)
		{
			lock(this)
			{
				if (gpx == null) return;
				gpx.WptList.Add(CreateWpt("Finish", loc));
				var fileName = gpx.Metadata.Time.ToString("yyyy-MM-dd-HH-mm-ss", CultureInfo.InvariantCulture);
				var sdCardPath = Android.OS.Environment.ExternalStorageDirectory.Path;
				var folderName = System.IO.Path.Combine(sdCardPath, "LegendDrive/Tracks");
				var fullFileName = $"{folderName}/gpx_{fileName}.xml";

				try
				{
					var directory = new DirectoryInfo(folderName);
					if (!directory.Exists)
					{
						directory.Create();
					}

					using (var file = File.Create(fullFileName))
					{
						//var xmlSerializer = new XmlSerializer(typeof(GPX));
						//xmlSerializer.Serialize(file, gpx);
						//gpx.SaveToFile(fullFileName);
						serializer.Serialize(gpx, file);
					}
				}
				catch (Exception ex)
				{
					var s = ex.Message;
				}
			}
			gpx = null;
		}

		void Process_NewLocation(LocationData loc)
		{
			GetGpx(loc).AddTrackPoint("default", 1, CreateWpt("z", loc));
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
	}
}

