using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using LegendDrive.Counters.Interfaces;

namespace LegendDrive.Counters
{
    public class BaseBindingObject<T> : INotifyPropertyChanged, ISupportSubscription<T>
    {
        public event PropertyChangedEventHandler PropertyChanged;
        int suppressed;
        List<string> suppressedProperties = new List<string>();
        object syncObject = new object();

        void RaisePropertyChangedSingle(string propertyName = null)
        {
            TriggerSubscribers(propertyName);
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
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


        IDictionary<string, List<Action<T>>> subscriptions = new Dictionary<string, List<Action<T>>>();

        public void Subscribe(string property, Action<T> action)
        {
			var listOfProperties = property.Split(',').Select(x => x.Trim()).ToList();
            var listOfRealProperties = typeof(T).GetProperties().Select(x => x.Name).Intersect(listOfProperties).ToList();
            foreach(var p in listOfRealProperties)
            {
                if(!subscriptions.TryGetValue(p, out List<Action<T>> actions))
                {
                    subscriptions[p] = actions = new List<Action<T>>();
                }
                actions.Add(action);
            }
        }

        public void UnSubscribe(string property, Action<T> action)
        {
			var listOfProperties = property.Split(',').Select(x => x.Trim()).ToList();
			var listOfRealProperties = typeof(T).GetProperties().Select(x => x.Name).Intersect(listOfProperties).ToList();
			foreach (var p in listOfRealProperties)
			{
				List<Action<T>> actions = subscriptions.ContainsKey(p) ? actions = subscriptions[p] : null;
                actions?.Remove(action);
			}
        }

        private void TriggerSubscribers(string propertyName)
        {
			if (!subscriptions.ContainsKey(propertyName)) return;
            //Debug.WriteLine($"Subcription property: {propertyName}");

            var list = subscriptions[propertyName];
            list.ForEach(x => x.DynamicInvoke(this));
        }
    }
}

