using System;
using System.Linq;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using LegendDrive.Model.ViewModel;
using System.Collections.Generic;
using System.Collections;

namespace LegendDrive.Collections
{
	public class ViewModelObservableCollection<T, V> : ObservableRangeCollection<V>
		where V: class, IViewModel<T>, new()
		where T: class
	{
		ObservableCollection<T> source;

		public ViewModelObservableCollection()
		{
		}

		public ViewModelObservableCollection(ObservableCollection<T> source)
		{
			this.source = source;
			this.AddRange(source.Select(x => new V().With(y => y.Model = x)).ToList());
			this.source.CollectionChanged += Source_CollectionChanged;
		}

		IEnumerable<T> GetItems(IList items)
		{
			return items.OfType<T>().Union(items.OfType<IEnumerable<T>>().SelectMany(x => x));
		}

		void Source_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
		{
			switch (e.Action)
			{
				case NotifyCollectionChangedAction.Add:
					var newItems = GetItems(e.NewItems).Select(x => new V().With(y => y.Model = x)).ToList();
					AddRange(newItems);
					break;
					
				case NotifyCollectionChangedAction.Move:
					Move(e.OldStartingIndex, e.NewStartingIndex);
					break;
					
				case NotifyCollectionChangedAction.Remove:
					if (e.OldItems != null)
					{
						var modelsToRemove = GetItems(e.OldItems).ToList();
						var viewModelsToRemove = new List<V>();
						foreach (var v in this)
						{
							if (modelsToRemove.Contains(v.Model))
							{
								viewModelsToRemove.Add(v);
							}
						}
						RemoveRange(viewModelsToRemove);
					}
					break;
				case NotifyCollectionChangedAction.Replace:
					throw new NotImplementedException();

				case NotifyCollectionChangedAction.Reset:
					this.Clear();
					break;
			}
		}
	}
}
