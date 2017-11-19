using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using LegendDrive.Counters;
using LegendDrive.Counters.Interfaces;
using LegendDrive.Persistance;
using Newtonsoft.Json.Linq;

namespace LegendDrive.Model.RaceModel
{
    public class Race : BaseBindingObject<Race>, ISupportStatePersistance
    {
        private Object _syncObject = new object();
        public Race()
        {
            Turns.CollectionChanged += OnTurnsCollectionChanged;
            Segments.CollectionChanged += OnSegmentsCollectionChanged;
        }

        public DateTime? StartTime
        {
            get
            {
                return Turns.FirstOrDefault()?.TurnTime;
            }
        }

        public DateTime? FinishTime
        {
            get
            {
                return Turns.LastOrDefault()?.TurnTime;
            }
        }

        public TimeSpan DurationOfRace
        {
            get
            {
                return TimeSpan.FromSeconds(Segments.Sum(y => y.SegmentTime.TotalSeconds)) + TimeCorrectionOnCheckpoints;
            }
        }

        public double LengthOfRace
        {
            get
            {
                return Segments.Sum(y => y.Length);
            }
        }


        public DateTime? EstimatedFinishTime
        {
            get
            {
                if (StartTime.HasValue)
                {
                    return StartTime.Value.Add(DurationOfRace);
                }
                return null;
            }
        }

        public bool CanGoBack
        {
            get
            {
                return IsRunning && Turns.Skip(1).Any(); //first turn is start
            }
        }

        public bool CanDelete
        {
            get
            {
                return Segments.Any();
            }
        }

        RaceSegment _currentSegment;
        public RaceSegment CurrentSegment
        {
            get
            {
                if (_currentSegment == null)
                {
                    _currentSegment = Segments.FirstOrDefault(x => x.IsCurrent);
                }
                return _currentSegment;
            }
        }

        public DateTime? TimeAtTheEndOfCurrentSegment
        {
            get
            {
                if (StartTime.HasValue)
                {
                    return StartTime.Value.Add(TimeOffsetAtTheEndOfCurrentSegment);
                }
                return null;
            }
        }

        public TimeSpan TimeCorrectionOnCheckpoints { get; set; }

        public TimeSpan TimeOffsetAtTheEndOfCurrentSegment
        {
            get
            {
                if (StartTime.HasValue)
                {
                    var timeOffset = Segments.TakeWhile(x => x.Passed || x.IsCurrent)
                                             .Aggregate(new TimeSpan(), (x, y) => x + y.SegmentTime);

                    return timeOffset + TimeCorrectionOnCheckpoints;
                }
                return TimeSpan.FromMilliseconds(0);
            }
        }

        public TimeSpan DelayTime
        {
            get
            {
                if (CurrentSegment != null && CurrentSegment.Speed > 0)
                {
                    var estimatedTime = Segments.Where(a => a.Passed)
                                             .Aggregate(new TimeSpan(), (a, b) => a + b.SegmentTime);
                    estimatedTime += TimeSpan.FromHours(CurrentSegment.PassedDistance / 1000 / CurrentSegment.Speed);

                    var realTime = DateTime.Now - StartTime.Value;

                    return realTime - estimatedTime;
                }
                return new TimeSpan();
            }
        }

        bool _isRunning;
        public bool IsRunning
        {
            get
            {
                return _isRunning;
            }
            private set
            {
                if (_isRunning != value)
                {
                    _isRunning = value;
                    RaisePropertyChanged(nameof(IsRunning));
                    RaisePropertyChanged(nameof(CanGoBack));
                }
            }
        }


        public bool ParseAndAddNewSegments(string newSegmentText)
        {
            var newSegments = RaceSegment.Parse(newSegmentText);
            AddSegments(newSegments);
            return true;
        }

        public bool AddSegments(IEnumerable<RaceSegment> segmentsToAdd)
        {
            var last = Segments.LastOrDefault();
            var lastNo = last != null ? last.No : 0;
            var lastSpeed = last != null ? last.Speed : 0;
            var numberedSegmentsList = new List<RaceSegment>();
            lock (_syncObject)
            {
                foreach (var s in segmentsToAdd)
                {
                    s.No = lastNo + 1;
                    if (Math.Abs(s.Speed) < 0.0001)
                    {
                        s.Speed = lastSpeed;
                    }
                    lastNo = s.No;
                    numberedSegmentsList.Add(s);
                }
                Segments.AddRange(numberedSegmentsList);
            }
            return true;
        }

