using Xamarin.Forms;

namespace LegendDrive
{
	public partial class App : Application
	{
		public App()
		{
			var model = new GlobalModel();
			MainPage = new MainPage(model);
			model.StartSimulator();
		}

		protected override void OnStart()
		{
			// Handle when your app starts
		}

		protected override void OnSleep()
		{
			// Handle when your app sleeps
		}

		protected override void OnResume()
		{
			// Handle when your app resumes
		}
	}
}

