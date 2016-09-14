namespace LegendDrive
{
	public interface IDifferenceCounter<T>
	{
		void SetBase(IRaceCounter<T> counter);
		void SetBase(T value);
		void SetTarget(IRaceCounter<T> counter);
		void SetTarget(T value);
	}
}

