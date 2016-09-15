using System.ComponentModel;

namespace LegendDrive.Counters
{
	public class BaseBindingObject : INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler PropertyChanged;

		protected void OnPropertyChanged(string propertyName = null)
		{
			var handler = PropertyChanged;
			if (handler != null)
			{
				handler(this, new PropertyChangedEventArgs(propertyName));
				handler(this, new PropertyChangedEventArgs("."));
			}
		}
	}
}

