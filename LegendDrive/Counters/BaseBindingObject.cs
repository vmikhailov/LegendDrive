using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;

namespace LegendDrive.Counters
{
	public class BaseBindingObject<T> : INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler PropertyChanged;
		int suppressed;
		List<string> suppressedProperties = new List<string>();
		object syncObject = new object();

		void RaisePropertyChangedSingle(string propertyName = null)
		{
			var handler = PropertyChanged;
			if (handler != null)
			{
				handler(this, new PropertyChangedEventArgs(propertyName));
			}
		}

		protected virtual void RaisePropertyChanged(Expression<Func<T>> propertyExpression)
		{
		}

		protected virtual void RaisePropertyChanged(string propertyName = null)
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
				RaisePropertyChangedSingle(propertyName);
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
					RaisePropertyChangedSingle(propertyName);
				}
			}
		}
	}
}

