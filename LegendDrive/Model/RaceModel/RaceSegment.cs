using System;
using System.Collections.Generic;
using System.Globalization;
using LegendDrive.Counters;
using LegendDrive.Persistance;
using Newtonsoft.Json.Linq;

namespace LegendDrive.Model.RaceModel
{
	public class RaceSegment : BaseBindingObject<RaceSegment>, ISupportStatePersistance 
	{
		public RaceSegment()
		{
		}

		public int No
		{
			get; set;
		}

		public double Length
		{
			get; set;
		}

		public double Speed
		{
			get; set;
		}

		public TimeSpan SegmentTime
		{
			get
			{
				var rideTime = TimeSpan.FromHours(Length / 1000 / Speed);
				return rideTime > Timeout ? rideTime : Timeout;
			}
		}

		public TimeSpan Timeout
		{
			get; set;
		}

		bool _isCurrent;
		public bool IsCurrent
		{
			get 
			{ 
				return _isCurrent; 
			}
			set
			{
				if (_isCurrent != value)
				{
					_isCurrent = value;
					RaisePropertyChanged(nameof(IsCurrent));
				}
			}
		}

		bool _passed;
		public bool Passed
		{
			get
			{
				return _passed;
			}
			set
			{
				if (_passed != value)
				{
					_passed = value;
					RaisePropertyChanged(nameof(Passed));
				}
			}
		}

		public override string ToString()
		{
			return string.Format("{0} {1} {2}", Length, Speed, Timeout.TotalMinutes);
		}

		public static IEnumerable<RaceSegment> Parse(string newDataText)
		{
			if (string.IsNullOrWhiteSpace(newDataText))
			{
				yield break;
			}
			var parts = newDataText.Trim().Split(' ');
			if (parts.Length == 3)
			{
				//neutralization enter
				var segment = new RaceSegment()
				{
					Length = DoubleParse(parts[0]),
					Speed = DoubleParse(parts[1]),
					Timeout = ParseTimeSpan(parts[2])
				};
				yield return segment;
				yield break;
			}

			if (parts.Length == 2)
			{
				var segment = new RaceSegment()
				{
					Length = DoubleParse(parts[0]),
					Speed = DoubleParse(parts[1]),
				};
				yield return segment;
				yield break;
			}

			for (int i = 0; i < parts.Length; i++)
			{
				var segment = new RaceSegment()
				{
					Length = DoubleParse(parts[i]),
				};
				yield return segment;
			}
			yield break;
		}

		private static TimeSpan ParseTimeSpan(string str)
		{
			if (str.Contains(".."))
			{
				var parts = str.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
				return new TimeSpan(0, Int32.Parse(parts[0]), parts.Length > 1 ? Int32.Parse(parts[1]) : 0);
			}
			else
			{
				return TimeSpan.FromMinutes(DoubleParse(str));
			}
		}

		private static Double DoubleParse(string str)
		{
			str = str.Trim('.');
			return Double.Parse(str, CultureInfo.InvariantCulture);
		}


		public JObject GetState()
		{
			var obj = new JObject();
			obj.AddValue(nameof(No), No);
			obj.AddValue(nameof(Length), Length);
			obj.AddValue(nameof(Speed), Speed);
			obj.AddValue(nameof(Timeout), Timeout);
			return obj;
		}

		public void LoadState(JObject obj)
		{
			No = obj.GetValue<int>(nameof(No));
			Length = obj.GetValue<double>(nameof(Length));
			Speed = obj.GetValue<double>(nameof(Speed));
			Timeout = obj.GetValue<TimeSpan>(nameof(Timeout));
		}

		protected override void RaisePropertyChanged(string propertyName = null)
		{
			base.RaisePropertyChanged(propertyName);
			//base.RaisePropertyChanged(nameof(.)); //workaround 
		}
	}
}

