
using Android.App;
using Android.Content.PM;
using Android.Views;
using Android.OS;
using Acr.UserDialogs;
using Xamarin.Forms;
using Android.Locations;
using Android.Runtime;
using System;

namespace LegendDrive.Droid
{
	[Activity(Label = "LegendDrive.Droid", Icon = "@drawable/icon", 
	          //Theme = "@style/DarkTheme",
	          Theme = "@style/MyTheme",
	          //Theme = "@android:style/Theme.Material",
	          MainLauncher = true, 
	          ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
	public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity, ILocationListener, GpsStatus.IListener
	{
		LocationManager _locationManager;
		string _providerName;
		//GPXLogWriter _logLogWriter

		protected override void OnCreate(Bundle bundle)
		{
			TabLayoutResource = Resource.Layout.Tabbar;
			ToolbarResource = Resource.Layout.Toolbar;
			RequestWindowFeature(WindowFeatures.NoTitle);

			base.OnCreate(bundle);

			global::Xamarin.Forms.Forms.Init(this, bundle);

			UserDialogs.Init(() => this);
			//InitializeLocationManager();
			GPXLogWriter.Instance.Init();

			LoadApplication(new App());

			MessagingCenter.Subscribe<GlobalCommand>(this, "ask", (cmd) => ProcessCommand(cmd));
			MessagingCenter.Subscribe<GlobalCommand>(this, "click", (cmd) => ProcessCommand(cmd));
		}

		//public override void OnConfigurationChanged(Android.Content.Res.Configuration newConfig)
		//{
		//	base.OnConfigurationChanged(newConfig);
		//}

		protected override void OnResume()
		{
			base.OnResume();
			if(_locationManager!=null)
				_locationManager.RequestLocationUpdates(_providerName, 0, 0, this);
		}

		protected override void OnPause()
		{
			base.OnPause();
			if (_locationManager != null)
			_locationManager.RemoveUpdates(this);
		}

		void InitializeLocationManager()
		{
			_locationManager = (LocationManager)GetSystemService(LocationService);
			var criteriaForLocationService = new Criteria
			{
				Accuracy = Accuracy.Fine,
				HorizontalAccuracy = Accuracy.Fine,
				VerticalAccuracy = Accuracy.Fine,
				PowerRequirement = Power.NoRequirement,
				SpeedRequired = true,
				SpeedAccuracy = Accuracy.Fine
			};

			//var allProviders = _locationManager.GetProviders(criteriaForLocationService, true);
			_providerName = _locationManager.GetBestProvider(criteriaForLocationService, false);
			//var acceptableLocationProviders = _locationManager.GetProviders(criteriaForLocationService, true);
			if (_providerName != null)
			{
				var lastLocation = _locationManager.GetLastKnownLocation(_providerName);
				OnLocationChanged(lastLocation);
			}
			_locationManager.AddGpsStatusListener(this);
		}

		public override void OnWindowFocusChanged(bool hasFocus)
		{
			base.OnWindowFocusChanged(hasFocus);
			//if (CurrentFocus != null)
			//{ 
			//	CurrentFocus.SystemUiVisibility = 
			//		(StatusBarVisibility)(SystemUiFlags.ImmersiveSticky | SystemUiFlags.HideNavigation | SystemUiFlags.LayoutFullscreen  );
			//}
		}

		private async void ProcessCommand(GlobalCommand cmd)
		{
			if (cmd.Code == GlobalCommandCodes.AskConfirmation)
			{
				var dlg = UserDialogs.Instance;
				var answer = await dlg.ConfirmAsync(new ConfirmConfig()
				{
					Message = cmd.Message + " " + cmd.GetHashCode(),
					CancelText = "Cancel",
					OkText = "Ok",
					Title = "Alert"
				});
				//var answer = await DisplayAlert("Alert", cmd.Message, "Yes", "No");
				if (answer)
				{
					MessagingCenter.Send(GlobalCommand.ReplyCofirmation(cmd.CommandToConfirm), "confirmed");
				}
				else
				{
					MessagingCenter.Send(GlobalCommand.ReplyCofirmation(cmd.CommandToConfirm), "canceled");
				}
			}
			if (cmd.Code == GlobalCommandCodes.GPSReset)
			{
				if (_locationManager != null)
					_locationManager.RemoveUpdates(this);
				InitializeLocationManager();
				_locationManager.RequestLocationUpdates(_providerName, 0, 0, this);
			}
		}

		public void OnLocationChanged(Location location)
		{
			if (location == null) return;
			var locationData = LocationData.CreateFrom(location);

			MessagingCenter.Send(locationData, "raceEvent_NewLocation");
		}

		public void OnProviderDisabled(string provider)
		{
			//throw new NotImplementedException();
		}

		public void OnProviderEnabled(string provider)
		{
			//throw new NotImplementedException();
		}

		public void OnStatusChanged(string provider, [GeneratedEnum] Availability status, Bundle extras)
		{
			//throw new NotImplementedException();
		}

		public void OnGpsStatusChanged([GeneratedEnum] GpsEvent e)
		{
			switch (e)
			{
				case GpsEvent.FirstFix:
					break;

				case GpsEvent.SatelliteStatus:
					//var status = _locationManager.GetGpsStatus(null);
					break;

				case GpsEvent.Started:
					break;

				case GpsEvent.Stopped:
					break;
			}
			//throw new NotImplementedException();
		}
	}
}

