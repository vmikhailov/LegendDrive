using System;
using System.IO;
using System.Threading;
using LegendDrive.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xamarin.Forms;

namespace LegendDrive
{
	public partial class App : Application
	{
		GlobalModel model;
		string stateFile = "LegendDriveState.json";
		string stateFileFullName;
		Simulator simulator; 
		Timer _timer;


		public App()
		{
			stateFileFullName = Path.Combine(System.Environment.GetFolderPath(Environment.SpecialFolder.Personal), stateFile);

			model = new GlobalModel();
			MainPage = new MainPage(model);
			simulator = new Simulator(model);
			simulator.Start();
			_timer = new Timer(x => SaveState(), this, 15000, 15000); 
		}

		void DeleteState()
		{
			var f = new FileInfo(stateFileFullName);
			if (f.Exists) f.Delete();
		}

		protected override void OnStart()
		{
			// Handle when your app starts
			LoadState();
		}

		
		protected override void OnSleep()
		{
			SaveState();
		}

		protected override void OnResume()
		{
			// Handle when your app resumes
		}

		private void LoadState()
		{
			if (new FileInfo(stateFileFullName).Exists)
			{
				try
				{
					var strState = File.ReadAllText(stateFileFullName);
					var objState = JObject.Parse(strState);
					model.LoadState(objState);
				}
				catch
				{
					File.Delete(stateFileFullName);
				}
			}
		}

		private void SaveState()
		{
			try
			{
				var state = model.GetState();
				File.WriteAllText(stateFileFullName, state.ToString());
			}
			catch
			{
			}
		}
	}
}

