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
            var segmentDistance = new DistanceCounter("Segment distance").With(x => {
                x.Subscribe("Value", y => {
                    if(model.Race.CurrentSegment != null && y.Value.HasValue)
                    {
                        model.Race.CurrentSegment.PassedDistance = y.Value.Value;
                    }
                });
            });

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
                        if (distanceLeft < 0 || speed <= 1) return 0;
                        if (speed >= 90) return 1000;
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
            var lagLeadTime = new TriggeredFuncCounter<Race, TimeSpan>("Delay time", @"{0:hh\:mm\:ss}").With(x =>
            {
                x.BindTo(model.Race, y => y.DelayTime);
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
                x.ApproximationCount = 5;
                x.ApproximationFunction = y =>
                {
                    if (!y.Any()) return TimeSpan.FromSeconds(0);
                    if (y.Any(z => z.Days == 1)) return TimeSpan.FromDays(1);
                    var avgSeconds = y.Select(z => z.TotalSeconds).Average();
                    return TimeSpan.FromSeconds(avgSeconds);
                };
            });

            //speech
            var lastSpeedNotification = 0;
			var lastDistanceNotification = 0;
            //var lastRoundedLen = 0;
            var lastNotificationTime = DateTime.Now;
            var minNotificationInterval = TimeSpan.FromSeconds(2);

            model.Race.Subscribe(nameof(Race.CurrentSegment), x =>
            {
                lock (this)
                {
                    if (model.Race.CurrentSegment != null && model.Race.IsRunning)
                    {
                        var len = (int)model.Race.CurrentSegment.Length;
                        lastDistanceNotification = len;
                        model.Speech?.Speak($"Сегмент {len}");
                    }
                }
            });


            segmentDistanceToTurn.Subscribe(nameof(IRaceCounter<double?>.Value), x =>
            {
                lock (this)
                {
                    if (model.Race.CurrentSegment != null && model.Race.IsRunning)
                    {
                        var len = x.Value.GetValue();
						var k = len > 1000 ? 1000 : len > 100 ? 100 : len > 50 ? 50 : 10;
						var len10 = (int)(Math.Round(len / k) * k);
						var speed = segmentAvgSpeed.Value;
						if (speed < 0.01) return;
						if (Math.Abs(len - len10) > speed / 2) return;
						if (lastDistanceNotification == len10) return;
						
                        lastDistanceNotification = len10;
                        if (len10 == 0)
                        {
                            model.Speech?.Speak($"Поворот");
                        }
                        else
                        {
							if (DateTime.Now - lastNotificationTime < minNotificationInterval) return;
                            if (len10 > 0)
                            {
                                model.Speech?.Speak($"Еще {len10}");
                            }
                            else
                            {
                                if (len < -50)
                                {
                                    model.Speech?.Speak(GetRandomPhrase(len < -200 ? "lost2" : "lost1", model.UseBadLanguage));
                                }
                            }
                        }
						lastNotificationTime = DateTime.Now;
                    }
                }
            });

            recommendedSpeed.Subscribe(nameof(IRaceCounter<double?>.Value), x =>
            {
                lock (this)
                {
                    if (DateTime.Now - lastNotificationTime < minNotificationInterval) return;
                    if (model.Race.CurrentSegment != null && model.Race.IsRunning)
                    {
                        var curSpeed = (int)Math.Round(segmentAvgSpeed.Value);
                        var recSpeed = (int)Math.Round(recommendedSpeed.Value.GetValue());
                        var speedStr = recSpeed >= 1000 ? GetRandomPhrase("maxSpeed", model.UseBadLanguage) : $"Скорость {recSpeed}";
                        if (Math.Abs(lastSpeedNotification - recSpeed) < 5) return;
                        if (Math.Abs(curSpeed - recSpeed) < 5) return;
                        lastSpeedNotification = recSpeed;
                        lastNotificationTime = DateTime.Now;
                        model.Speech?.Speak(speedStr);
                    }
                }
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

        private class VoicePharse
        {
            public VoicePharse(string k, bool b, string t)
            {
                Key = k;
                Bad = b;
                Text = t;
            }

            public string Key { get; set; }
            public bool Bad { get; set; }
            public string Text { get; set; }
        }

        private List<VoicePharse> GetAllPhrases()
        {
            return new List<VoicePharse>
            {
				new VoicePharse("lost2", true,  "Ну всё, пиздец, мы потерялись"),
				//new VoicePharse("lost2", true,  "Где ты права купил, дядя?"),
				//new VoicePharse("lost2", true,  "Как тебя только на паркетник пустили?"),
				//new VoicePharse("lost2", true,  "Нахуй-нахуй, дальше без меня"),
				//new VoicePharse("lost2", true,  "Давай разворачивайся и пиздуй обратно"),
				//new VoicePharse("lost2", false, "Ой, мне страшно"),
				//new VoicePharse("lost2", false, "Я хочу домой"),
				new VoicePharse("lost2", false, "Там точно никакой дороги нет"),
				//new VoicePharse("lost2", false, "Чак будет смеяться над нами"),
				new VoicePharse("lost1", true,  "Куда ты, блять, едешь?"),
				//new VoicePharse("lost1", true,  "Стой, сука, мы же потеряемся"),
				//new VoicePharse("lost1", true,  "Ты вообще по сторонам смотришь?"),
				new VoicePharse("lost1", false, "Пропущен поворот"),
				new VoicePharse("lost1", false, "Придется теперь ехать обратно"),
				new VoicePharse("lost1", false, "Так будет сложно победить"),
				//new VoicePharse("maxSpeed", true,  "Дави на газ!"),
				//new VoicePharse("maxSpeed", true,  "Тапку в пол"),
				new VoicePharse("maxSpeed", false, "максимальная скорость"),
				new VoicePharse("maxSpeed", false, "как можно быстрее")
            };
        }

        Random rnd = new Random();
		private string GetRandomPhrase(string key, bool useBadLanguage)
		{
            var match = GetAllPhrases().Where(x => x.Key == key && x.Bad == useBadLanguage).ToList();
            if(match.Count() == 0)
            {
                return "";
            }

            return match[rnd.Next(match.Count())].Text;
		}

	}
}
