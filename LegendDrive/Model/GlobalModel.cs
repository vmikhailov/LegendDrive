using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Xamarin.Forms;

namespace LegendDrive
{
	public class GlobalModel : BaseBindingObject
	{
		public NumpadModel Numpad { get; set; }

		public Race Race { get; set; }

		public IDictionary<string, ObservableRangeCollection<IRaceCounter>> Counters { get; set; }

		private List<IRaceCounter> HiddenCounters { get; set; }
		private IEnumerable<IRaceCounter> RefreshAtTurn { get; set; }
		private IEnumerable<IRaceCounter> AllCounters { get; set; }


		private static int gpsSymbol;
		public GlobalModel()
		{
			Numpad = new NumpadModel();
			Race = new Race();
			Numpad.NewDataTextEntered += Race.ParseAndAddNewSegments;
			InitCounters();

			MessagingCenter.Subscribe<GlobalCommand>(this, "click", (cmd) => ProcessClickCommand(cmd));
			MessagingCenter.Subscribe<GlobalCommand>(this, "confirmed", (cmd) => ProcessConfirmedCommand(cmd));
			MessagingCenter.Subscribe<GlobalCommand>(this, "canceled", (cmd) => ProcessCanceledCommand(cmd));
			MessagingCenter.Subscribe<LocationData>(this, "raceEvent_NewLocation", (loc) => ProcessNewLocation(loc));
		}

