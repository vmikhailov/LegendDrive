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
		 
		public App()
		{
			model = new GlobalModel();
			MainPage = new RaceMainPage(model);

			simulator = new Simulator(model);
			simulator.Start();
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

