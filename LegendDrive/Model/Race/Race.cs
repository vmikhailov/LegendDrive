using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

namespace LegendDrive
{
	public class Race : BaseBindingObject
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
				return TimeSpan.FromSeconds(Segments.Sum(y => y.SegmentTime.TotalSeconds));
			}
		}

		public double LengthOfRace
		{
			get
			{
				return Segments.Sum(y => y.Distance);
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
				return Turns.Skip(1).Any(); //first turn is start
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

		public TimeSpan TimeOffsetAtTheEndOfCurrentSegment
		{
			get
			{
				if (StartTime.HasValue)
				{
					var timeOffset = Segments.TakeWhile(x => x.Passed || x.IsCurrent).Sum(x => x.SegmentTime.TotalMilliseconds);
					return TimeSpan.FromMilliseconds(timeOffset);
				}
				return TimeSpan.FromMilliseconds(0);
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
				_isRunning = value;
				OnPropertyChanged("IsRunning");
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
						if(Math.Abs(lastSpeed) < 0.0001)
						{
							return false;
						}
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
			lock(_syncObject)
			{
				var last = Turns.LastOrDefault();
				var lastNo = last != null ? last.No : 0;
				var turn = new TurnInfo()
				{
					No = lastNo + 1,
					TurnTime = DateTime.Now
				};
				Turns.Add(turn);
				OnPropertyChanged("CanGoBack");
			}
		}

		public ObservableRangeCollection<TurnInfo> Turns { get; set; } = new ObservableRangeCollection<TurnInfo>();
		public ObservableRangeCollection<RaceSegment> Segments { get; set; } = new ObservableRangeCollection<RaceSegment>();


		private void OnTurnsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			SyncTurnsAndSegments();
			OnPropertyChanged("Turns");
		}

		private void OnSegmentsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			SyncTurnsAndSegments();
			OnPropertyChanged("Segments");
		}

		private void SyncTurnsAndSegments()
		{
			if (!IsRunning) return;
			lock(_syncObject)
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
					OnPropertyChanged("CurrentSegment");
					OnPropertyChanged("TimeAtTheEndOfCurrentSegment");
					OnPropertyChanged("TimeOffsetAtTheEndOfCurrentSegment");
				}
			}
		}

		internal void RemoveLastTurn()
		{
			lock(_syncObject)
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
			AddTurn();
		}

		internal void FinishRace()
		{
			AddTurn();
			IsRunning = false;
		}

		internal void DeleteLastSegment()
		{
			lock(_syncObject)
			{
				var last = Segments.LastOrDefault();
				if (last != null) Segments.Remove(last);
			}
		}
	}
}

