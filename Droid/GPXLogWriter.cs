using System;
using System.IO;
using Xamarin.Forms;
using MKCoolsoft.GPXLib;
using GPX = MKCoolsoft.GPXLib.GPXLib;
using System.Xml.Serialization;
using System.Globalization;
using Polenter.Serialization;

namespace LegendDrive
{
	public class GPXLogWriter
	{
		GPX gpx;
		DateTime trackStartTime;

		SharpSerializer serializer;

		static GPXLogWriter _instance;
		public static GPXLogWriter Instance
		{
			get
			{
				if (_instance == null)
				{
					_instance = new GPXLogWriter();
				}
				return _instance;
			}
		}

		public GPXLogWriter()
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

		void Process_Start(LocationData loc)
		{
			gpx = CreateGPX(loc);
		}

		void Process_Back(LocationData loc)
		{
			if (gpx == null)
			{
				gpx = CreateGPX(loc);
			}
			gpx.WptList.Add(CreateWpt("Back", loc));
		}

		void Process_Turn(LocationData loc)
		{
			if (gpx == null)
			{
				gpx = CreateGPX(loc);
			}
			
			gpx.WptList.Add(CreateWpt("Turn", loc));
		}

		void Process_Finish(LocationData loc)
		{
			lock(this)
			{
				if (gpx == null) return;
				gpx.WptList.Add(CreateWpt("Finish", loc));
				var fileName = trackStartTime.ToString("yyyy-MM-dd-HH-mm", CultureInfo.InvariantCulture);


				var sdCardPath = Android.OS.Environment.ExternalStorageDirectory.Path;
				var folderName = System.IO.Path.Combine(sdCardPath, "LegendDrive/Tracks");
			
				//var folderName = Environment.GetFolderPath(Environment.SpecialFolder.CommonDocuments); //"/LegendDrive/Tracks"; 


				var fullFileName = $"{folderName}/gpx_{fileName}.xml";

				try
				{
					var directory = new DirectoryInfo(folderName);
					if (!directory.Exists)
					{
						directory.Create();
					}

					gpx.SaveToFile(fullFileName);
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
			if (gpx == null)
			{
				gpx = CreateGPX(loc);
			}
			gpx.AddTrackPoint("default", 1, CreateWpt("z", loc));
		}

		private GPX CreateGPX(LocationData loc)
		{
			trackStartTime = DateTime.Now;
			gpx = new GPX();
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
	}
}

