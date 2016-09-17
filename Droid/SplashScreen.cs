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

		public override void OnCreate(Bundle savedInstanceState, PersistableBundle persistentState)
		{
			base.OnCreate(savedInstanceState, persistentState);

			Log.Debug(TAG, "SplashActivity.OnCreate");
		}

		protected override void OnResume()
		{
			base.OnResume();
			SetContentView(Resource.Layout.Splash2);

			var startupWork = new Task(() => Log.Debug(TAG, "*"));
			startupWork.ContinueWith(t => StartActivity(new Intent(Application.Context, typeof(MainActivity))), 
			                         TaskScheduler.FromCurrentSynchronizationContext());

			startupWork.Start();
		}
	}
}
