using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using LegendDrive.Counters.Interfaces;
using LegendDrive.Model;
using LegendDrive.Persistance;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace LegendDrive.Counters
{
	public class DistanceCounter : BaseCounter<double?>, ILocationProcessor, ISupportHistory, ISupportStatePersistance
	{
		double? distance;
		LocationData previousLocation;
		Stack<double?> history;

		public DistanceCounter():this("Distance")
		{
		}

		public DistanceCounter(string name):base(name)
		{
			history = new Stack<double?>();
		}

		public override string ValueString
		{
			get { return Value?.ToString("#,0", NumberFormatInfo); }
		}

		public override double? Value
		{
			get 
			{
				EnsureInitialized();
				return distance; 
			}
		}

		public virtual void SetLocation(LocationData location)
		{
			if (!location.GpsOn) return;
			//location = location.RoundCoords();
			if (IsRunning)
			{
				if (previousLocation != null)
				{
					var delta = location.DistanceTo(previousLocation);
					distance += delta;
					OnPropertyChanged("Value");
				}
			}
			previousLocation = location;
		}

		public override void Reset()
		{
			distance = 0;
			history.Clear();
			OnPropertyChanged("Value");
		}

		public void Push()
		{
			history.Push(distance);
			distance = 0;
			OnPropertyChanged("Value");
		}

		public void Pop()
		{
			if (history.Any())
			{
				distance = distance + history.Pop();
				OnPropertyChanged("Value");
			}
		}

		public override JObject GetState()
		{
			var obj = new JObject();
			obj.AddValue("base", base.GetState());
			obj.AddValue(nameof(distance), distance);
			obj.AddValue(nameof(history), history);
			return obj;
		}

		public override void LoadState(JObject obj)
		{
			distance = obj.GetValue<double?>(nameof(distance));
			history = obj.GetValue<Stack<double?>>(nameof(history));
			base.LoadState(obj.GetValue<JObject>("base"));
			OnPropertyChanged("Value");
		}

		public override void Dispose()
		{
			base.Dispose();
		}
	}
}