        public bool AddSegment(RaceSegment segment)
        {
            lock (_syncObject)
            {
                var last = Segments.LastOrDefault();
                var lastNo = last != null ? last.No : 0;
                segment.No = lastNo + 1;
                if (Math.Abs(segment.Speed) < 0.0001) return false;
                Segments.Add(segment);
            }
            return true;
        }

        public void AddTurn()
        {
            lock (_syncObject)
            {
                var last = Turns.LastOrDefault();
                var lastNo = last != null ? last.No : 0;
                var turn = new TurnInfo()
                {
                    No = lastNo + 1,
                    TurnTime = DateTime.Now
                };
                Turns.Add(turn);
            }
        }

        public ObservableRangeCollection<TurnInfo> Turns { get; set; } = new ObservableRangeCollection<TurnInfo>();
        public ObservableRangeCollection<RaceSegment> Segments { get; set; } = new ObservableRangeCollection<RaceSegment>();


        private void OnTurnsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            SuppressEvent();
            RaisePropertyChanged(nameof(Turns)); //main object
            SyncTurnsAndSegments();
            RaisePropertyChanged(nameof(CanGoBack));
            if (Turns.Count <= 1)
            {
                RaisePropertyChanged(nameof(StartTime));
            }
            ResumeEvents();
        }

        private void OnSegmentsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            SuppressEvent();
            RaisePropertyChanged(nameof(Segments)); //main object
            SyncTurnsAndSegments();
            RaisePropertyChanged(nameof(DurationOfRace));
            RaisePropertyChanged(nameof(LengthOfRace));
            RaisePropertyChanged(nameof(CanDelete));
            RaisePropertyChanged(nameof(EstimatedFinishTime));
            ResumeEvents();
        }

        private void SyncTurnsAndSegments()
        {
            if (!IsRunning) return;
            lock (_syncObject)
            {
                var prevCurrentSegment = CurrentSegment;
                //FIRST turn - is start
                //LAST turn - is finish
                var lastTurn = Turns.LastOrDefault();
                var lastTurnNo = lastTurn != null ? lastTurn.No : 0;
                foreach (var s in Segments)
                {
                    s.Passed = s.No < lastTurnNo;
                    s.IsCurrent = s.No == lastTurnNo;
                }
                _currentSegment = null;
                if (prevCurrentSegment != CurrentSegment)
                {
                    RaisePropertyChanged(nameof(CurrentSegment));
                    RaisePropertyChanged(nameof(TimeAtTheEndOfCurrentSegment));
                    RaisePropertyChanged(nameof(TimeOffsetAtTheEndOfCurrentSegment));
                }
            }
        }

        internal void RemoveLastTurn()
        {
            lock (_syncObject)
            {
                var last = Turns.LastOrDefault();
                if (last != null)
                {
                    Turns.Remove(last);
                }
            }
        }

        internal void StartRace()
        {
            IsRunning = true;
            Turns.Clear();
            TimeCorrectionOnCheckpoints = new TimeSpan();
            AddTurn();
        }

        internal void FinishRace()
        {
            AddTurn();
            IsRunning = false;
        }

        internal void DeleteLastSegment()
        {
            lock (_syncObject)
            {
                var last = Segments.LastOrDefault();
                if (last != null) Segments.Remove(last);
            }
        }

        internal void ResetTimeOnCheckpoint()
        {
            //this reset lag or lead time on checkpoint.
            //effectivly we just need to 
            if (!IsRunning || CurrentSegment == null)
            {
                return;
            }

            var delayTime = DelayTime;

            RaisePropertyChanged(nameof(EstimatedFinishTime));
            RaisePropertyChanged(nameof(DurationOfRace));
            RaisePropertyChanged(nameof(TimeOffsetAtTheEndOfCurrentSegment));
        }

        #region State Management
        public JObject GetState()
        {
            var obj = new JObject();
            obj.AddValue(nameof(IsRunning), IsRunning);
            obj.AddCollection(nameof(Turns), Turns);
            obj.AddCollection(nameof(Segments), Segments);
            return obj;
        }

        public void LoadState(JObject obj)
        {
            obj.LoadCollection(nameof(Turns), Turns);
            obj.LoadCollection(nameof(Segments), Segments);
            IsRunning = obj.GetValue<bool>(nameof(IsRunning));
            SyncTurnsAndSegments();
        }
        #endregion
    }
}

