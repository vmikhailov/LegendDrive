using System;
using System.Collections.Generic;
using System.Linq;
using LegendDrive;
using LegendDrive.Model.RaceModel;
using LegendDrive.Counters;
using LegendDrive.Counters.Interfaces;
using LegendDrive.Messaging;

namespace LegendDrive.Model
{
	public class CountersContainer
	{
		List<CountersGroup> groups;
		public IEnumerable<CountersGroup> Groups => groups;

		List<IRaceCounter> HiddenCounters { get; set; }
		IEnumerable<IRaceCounter> SegmentCounters { get; set; }
		IEnumerable<IRaceCounter> AllCounters { get; set; }

		GlobalModel model;
		static int gpsSymbol;

		public CountersContainer(GlobalModel model)
		{
			this.model = model;
		}

		public void Init()
		{
			groups = new List<CountersGroup>();

			//TIME counters (8)
			var globalCurrentTime = new CurrentTimeCounter();

			var raceTimer = new TimerCounter("Race timer");

			var raceDuration = new TriggeredFuncCounter<Race, TimeSpan?>("Race duration", @"{0:hh\:mm\:ss}").With(x =>
			{
				x.BindTo(model.Race, y => y.DurationOfRace);
				x.AddTrigger(nameof(Race.DurationOfRace), model.Race);
			});

			var raceStartTime = new TriggeredFuncCounter<Race, DateTime?>("Race start time", @"{0:HH\:mm\:ss}").With(x =>
			{
				x.BindTo(model.Race, y => y.StartTime);
				x.AddTrigger(nameof(Race.StartTime), model.Race);
			});

			var raceEstimatedFinishTime = new TriggeredFuncCounter<Race, DateTime?>("Estimated finish time", @"{0:HH\:mm\:ss}").With(x =>
			{
				x.BindTo(model.Race, y => y.EstimatedFinishTime);
				x.AddTrigger(nameof(Race.EstimatedFinishTime), model.Race);
			});

			var segmentTimer = new TriggeredFuncCounter<Race, TimeSpan?>("Timer at the end of segment", @"{0:hh\:mm\:ss}").With(x =>
			{
				x.BindTo(model.Race, y => y.TimeOffsetAtTheEndOfCurrentSegment);
				x.AddTrigger(nameof(Race.TimeOffsetAtTheEndOfCurrentSegment), model.Race);
			});

			var timeToTurn = new RemainingTimeCounter("Time to turn").With(x =>
			{
				x.SetBase(raceTimer);
				x.SetTarget(segmentTimer);
				x.Size = CounterSize.XL;
				//x.OnValueChanged += y =>
				//{
				//	if (y.TypedValue.GetValue().TotalSeconds < 10)
				//	{
				//		MessagingHub.Send(new VibrateCommand("1"));
				//	}
				//};
			});

			var raceRemainingTime = new RemainingTimeCounter("Remaining race time").With(x =>
			{
				x.SetBase(raceTimer);
				x.SetTarget(raceDuration);
			});

			var segmentEndTime = new TriggeredFuncCounter<Race, DateTime?>("Time at end of segment", @"{0:HH\:mm\:ss}").With(x =>
			{
				x.BindTo(model.Race, y => y.TimeAtTheEndOfCurrentSegment);
				x.AddTrigger("TimeAtTheEndOfCurrentSegment", model.Race);
			});

			//GPS Counters (1)
			var gpsCoords = new TriggeredFuncCounter<GlobalModel, string>("GPS", "{0}").With(x =>
			{
				x.BindTo(model, y =>
				{
					if (y.LastLocaton == null) return "N/A";
					return string.Format("Lat = {0:F5}\nLon = {1:F5}\nAcc = {2:F3}\nSpd = {3:F1} {4}",
										 y.LastLocaton.Latitude,
										 y.LastLocaton.Longitude,
										 y.LastLocaton.Accuracy,
					                     y.LastLocaton.SpeedKmh,
										 @"-\|/-\|/"[gpsSymbol++ % 8]);
				});
				x.AddTrigger("LastLocation", model);
				x.AddTrigger("Value", globalCurrentTime);
				x.Size = CounterSize.XS;
				x.ValueChanged = y =>
				{
					var z = y as TriggeredFuncCounter<GlobalModel, string>;
					var loc = z.BindingContext?.LastLocaton;
					if (loc == null) return;
					if (loc.GpsOn)
					{
						if (!y.IsRunning)
						{
							y.Start();
						}
					}
					else
					{
						if (y.IsRunning)
						{
							y.Stop();
						}
					}
					var gpsage = (DateTime.Now.ToUniversalTime() - loc.Time).TotalSeconds;
					y.SetImportant(gpsage > 5 || loc.Accuracy > 50);
					y.SetCritical(gpsage > 30 || loc.Accuracy > 100);
				};
			});

			//SEGMENT info (3)
			var currentSegmentNo = new TriggeredFuncCounter<Race, int?>("Segment no", "{0}").With(x =>
			{
				x.BindTo(model.Race, y => y.CurrentSegment?.No);
				x.AddTrigger(nameof(Race.CurrentSegment), model.Race);
			});

			var currentSegmentSpeed = new TriggeredFuncCounter<Race, double?>("Speed at segment").With(x =>
			{
				x.BindTo(model.Race, y => y.CurrentSegment?.Speed);
				x.AddTrigger(nameof(Race.CurrentSegment), model.Race);
			});

			var currentSegmentLength = new TriggeredFuncCounter<Race, double?>("Length of segment").With(x =>
			{
				x.BindTo(model.Race, y => y.CurrentSegment?.Length);
				x.AddTrigger(nameof(Race.CurrentSegment), model.Race);
			});


			//DISTANCE counters (4)
			var globalDistance = new DistanceCounter("Global distance");

			//var raceDistance3 = new DistanceCounter3("S(by speed)");
			var segmentDistance = new DistanceCounter("Segment distance");

			var raceLength = new TriggeredFuncCounter<Race, double?>("Length of race").With(x =>
			{
				x.BindTo(model.Race, y => y.LengthOfRace);
				x.AddTrigger(nameof(Race.LengthOfRace), model.Race);
			});

			var segmentDistanceToTurn = new RemainingDistanceCounter("Distance to turn").With(x =>
			{
				x.SetBase(segmentDistance);
				x.SetTarget(currentSegmentLength);
				x.Size = CounterSize.XXL;
			});

			var racePassedDistance = new TriggeredFuncCounter<Race, double?>("Race distance").With(x =>
			{
				x.BindTo(model.Race, y =>
				{
					return y.Segments.TakeWhile(z => z.Passed).Sum(z => z.Length)
							+ segmentDistance.Value;
				});
				x.AddTrigger(nameof(Race.Segments), model.Race);
				x.AddTrigger(nameof(IRaceCounter<double?>.Value), segmentDistance);
			});

			var raceDistanceToFinish = new RemainingDistanceCounter("Distance to finish").With(x =>
			{
				x.SetBase(racePassedDistance);
				x.SetTarget(raceLength);
			});


			//SPEED (6)
			var raceAverageSpeed = new AvgSpeedKmhCounter("Avg race speed", 86400);
			var segmentAvgSpeed = new AvgSpeedKmhCounter("Avg segment speed", 86400);
			var segment15SecondsSpeed = new AvgSpeedKmhCounter("Avg segment speed 30 sec", 15);
			var segment5SecondsSpeed = new AvgSpeedKmhCounter("Current speed", 5)
			{
				Size = CounterSize.XXL,
				Color = CounterColor.Green
			}.With(x => x.AddTrigger("Value", raceTimer));

			var recommendedSpeed2 = new TriggeredFuncCounter<Race, double?>("Recommended speed value", "{0}").With(x =>
			{
				x.BindTo(model.Race, y =>
				{
					if (y.IsRunning)
					{
						var timerFromStart = raceTimer.Value;
						var timerAtEndOfSegment = y.TimeOffsetAtTheEndOfCurrentSegment;
						var timeLeft = timerAtEndOfSegment - timerFromStart;
						var remainingRaceTimeSeconds = raceRemainingTime.Value.GetValue().TotalSeconds;
						if (timeLeft?.TotalSeconds <= 1) return remainingRaceTimeSeconds > 0 ? 1000 : 0;
						var distanceLeft = y.CurrentSegment?.Length - segmentDistance?.Value;
						var speed = distanceLeft / timeLeft?.TotalSeconds * 3.6;
						if (speed >= 90) return 1000;
						if (speed <= 1) return 0;
						return speed;
					}
					return 0;
				});
				x.AddTrigger(nameof(Race.CurrentSegment), model.Race);
				x.AddTrigger(nameof(Race.IsRunning), model.Race);
				x.AddTrigger(nameof(IRaceCounter<TimeSpan?>.Value), raceTimer);
				x.AddTrigger(nameof(IRaceCounter<TimeSpan?>.Value), raceRemainingTime);
			});

			var recommendedSpeed = new TriggeredFuncCounter<Race, double?>("Recommended speed value", "{0}").With(x =>
			{
				x.BindTo(model.Race, y =>
				{
					if (y.IsRunning)
					{
						var timeLeft = timeToTurn.Value;
						var remainingRaceTimeSeconds = raceRemainingTime.Value.GetValue().TotalSeconds;
						if (timeLeft?.TotalSeconds <= 1) return remainingRaceTimeSeconds > 0 ? 1000 : 0;
						var distanceLeft = segmentDistanceToTurn.Value;
						var speed = distanceLeft / timeLeft?.TotalSeconds * 3.6;
						if (speed >= 90) return 1000;
						if (speed <= 1) return 0;
						return speed;
					}
					return 0;
				});
				x.AddTrigger(nameof(Race.CurrentSegment), model.Race);
				x.AddTrigger(nameof(Race.IsRunning), model.Race);
				x.AddTrigger(nameof(IRaceCounter<TimeSpan?>.Value), raceTimer);
				x.AddTrigger(nameof(IRaceCounter<TimeSpan?>.Value), raceRemainingTime);
			});


			var recommendedSpeedString = new TriggeredFuncCounter<Race, string>("Recommended speed", "{0}").With(x =>
			{
				x.BindTo(model.Race, y =>
				{
					if (!y.IsRunning) return "0";
					var speed = Convert.ToInt32(recommendedSpeed.Value);
					if (speed == 1000) return "MAX";
					if (speed == 0) return "STOP";
					return speed.ToString();
				});
				x.AddTrigger("Value", recommendedSpeed);
				x.AddTrigger("Value", segment5SecondsSpeed);
				x.Size = CounterSize.XXL;
				x.SetImportant(true);
				x.ValueChanged = y =>
				{
					var speed = (int)recommendedSpeed.Value.GetValue();
					if (speed == 0 || speed == 1000)
					{
						x.SetCritical(true);
						return;
					}
					var avgSpeed = segment5SecondsSpeed.Value;
					var rel = avgSpeed > 0.5 ? Math.Abs(speed / avgSpeed) : 1000;
					x.SetImportant(rel > 1.5 || rel < 0.75);
					x.SetCritical(rel > 2 || rel < 1 / 2);
				};
			});

			//Mixed
			var lagLeadTime = new TriggeredFuncCounter<Race, TimeSpan>("LAG or LEAD time", @"{0:hh\:mm\:ss}").With(x =>
			{
				x.BindTo(model.Race, y =>
				{
					var speed = segment5SecondsSpeed.Value;//segmentFiveSecondsSpeed.TypedValue;
					speed = Math.Round(speed, speed >= 10 ? 0 : 1); 
					var timerFromStart = raceTimer.Value;
					var distanceLeft = y.CurrentSegment?.Length - segmentDistance?.Value;

					if (speed <= 0.5 || !timerFromStart.HasValue || !distanceLeft.HasValue) return TimeSpan.FromDays(1);
					var timeToEndOfSeg = distanceLeft.Value / speed * 3.6; //relative time TO End Of Segment With Current Speed

					var time1 = timerFromStart.Value + TimeSpan.FromSeconds(timeToEndOfSeg); //time AT End Of Segment With Current Speed
					var time2 = y.TimeOffsetAtTheEndOfCurrentSegment;

					var dt = (time2 - time1).TotalSeconds;

					dt = Math.Round(dt, 0);


					//drop milliseconds to avoid blinking
					return TimeSpan.FromSeconds(dt);
				});
				x.AddTrigger("Value", recommendedSpeed);
				x.AddTrigger("Value", segment5SecondsSpeed);
				x.Size = CounterSize.XL;
				x.SetImportant(false);
				x.SetCritical(false);
				x.ValueChanged = y =>
				{
					var seconds = y.Value.TotalSeconds;
					if (y.Value.Days == 1) x.Color = CounterColor.Red;
					{
						if (seconds < -30) x.Color = CounterColor.Red;
						if (seconds >= -30 && seconds <= 30) x.Color = CounterColor.White;
						if (seconds > 30) x.Color = CounterColor.Green;
					}
					//x.SetImportant(seconds > 30);
					//x.SetCritical(seconds > 60);
				};
				x.ApproximationCount = 10;
				x.ApproximationFunction = y => 
				{
					if (!y.Any()) return TimeSpan.FromSeconds(0);
					if (y.Any(z => z.Days == 1)) return TimeSpan.FromDays(1);
					var avgSeconds = y.Select(z => z.TotalSeconds).Average();
					return TimeSpan.FromSeconds(avgSeconds); 
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
				segment5SecondsSpeed,
				recommendedSpeedString,
				lagLeadTime
			};

			var group3 = new ObservableRangeCollection<IRaceCounter>()
			{
				raceLength, raceDuration, globalDistance, racePassedDistance, segmentDistance, 
				raceDistanceToFinish, raceAverageSpeed, segmentAvgSpeed, currentSegmentSpeed, currentSegmentLength
			};

			groups.Add(new CountersGroup { Counters = group0, Weight = 0.5 });
			groups.Add(new CountersGroup { Counters = group1 });
			groups.Add(new CountersGroup { Counters = group2 });
			groups.Add(new CountersGroup { Counters = group3 });

			HiddenCounters = new List<IRaceCounter>();
			HiddenCounters.AddRange(new IRaceCounter[]
			{
				raceAverageSpeed,
				raceRemainingTime,
				recommendedSpeed,
				raceLength,
				currentSegmentNo,
				currentSegmentSpeed,
				currentSegmentLength,
				segmentAvgSpeed,
				segmentDistance,
				segmentEndTime,
				segmentTimer,
				segment15SecondsSpeed
			});

			SegmentCounters = new List<IRaceCounter>()
			{
				segmentTimer,
				segmentEndTime,
				segmentAvgSpeed,
				segmentDistance,
				segment5SecondsSpeed
			};

			AllCounters = HiddenCounters
				.Union(Groups.SelectMany(x => x.Counters))
				.Distinct()
				.ToList();

			foreach (var c in AllCounters)
			{
				c.Init();
			}
		}

		public IEnumerable<IRaceCounter> All
		{
			get
			{
				return AllCounters;
			}
		}

		public IEnumerable<IRaceCounter> Segment
		{
			get
			{
				return SegmentCounters;
			}
		}

		public void ProcessNewLocation(LocationData loc)
		{
			var locationCounters = All.OfType<ILocationProcessor>().ToList();
			foreach (var c in locationCounters)
			{
				c.SetLocation(loc);
			};
		}
	}
}
