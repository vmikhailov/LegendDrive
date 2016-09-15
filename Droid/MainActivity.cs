
using Android.App;
using Android.Content.PM;
using Android.Views;
using Android.OS;
using Acr.UserDialogs;
using Xamarin.Forms;
using Android.Locations;
using Android.Runtime;
using System;
using System.Threading.Tasks;
using LegendDrive.Model;

namespace LegendDrive.Droid
{
	[Activity(Label = "LegendDrive.Droid", Icon = "@drawable/icon", 
	          //Theme = "@style/DarkTheme",
	          Theme = "@style/MyTheme",
	          //Theme = "@android:style/Theme.Material",
	          MainLauncher = true, 
	          ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
	public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
	{
		LocationService _locationService;

		protected override void OnCreate(Bundle bundle)
		{
			TabLayoutResource = Resource.Layout.Tabbar;
			ToolbarResource = Resource.Layout.Toolbar;
			RequestWindowFeature(WindowFeatures.NoTitle);

			base.OnCreate(bundle);

			global::Xamarin.Forms.Forms.Init(this, bundle);

			UserDialogs.Init(() => this);

			var _locationManager = (LocationManager)GetSystemService(LocationService);
			_locationService = new Droid.LocationService(_locationManager);
			_locationService.Init();
			GPSLoggingService.Instance.Init();
			LoadApplication(new App());

			MessagingCenter.Subscribe<GlobalCommand>(this, "ask", (cmd) => ProcessCommand(cmd));
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
			//if (CurrentFocus != null)
			//{ 
			//	CurrentFocus.SystemUiVisibility = 
			//		(StatusBarVisibility)(SystemUiFlags.ImmersiveSticky | SystemUiFlags.HideNavigation | SystemUiFlags.LayoutFullscreen  );
			//}
		}

		private async Task ProcessCommand(GlobalCommand cmd)
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
		}
	}
}