		private void InitCounters()
		{
			Counters = new Dictionary<string, ObservableRangeCollection<IRaceCounter>>();

			//TIME counters (8)
			var globalCurrentTime = new CurrentTimeCounter();

			var raceTimer = new TimerCounter("Race timer");

			var raceDuration = new TriggeredFuncCounter<Race, TimeSpan?>("Race duration", @"{0:hh\:mm\:ss}").With(x =>
			{
				x.BindTo(Race, y => y.DurationOfRace);
				x.AddTrigger(".", Race);
			});

			var raceStartTime = new TriggeredFuncCounter<Race, DateTime?>("Race start time", @"{0:HH\:mm\:ss}").With(x =>
			{
				x.BindTo(Race, y => y.StartTime);
				x.AddTrigger(".", Race);
			});

			var raceEstimatedFinishTime = new TriggeredFuncCounter<Race, DateTime?>("Estimated finish time", @"{0:HH\:mm\:ss}").With(x =>
			{
				x.BindTo(Race, y => y.EstimatedFinishTime);
				x.AddTrigger(".", Race);
			});

			var segmentTimer = new TriggeredFuncCounter<Race, TimeSpan?>("Timer at the end of segment", @"{0:hh\:mm\:ss}").With(x =>
			{
				x.BindTo(Race, y => y.TimeOffsetAtTheEndOfCurrentSegment);
				x.AddTrigger("TimeOffsetAtTheEndOfCurrentSegment", Race);
			});

			var timeToTurn = new RemainingTimeCounter("Time to turn").With(x =>
			{
				x.SetBase(raceTimer);
				x.SetTarget(segmentTimer);
				x.Size = CounterSize.L;
			});

			var raceRemainingTime = new RemainingTimeCounter("Remaining race time").With(x =>
			{
				x.SetBase(raceTimer);
				x.SetTarget(raceDuration);
			});

			var segmentEndTime = new TriggeredFuncCounter<Race, DateTime?>("Time at end of segment", @"{0:HH\:mm\:ss}").With(x =>
			{
				x.BindTo(Race, y => y.TimeAtTheEndOfCurrentSegment);
				x.AddTrigger("TimeAtTheEndOfCurrentSegment", Race);
			});

			//GPS Counters (1)
			var gpsCoords = new TriggeredFuncCounter<GlobalModel, string>("GPS", "{0}").With(x =>
			{
				x.BindTo(this, y =>
				{
					if (y.LastLocaton == null) return "N/A";
					return string.Format("La = {0:F5}\nLo = {1:F5}\nAc = {2:F3} {3:F2}",
										 y.LastLocaton.Latitude,
										 y.LastLocaton.Longitude,
										 y.LastLocaton.Accuracy,
										 @"-\|/-\|/"[gpsSymbol++ % 8]);
				});
				x.AddTrigger("LastLocation", this);
				x.Size = CounterSize.S;
				x.AfterNewValue = y =>
				{
					var loc = y.BindingContext.LastLocaton;
					var gpsage = loc.DeltaT.TotalSeconds;
					y.SetImportant(gpsage > 3 || loc.Accuracy > 50);
					y.SetCritical(gpsage > 5 || loc.Accuracy > 100);
				};
			});

			//SEGMENT info (3)
			//var currentSegmentNo = new TriggeredFuncCounter<Race, int?>("Segment no", "{0}").With(x =>
			//{
			//	x.BindTo(Race, y => y.CurrentSegment?.No);
			//	x.AddTrigger("CurrentSegment", Race);
			//});

			//var currentSegmentSpeed = new TriggeredFuncCounter<Race, double?>("Speed at segment").With(x =>
			//{
			//	x.BindTo(Race, y => y.CurrentSegment?.Speed);
			//	x.AddTrigger("CurrentSegment", Race);
			//});

			var currentSegmentLength = new TriggeredFuncCounter<Race, double?>("Length of segment").With(x =>
			{
				x.BindTo(Race, y => y.CurrentSegment?.Distance);
				x.AddTrigger("CurrentSegment", Race);
			});


			//DISTANCE counters (4)
			var raceDistance = new DistanceCounter("Race distance");
			var raceDistance2 = new DistanceCounter2("S(round)");
			var raceDistance3 = new DistanceCounter3("S(by speed)");
			var segmentDistance = new DistanceCounter("Segment distance"); 

			var raceLength = new TriggeredFuncCounter<Race, double?>("Length of race").With(x =>
			{
				x.BindTo(Race, y => y.LengthOfRace);
				x.AddTrigger(".", Race);
			});

			var segmentDistanceToTurn = new RemainingDistanceCounter("Distance to turn").With(x =>
			{
				x.SetBase(segmentDistance);
				x.SetTarget(currentSegmentLength);
				x.Size = CounterSize.XXL;
			});

			var raceDistanceToFinish = new RemainingDistanceCounter("Distance to finish").With(x =>
			{
				x.SetBase(raceDistance);
				x.SetTarget(raceLength);
			});

			var racePassedDistance = new TriggeredFuncCounter<Race, double?>("Length of passed race").With(x =>
			{
				x.BindTo(Race, y =>
				{
					return y.Segments.TakeWhile(z => z.Passed).Sum(z => z.Distance)
						    + segmentDistance.TypedValue;
				});
				x.AddTrigger(".", Race);
				x.AddTrigger("Value", segmentDistance);
			});


			//SPEED (6)
			var raceAverageSpeed = new AvgSpeedKmhCounter("Avg race speed", 86400);
			var segmentAvgSpeed = new AvgSpeedKmhCounter("Avg segment speed", 86400);
			var segmentFiveSecondsSpeed = new AvgSpeedKmhCounter("Current speed", 5)
			{
				Size = CounterSize.XXL,
				Color = CounterColor.Green
			};

			var recommendedSpeed = new TriggeredFuncCounter<Race, int>("Recommended Speed Value", "{0}").With(x =>
			{
				x.BindTo(Race, y =>
				{
					if (y.IsRunning)
					{
						var timerFromStart = raceTimer.TypedValue;
						var timerAtEndOfSegment = y.TimeOffsetAtTheEndOfCurrentSegment;
						var timeLeft = timerAtEndOfSegment - timerFromStart;
						if (timeLeft?.TotalSeconds <= 1) return 1000;
						var distanceLeft = y.CurrentSegment?.Distance - segmentDistance?.TypedValue;
						var speed = distanceLeft / timeLeft?.TotalSeconds * 3.6;
						if (speed >= 90) return 1000;
						if (speed <= 5) return 0;
						return speed.HasValue ? (int)speed.Value : 0;
					}
					return 0;
				});
				x.AddTrigger(".", Race);
				x.AddTrigger("Value", raceTimer);
				x.AddTrigger("Value", segmentFiveSecondsSpeed);
			});

			var recommendedSpeedString = new TriggeredFuncCounter<Race, string>("Recommended speed", "{0}").With(x =>
			{
				x.BindTo(Race, y =>
				{
					if (!y.IsRunning) return "0";
					var speed = recommendedSpeed.TypedValue;
					if (speed == 1000) return "MAX";
					if (speed == 0) return "STOP";
					return speed.ToString();
				});
				x.AddTrigger("Value", recommendedSpeed);
				x.Size = CounterSize.XXL;
				x.SetImportant(true);
				x.AfterNewValue = y =>
				{
					var speed = recommendedSpeed.TypedValue;
					if (speed == 0 || speed == 1000)
					{
						x.SetCritical(true);
						return;
					}
					var avgSpeed = segmentFiveSecondsSpeed.TypedValue;
					var rel = avgSpeed > 0.5 ? Math.Abs(speed / avgSpeed) : 1000;
					x.SetImportant(rel > 1.5 || rel < 0.75);
					x.SetCritical(rel > 2 || rel < 1 / 2);
				};
			});

			//Mixed
			var lagLeadTime = new TriggeredFuncCounter<Race, TimeSpan>("LAG or LEAD time", @"{0:hh\:mm\:ss}").With(x =>
			{
				x.BindTo(Race, y =>
				{
					var speed = segmentFiveSecondsSpeed.TypedValue;
					var timerFromStart = raceTimer.TypedValue;
					var distanceLeft = y.CurrentSegment?.Distance - segmentDistance?.TypedValue;

					if(speed <= 0.5 || 
					   !timerFromStart.HasValue ||
	                   !distanceLeft.HasValue) return TimeSpan.FromDays(1);
				
					var timeToEndOfSeg = distanceLeft.Value / speed * 3.6; //relative time TO End Of Segment With Current Speed
					var time1 = timerFromStart.Value + TimeSpan.FromSeconds(timeToEndOfSeg); //time AT End Of Segment With Current Speed
					var time2 = y.TimeOffsetAtTheEndOfCurrentSegment;

					return time2 - time1;
				});
				x.AddTrigger("Value", recommendedSpeed);
				x.Size = CounterSize.L;
				x.SetImportant(false);
				x.SetCritical(false);
				x.AfterNewValue = y =>
				{
					var seconds = Math.Abs(y.TypedValue.TotalSeconds);
					x.SetImportant(seconds > 30);
					x.SetCritical(seconds > 60);
				};
			});

			//making groups
			var group0 = new ObservableRangeCollection<IRaceCounter>()
			{
				globalCurrentTime, 
				raceStartTime,
				raceEstimatedFinishTime, 
				raceTimer,
				raceRemainingTime

			};


			var group1 = new ObservableRangeCollection<IRaceCounter>()
			{
				gpsCoords,
				segmentDistanceToTurn,
				timeToTurn
			};

			var group2 = new ObservableRangeCollection<IRaceCounter>()
			{
				segmentFiveSecondsSpeed, 
				recommendedSpeedString,
				lagLeadTime
			};

			var group3 = new ObservableRangeCollection<IRaceCounter>()
			{
				raceLength,
				raceDuration,
				raceDistance, raceDistance2, raceDistance3,
				segmentDistance, raceAverageSpeed, segmentAvgSpeed,
				racePassedDistance, raceDistanceToFinish
			};


			Counters["Group0"] = group0;
			Counters["Group1"] = group1;
			Counters["Group2"] = group2;
			Counters["Group3"] = group3;

			HiddenCounters = new List<IRaceCounter>();
			HiddenCounters.AddRange(new IRaceCounter[]
			{
				//raceAverageSpeed,
				raceRemainingTime,
				recommendedSpeed, 
				raceLength,
				//currentSegmentNo,
				//currentSegmentSpeed,
				currentSegmentLength,
				//segmentAvgSpeed,
				segmentDistance,
				segmentEndTime,
				segmentTimer
			});

			RefreshAtTurn = new List<IRaceCounter>()
			{
				segmentTimer,
				segmentEndTime,
				segmentAvgSpeed,
				segmentDistance,
				segmentFiveSecondsSpeed
			};

			AllCounters = HiddenCounters
				.Union(Counters.Values.SelectMany(x => x))
				.Distinct()
				.ToList();

			foreach (var c in AllCounters)
			{
				c.Init();
			}
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

				case GlobalCommandCodes.ClearAll:
					MessagingCenter.Send(GlobalCommand.AskCofirmation(cmd, "Delete all data?"), "ask");
					break;
				case GlobalCommandCodes.GPSReset:
					ManageParams();
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
			foreach (var c in AllCounters)
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
			foreach (var c in AllCounters)
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
			foreach (var c in RefreshAtTurn)
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
			foreach (var c in RefreshAtTurn)
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

		public void ManageParams()
		{
		}

		public void ClearAll()
		{
			foreach (var c in AllCounters)
			{
				c.Reset();
			}
			Race.Turns.Clear();
			Race.Segments.Clear();
		}

		public LocationData LastLocaton
		{
			get; private set;
		}

		private void ProcessNewLocation(LocationData loc)
		{
			var prevLocTime = (LastLocaton != null) ? LastLocaton.Time : new DateTime(0);
			LastLocaton = loc;
			LastLocaton.DeltaT = LastLocaton.Time - prevLocTime;
			OnPropertyChanged("LastLocation");
			foreach (var c in AllCounters.OfType<ILocationProcessor>())
			{
				c.SetLocation(loc);
			};
		}

		public void StartSimulator()
		{
			//var numbers = Enumerable.Range(1, 200)
			//					.SelectMany(x => new[] { r.Next(100, 1000), r.Next(10, 40) })
			//					.Select(x => x.ToString());

			//Race.ParseAndAddNewSegments(string.Join(" ", numbers));
			Race.ParseAndAddNewSegments("441 35 271 30 348 15 210 20 410 13 760 30 870 35 860 40 1120 28 830 40 680 35 90 13 430 15 80 13 660 30 210 25 210 20");
			Race.ParseAndAddNewSegments("210 8 5");
			Race.ParseAndAddNewSegments("680 34 1540 30");

			prevLoc = new LocationData()
			 {
				 Latitude = 50 + r.Next(200) / 1000000.0,
				 Longitude = 30 + r.Next(200) / 1000000.0,
			 };

			new Timer(Simulator, this, 1000, 100);
		}


		private Random r = new Random();
		private long tick = 0;
		private LocationData prevLoc;

		private void Simulator(object state)
		{
			//var model = state as GlobalModel;
			if (!Race.IsRunning) return;

			var speed = Race.CurrentSegment?.Speed;
			if (speed.HasValue)
			{
				speed += r.Next(16) - 8;
			}
			else
			{
				speed = r.Next(10) + 15;
			}

			//speed = 20;

			var k = LocationData.DistanceBetween(
				new LocationData() { Longitude = 50.0001, Latitude = 30.0001 },
				new LocationData() { Longitude = 50.0002, Latitude = 30.0002 });

			var loc = new LocationData()
			{
				Latitude = prevLoc.Latitude + 0.0001 / k * (speed.Value / 3.6),
				Longitude = prevLoc.Longitude + 0.0001 / k * (speed.Value / 3.6),
				Speed = speed.Value / 3.6,
				Time = DateTime.Now
			};

			//loc.Speed = (double)speed;
			prevLoc = loc;
			MessagingCenter.Send(loc, "raceEvent_NewLocation");
		
			tick += 1000;
		}


	}
}

