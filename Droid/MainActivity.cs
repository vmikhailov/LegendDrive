using Android.App;
using Android.Content.PM;
using Android.Views;
using Android.OS;
using Android.Locations;
using LegendDrive.Droid.Services;

namespace LegendDrive.Droid
{
	[Activity(Label = "Legend Drive", Icon = "@drawable/icon", 
	          Theme = "@style/MyTheme",
	          MainLauncher = false, 
	          ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
	public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsApplicationActivity
	{
		LocationService _locationService;
		VibrationService _vibrationService;

		protected override void OnCreate(Bundle bundle)
		{
			//SetPersistent(true);
			//TabLayoutResource = Resource.Layout.Tabbar;
			//ToolbarResource = Resource.Layout.Toolbar;
			//RequestWindowFeature(WindowFeatures.NoTitle);
			RequestWindowFeature(WindowFeatures.ActionBar);

			base.OnCreate(bundle);
			global::Xamarin.Forms.Forms.Init(this, bundle);
			InitServices();
			LoadApplication(new App());
		}

		protected void InitServices()
		{
			var locationManager = (LocationManager)GetSystemService(LocationService);
			_locationService = new LocationService(locationManager);
			_locationService.Init();

			var vibrator = (Vibrator)GetSystemService(VibratorService);
			_vibrationService = new VibrationService(vibrator);
			_vibrationService.Init();

			GPSLoggingService.Instance.Init();
		}

		protected override void OnResume()
		{
			base.OnResume();
			_locationService.OnResume();
		}

		protected override void OnPause()
		{
			base.OnPause();
			_locationService.OnPause();
		}
	
		public override void OnWindowFocusChanged(bool hasFocus)
		{
			base.OnWindowFocusChanged(hasFocus);
			if (CurrentFocus != null)
			{ 
				CurrentFocus.SystemUiVisibility = 
					(StatusBarVisibility)(SystemUiFlags.ImmersiveSticky | SystemUiFlags.HideNavigation);
			}
		}
	}
}

