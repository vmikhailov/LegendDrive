using System;
namespace LegendDrive.Counters.Interfaces
{
	public interface ISupportHistory
	{
		void Push();
		void Pop();
	}
}

