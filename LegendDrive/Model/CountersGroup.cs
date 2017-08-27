using System;
using LegendDrive.Counters.Interfaces;

namespace LegendDrive.Model
{
	public class CountersGroup
	{
		public string Name { get; set; }
		public double Weight { get; set; } = 1;
		public ObservableRangeCollection<IRaceCounter> Counters { get; set; }
	}
}
