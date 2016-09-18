using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace LegendDrive.Counters
{
	public class BaseBindingObject : INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler PropertyChanged;
		int suppressed;
		List<string> suppressedProperties = new List<string>();
		object syncObject = new object();

		void OnPropertyChangedSingle(string propertyName = null)
		{
			var handler = PropertyChanged;
			if (handler != null)
			{
				handler(this, new PropertyChangedEventArgs(propertyName));
				//handler(this, new PropertyChangedEventArgs("."));
			}
		}

		protected virtual void OnPropertyChanged(string propertyName = null)
		{
			if (suppressed > 0)
			{
				lock (syncObject)
				{
					suppressedProperties.Add(propertyName ?? ".");
				}
			}
			else
			{
				OnPropertyChangedSingle(propertyName);
			}
		}

		protected void SuppressEvent()
		{
			//suppressed++;
		}

		protected void ResumeEvents()
		{
			if (suppressed > 0)
			{
				lock (syncObject)
				{
					if (suppressed > 0)
					{
						ResumeEventsImpl();
					}
				}
			}
		}

		private void ResumeEventsImpl()
		{
			if (--suppressed == 0)
			{
				foreach (var propertyName in suppressedProperties.Distinct().ToList())
				{
					OnPropertyChangedSingle(propertyName);
				}
			}
		}
	}
}

