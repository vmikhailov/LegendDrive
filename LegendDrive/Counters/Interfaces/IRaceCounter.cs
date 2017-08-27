using System.ComponentModel;

namespace LegendDrive.Counters.Interfaces
{
	public interface IRaceCounter : INotifyPropertyChanged
	{
		string Name { get; }
		object ValueObject { get; }
		string ValueString { get; }
		string DebugString { get; }

		void Reset();
		void Start();
		void Stop();
		bool IsImportant { get; }
		bool IsInitialized { get; }
		bool IsCritical { get; }
		bool IsRunning { get; }
		void SetImportant(bool value);
		void SetCritical(bool value);
		void Flash();
		void Flash(int msec);
		CounterColor Color { get; set; }
		CounterSize Size { get; set; }

		void Init();
	}

	public interface IRaceCounter<T> : IRaceCounter
	{
		T Value { get; }
	}


	public enum CounterColor
	{
		White = 0,
		Red,
		Blue,
		Orange,
		Green,
		Magenta,
		Yellow,
		AbsoluteWhite
	}

	public enum CounterSize
	{
		XXS = -3,
		XS = -2,
		S = -1,
		M = 0,
		L = 1,
		XL = 2,
		XXL = 3
	}
}

