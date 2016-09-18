using System;
using System.IO;
using System.Threading;
using Android.Util;
using LegendDrive.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xamarin.Forms;

namespace LegendDrive
{
	public partial class App : Application
	{
		GlobalModel model;
		Simulator simulator; 
		Timer _timer;


		public App()
		{
			model = new GlobalModel();

			//var tabs = new TabbedPage();
			//tabs.Children.Add(new RaceMainPage(model) { Title = "Race" });

			//tabs.Children.Add(new SettingsPage { Title = "Settings"});
			//MainPage = tabs;
			MainPage = new RaceMainPage(model);

			simulator = new Simulator(model);
			simulator.Start();
			//_timer = new Timer(x => SaveState(), this, 15000, 15000); 
		}

		protected override void OnStart()
		{
			if (Properties.ContainsKey("appstate"))
			{
				var state = (string)Properties["appstate"];
				LoadAppState(state);
			}
		}
		
		protected override void OnSleep()
		{
			var state = GetAppState();
			Properties["appstate"] = state;
		}

		protected override void OnResume()
		{
		}

		public string GetAppState()
		{
			return model.GetState().ToString();
		}

		public void LoadAppState(string state)
		{
			if (state != null)
			{
				try
				{
					var objState = JObject.Parse(state);
					if (objState != null)
					{
						model.LoadState(objState);
					}
				}
				catch(Exception ex)
				{
					Log.Debug("state", ex.Message); 
				}
			}
		}
	}
}

