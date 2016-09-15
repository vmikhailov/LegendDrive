using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using LegendDrive.Counters;
using LegendDrive.Counters.Interfaces;
using LegendDrive.Model;
using LegendDrive.Model.RaceModel;
using LegendDrive.Persistance;
using Newtonsoft.Json.Linq;
using Xamarin.Forms;

namespace LegendDrive.Model
{
	public class GlobalModel : BaseBindingObject
	{
		public NumpadModel Numpad { get; set; }

		public Race Race { get; set; }

		public CountersGroup CountersGroup { get; set; }

		public GlobalModel()
		{
			Numpad = new NumpadModel();
			Race = new Race();
			Numpad.NewDataTextEntered += Race.ParseAndAddNewSegments;

			CountersGroup = new CountersGroup(this);
			CountersGroup.Init();

			MessagingCenter.Subscribe<GlobalCommand>(this, "click", (cmd) => ProcessClickCommand(cmd));
			MessagingCenter.Subscribe<GlobalCommand>(this, "confirmed", (cmd) => ProcessConfirmedCommand(cmd));
			MessagingCenter.Subscribe<GlobalCommand>(this, "canceled", (cmd) => ProcessCanceledCommand(cmd));
			MessagingCenter.Subscribe<LocationData>(this, "raceEvent_NewLocation", (loc) => ProcessNewLocation(loc));
		}


		private void ProcessClickCommand(GlobalCommand cmd)
		{
			switch (cmd.Code)
			{
				case GlobalCommandCodes.StartFinish:
					if (Race.IsRunning)
					{
						MessagingCenter.Send(GlobalCommand.AskCofirmation(cmd, "Do you really want to finish the race?"), "ask");
					}
					else StartRace();
					break;
				case GlobalCommandCodes.ResetAll:
					if (Race.IsRunning)
					{
						MessagingCenter.Send(GlobalCommand.AskCofirmation(cmd, "Do you really want to reset after start?"), "ask");
					}
					break;
				case GlobalCommandCodes.ClearAll:
					MessagingCenter.Send(GlobalCommand.AskCofirmation(cmd, "Delete all data?"), "ask");
					break;
				case GlobalCommandCodes.GPSReset:
					MessagingCenter.Send(cmd, "global");
					break;
				case GlobalCommandCodes.Turn:
					MakeATurn();
					break;
				case GlobalCommandCodes.Back:
					GoBack();
					break;
				case GlobalCommandCodes.DelSegment:
					Race.DeleteLastSegment();
					break;
			}
		}

		private void ProcessConfirmedCommand(GlobalCommand cmd)
		{
			if (cmd.CommandToConfirm == null) return;

			switch (cmd.CommandToConfirm.Code)
			{
				case GlobalCommandCodes.StartFinish:
					if (Race.IsRunning)
					{
						FinishRace();
					}
					break;
				case GlobalCommandCodes.ResetAll:
					ResetAll();
					break;
				case GlobalCommandCodes.ClearAll:
					ClearAll();
					break;
			}
		}

		private void ProcessCanceledCommand(GlobalCommand cmd)
		{
			//do nothing
		}

		public void StartRace()
		{
			foreach (var c in CountersGroup.All)
			{
				c.Reset();
				c.Start();
			}
			Race.StartRace();
			if (LastLocaton != null)
			{
				MessagingCenter.Send(LastLocaton, "raceEvent_Start");
			}
		}

		public void FinishRace()
		{
			foreach (var c in CountersGroup.All)
			{
				c.Stop();
			}
			Race.FinishRace();
			if (LastLocaton != null)
			{
				MessagingCenter.Send(LastLocaton, "raceEvent_Finish");
			}
		}

		public void MakeATurn()
		{
			Race.AddTurn();
			foreach (var c in CountersGroup.Segment)
			{
				var h = c as ISupportHistory;
				if (h != null)
				{
					h.Push();
				}
				else
				{
					c.Reset();
				}
			}
			if (LastLocaton != null)
			{
				MessagingCenter.Send(LastLocaton, "raceEvent_Turn");
			}
		}

		public void GoBack()
		{
			foreach (var c in CountersGroup.Segment)
			{
				var h = c as ISupportHistory;
				if (h != null)
				{
					h.Pop();
				}
				else
				{
					c.Reset();
				}
			}
			Race.RemoveLastTurn();
			if (LastLocaton != null)
			{
				MessagingCenter.Send(LastLocaton, "raceEvent_Back");
			}
		}


		public void ResetAll()
		{
			foreach (var c in CountersGroup.All)
			{
				c.Reset();
			}
			Race.Turns.Clear();
			if (Race.IsRunning)
			{
				Race.AddTurn();
			}
		}


		public void ClearAll()
		{
			ResetAll();
			Race.Segments.Clear();
		}

		public LocationData LastLocaton
		{
			get; private set;
		}

		private void ProcessNewLocation(LocationData loc)
		{
			LastLocaton = loc;
			OnPropertyChanged("LastLocation");
			CountersGroup.ProcessNewLocation(loc);
			//SaveState();
		}

		public JObject GetState()
		{
			var obj = new JObject();
			var raceState = Race.GetState();
			var countersState = new List<JObject>();

			foreach (var c in CountersGroup.All.OfType<IRaceCounter>())
			{
				var p = c as ISupportStatePersistance;
				if (p == null) continue;
				var jobj = p.GetState();
				var id = c.GetType().FullName + "-" + c.Name;
				jobj.AddValue("id", id);
				countersState.Add(jobj);
			}

			obj.AddValue("race", raceState);
			obj.AddValue("counters", countersState);
			return obj;
		}

		public void LoadState(JObject obj)
		{
			var raceState = obj.GetValue<JObject>("race");
			var countersState = obj.GetValue<List<JObject>>("counters")
			                       .ToDictionary(x => x.GetValue<string>("id"), y => y);

			Race.LoadState(raceState);

			foreach (var c in CountersGroup.All.OfType<IRaceCounter>())
			{
				var p = c as ISupportStatePersistance;
				if (p == null) continue;
				var id = c.GetType().FullName + "-" + c.Name;
				if (countersState.ContainsKey(id))
				{
					p.LoadState(countersState[id]);
				}
			}

			if (Race.IsRunning)
			{
				foreach (var c in CountersGroup.All)
				{
					c.Start();
				}
			}
		}
	}
}

