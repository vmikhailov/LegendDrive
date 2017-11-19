using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using LegendDrive.Counters;
using LegendDrive.Counters.Interfaces;
using LegendDrive.Messaging;
using LegendDrive.Model;
using LegendDrive.Model.RaceModel;
using LegendDrive.Persistance;
using Newtonsoft.Json.Linq;
using Xamarin.Forms;

namespace LegendDrive.Model
{
	public class GlobalModel : BaseBindingObject<GlobalModel>
	{
		public NumpadModel Numpad { get; set; }

		public Race Race { get; set; }

		public CountersContainer CountersGroup { get; set; }

		public bool ShowDebugInfo { get; set;}

		public ITextToSpeech Speech { get; private set; }
		public bool UseBadLanguage { get; set; }

		public GlobalModel()
		{
			Numpad = new NumpadModel();
			Race = new Race();
			Numpad.NewDataTextEntered += Race.ParseAndAddNewSegments;


			CountersGroup = new CountersContainer(this);
			CountersGroup.Init();
			ShowDebugInfo = true;

			MessagingHub.Subscribe<GlobalCommand>(this, QueueType.Click, (cmd) => ProcessClickCommand(cmd));
			MessagingHub.Subscribe<GlobalCommand>(this, QueueType.Confirmed, (cmd) => ProcessConfirmedCommand(cmd));
			MessagingHub.Subscribe<GlobalCommand>(this, QueueType.Canceled, (cmd) => ProcessCanceledCommand(cmd));
			MessagingHub.Subscribe<LocationData>(this, QueueType.Location, (loc) => ProcessNewLocation(loc));
            InitSpeech();
            UseBadLanguage = false;
		}

        public void InitSpeech()
        {
            Speech = DependencyService.Get<ITextToSpeech>();
        }

        public void ResetSpeech()
        {
            Speech = null;
        }

		private void ProcessClickCommand(GlobalCommand cmd)
		{
			switch (cmd.Code)
			{
				case GlobalCommandCodes.StartFinish:
					if (Race.IsRunning)
					{
						MessagingHub.Send(QueueType.AskConfirmation, GlobalCommand.AskConfirmation(cmd, "Do you really want to finish the race?"));
					}
					else 
					{
						StartRace();
					}
					break;
				case GlobalCommandCodes.ResetAll:
					if (Race.IsRunning)
					{
						MessagingHub.Send(QueueType.AskConfirmation, GlobalCommand.AskConfirmation(cmd, "Do you really want to reset after start?"));
					}
					else
					{
						ResetAll();
					}
					break;
                case GlobalCommandCodes.ResetTime:
                    Race.ResetTimeOnCheckpoint();
                    break;
				case GlobalCommandCodes.ClearAll:
					MessagingHub.Send(QueueType.AskConfirmation, GlobalCommand.AskConfirmation(cmd, "Delete all data?"));
					break;
				case GlobalCommandCodes.GPSReset:
					MessagingHub.Send(QueueType.Global, cmd);
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
				MessagingHub.Send(QueueType.Race, new RaceEvent(LastLocaton, RaceEventTypes.Start));
				MessagingHub.Send(new VibrateCommand("11"));
			}
            Speech?.Speak("Поехали");
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
				MessagingHub.Send(QueueType.Race, new RaceEvent(LastLocaton, RaceEventTypes.Finish));
				MessagingHub.Send(new VibrateCommand("99"));
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
				MessagingHub.Send(QueueType.Race, new RaceEvent(LastLocaton, RaceEventTypes.Turn));
				MessagingHub.Send(new VibrateCommand("1"));
			}
			//Speech?.Speak("Поворот");
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
				MessagingHub.Send(QueueType.Race, new RaceEvent(LastLocaton, RaceEventTypes.Back));
				MessagingHub.Send(new VibrateCommand("3"));
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
            if (LastLocaton != null)
            {
                var distance = loc.DistanceTo(LastLocaton);
                if(distance > 1000)
                {
                    LastLocaton = loc;
                    return;
                }
            }
            LastLocaton = loc;
			RaisePropertyChanged(nameof(LastLocaton));
			CountersGroup.ProcessNewLocation(loc);
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



			//if (Race.IsRunning)
			//{
			//	foreach (var c in CountersGroup.All)
			//	{
			//		c.Start();
			//	}
			//}
		}
	}
}

