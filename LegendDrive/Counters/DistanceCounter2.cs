using System;
using System.Collections.Generic;
using System.Linq;

namespace LegendDrive
{
	public class DistanceCounter2 : BaseCounter<double?>, ILocationProcessor, ISupportHistory
	{
		double? _distance;
		LocationData _previousLocation;
		Stack<double?> _history;

		public DistanceCounter2():this("Distance")
		{
		}

		public DistanceCounter2(string name):base(name)
		{
			_history = new Stack<double?>();
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
				return _distance; 
			}
		}

		public virtual void SetLocation(LocationData location)
		{
			location = location.RoundCoords();
			if (IsRunning)
			{
				if (_previousLocation != null)
				{
					_distance += location.DistanceTo(_previousLocation);
					OnPropertyChanged("Value");
				}
			}
			_previousLocation = location;
		}

		public override void Reset()
		{
			_distance = 0;
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
	}
}
