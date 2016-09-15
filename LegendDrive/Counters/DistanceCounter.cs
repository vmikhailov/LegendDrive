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
		double? _distance;
		double _smoothIncrement;
		LocationData _previousLocation;
		Stack<double?> _history;
		Timer _timer;

		int periodOfUpdate = 1100; //ms
		int periodOfSmoothUpdate = 100;

		public DistanceCounter():this("Distance")
		{
		}

		public DistanceCounter(string name):base(name)
		{
			_history = new Stack<double?>();
			//_timer = new Timer(x => SmoothValueUpdate(), null, 0, periodOfSmoothUpdate);
		}

		void SmoothValueUpdate()
		{
			var currentSpeed = _previousLocation?.Speed ?? 0;
			_smoothIncrement += periodOfSmoothUpdate * currentSpeed / periodOfUpdate;
			if(_distance.HasValue && 
			   Math.Round(_distance.Value + _smoothIncrement,0 ) > Math.Round(_distance.Value, 0))
			{
				OnPropertyChanged("Value");
			}
		}

		public override string ValueString
		{
			get { return TypedValue?.ToString("#,0", NumberFormatInfo); }
		}

		public override double? TypedValue
		{
			get 
			{
				EnsureInitialized();
				return _distance + _smoothIncrement; 
			}
		}

		public virtual void SetLocation(LocationData location)
		{
			if (!location.GpsOn) return;
			//location = location.RoundCoords();
			if (IsRunning)
			{
				if (_previousLocation != null)
				{
					var delta = location.DistanceTo(_previousLocation);
					_distance += delta;
					_smoothIncrement = 0;
					OnPropertyChanged("Value");
				}
			}
			_previousLocation = location;
		}

		public override void Reset()
		{
			_distance = 0;
			_history.Clear();
			OnPropertyChanged("Value");
		}

		public void Push()
		{
			_history.Push(_distance);
			_distance = 0;
			OnPropertyChanged("Value");
		}

		public void Pop()
		{
			if (_history.Any())
			{
				_distance = _distance + _history.Pop();
				OnPropertyChanged("Value");
			}
		}

		public JObject GetState()
		{
			var obj = new JObject();
			obj.AddValue(nameof(_distance), _distance);
			obj.AddValue(nameof(_history), _history);
			return obj;
		}

		public void LoadState(JObject obj)
		{
			_distance = obj.GetValue<double?>(nameof(_distance));
			_history = obj.GetValue<Stack<double?>>(nameof(_history));
			OnPropertyChanged("Value");
		}

		public override void Dispose()
		{
			base.Dispose();
			if (_timer != null) _timer.Dispose();
		}
	}
}
