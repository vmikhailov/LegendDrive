﻿using System;
using Android.Locations;
using Android.OS;
using Android.Runtime;
using LegendDrive.Model;
using Xamarin.Forms;

namespace LegendDrive.Droid
{
	public class LocationService: Java.Lang.Object, ILocationListener, GpsStatus.IListener
	{
		LocationManager _locationManager;
		string _providerName;

		public LocationService(LocationManager manager)
		{
			_locationManager = manager;
			MessagingCenter.Subscribe<GlobalCommand>(this, "global", (cmd) => ProcessCommand(cmd));
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
				_locationManager.RequestLocationUpdates(_providerName, 0, 1, this);
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
				_locationManager.RequestLocationUpdates(_providerName, 0, 1, this);
			}
		}

		public void OnResume()
		{
			_locationManager.RequestLocationUpdates(_providerName, 0, 1, this);
		}

		public void OnPause()
		{
			_locationManager.RemoveUpdates(this);
		}


		public void OnLocationChanged(Location location)
		{
			if (location == null) return;
			MessagingCenter.Send(LocationData.CreateFrom(location), "raceEvent_NewLocation");
		}

		public void OnProviderDisabled(string provider)
		{
			MessagingCenter.Send(LocationData.Offline, "raceEvent_NewLocation");
		}

		public void OnProviderEnabled(string provider)
		{
			if (_providerName == provider)
			{
				var location = _locationManager.GetLastKnownLocation(provider);
				MessagingCenter.Send(LocationData.CreateFrom(location), "raceEvent_NewLocation");
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