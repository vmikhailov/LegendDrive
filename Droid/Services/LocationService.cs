using System;
using Android.Locations;
using Android.OS;
using Android.Runtime;
using LegendDrive.Messaging;
using LegendDrive.Model;
using Xamarin.Forms;

namespace LegendDrive.Droid.Services
{
	public class LocationService: Java.Lang.Object, ILocationListener, GpsStatus.IListener
	{
		LocationManager _locationManager;
		string _providerName;
		int minDistance = 0;

		public LocationService(LocationManager manager)
		{
			_locationManager = manager;
			MessagingHub.Subscribe<GlobalCommand>(this, QueueType.Global, (cmd) => ProcessCommand(cmd));
		}

		public void Init()
		{
			var criteriaForLocationService = new Criteria
			{
				Accuracy = Accuracy.Fine,
				HorizontalAccuracy = Accuracy.Fine,
				VerticalAccuracy = Accuracy.Fine,
				PowerRequirement = Power.NoRequirement,
				SpeedRequired = true,
				SpeedAccuracy = Accuracy.Fine
			};

			_providerName = _locationManager.GetBestProvider(criteriaForLocationService, false);
			if (_providerName != null)
			{
				_locationManager.RequestLocationUpdates(_providerName, 0, minDistance, this);
				var lastLocation = _locationManager.GetLastKnownLocation(_providerName);
				OnLocationChanged(lastLocation);
			}
			_locationManager.AddGpsStatusListener(this);
		}

		void ProcessCommand(GlobalCommand cmd)
		{
			if (cmd.Code == GlobalCommandCodes.GPSReset)
			{
				Init();
				_locationManager.RequestLocationUpdates(_providerName, 0, minDistance, this);
			}
		}

		public void OnResume()
		{
			_locationManager.RequestLocationUpdates(_providerName, 0, minDistance, this);
		}

		public void OnPause()
		{
			_locationManager.RemoveUpdates(this);
		}


		public void OnLocationChanged(Location location)
		{
			if (location != null)
			{
				MessagingHub.Send(QueueType.Location, LocationData.CreateFrom(location));
			}
		}

		public void OnProviderDisabled(string provider)
		{
			MessagingHub.Send(QueueType.Location, LocationData.Offline);
		}

		public void OnProviderEnabled(string provider)
		{
			if (_providerName == provider)
			{
				var location = _locationManager.GetLastKnownLocation(provider);
				if (location != null)
				{
					MessagingHub.Send(QueueType.Location, LocationData.CreateFrom(location));
				}
			}
		}

		public void OnStatusChanged(string provider, [GeneratedEnum] Availability status, Bundle extras)
		{
		}

		public void OnGpsStatusChanged([GeneratedEnum] GpsEvent e)
		{
		}
	}
}
