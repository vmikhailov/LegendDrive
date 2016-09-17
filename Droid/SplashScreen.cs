using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Support.V7.App;
using Android.Util;

namespace LegendDrive.Droid
{
	[Activity(Theme = "@style/MyTheme", MainLauncher = true, NoHistory = true)]
	public class SplashActivity : AppCompatActivity
	{
		static readonly string TAG = "X:" + typeof(SplashActivity).Name;

		protected override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);
			SetContentView(Resource.Layout.Splash2);
			Log.Debug(TAG, "SplashActivity.OnCreate");
		}

		protected override void OnResume()
		{
			base.OnResume();


			var startupWork = new Task(() => Log.Debug(TAG, "*"));

			startupWork.ContinueWith(t =>
			{
				var main = new Intent(Application.Context, typeof(MainActivity));
				main.AddFlags(ActivityFlags.NoAnimation);
				StartActivity(main);
			}, TaskScheduler.FromCurrentSynchronizationContext());

			startupWork.Start();
		}
	}
}
