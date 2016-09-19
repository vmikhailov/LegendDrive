using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using LegendDrive.Collections;
using LegendDrive.Counters;
using LegendDrive.Messaging;
using LegendDrive.Model.RaceModel;
using Xamarin.Forms;

namespace LegendDrive.Model.ViewModel
{
	public class RaceViewModel : BaseBindingObject<RaceViewModel>, IViewModel<Race>
	{
		Race race;

		public RaceViewModel()
		{
		}

		public Race Model
		{
			get
			{
				return race;
			}
			set
			{
				race = value;
				race.PropertyChanged += Race_PropertyChanged;
				Segments = new ViewModelObservableCollection<RaceSegment, RaceSegmentViewModel>(race.Segments);
				RaisePropertyChanged(nameof(CurrentSegment));
				RaisePropertyChanged(nameof(Segments));
			}
		}

		public RaceViewModel(Race race)
		{
			Model = race;
		}

		public RaceSegmentViewModel CurrentSegment
		{
			get
			{
				var index = race.CurrentSegment?.No ?? 0 - 1;
				return index >= 0 ? Segments[index] : null;
			}
		}

		public ObservableCollection<RaceSegmentViewModel> Segments { get; private set; }

		void Race_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if (e.PropertyName == nameof(Race.CurrentSegment))
			{
				RaisePropertyChanged(nameof(CurrentSegment));
			}
			if (e.PropertyName == nameof(Race.Segments))
			{
				RaisePropertyChanged(nameof(Segments));
			}
			//RaisePropertyChanged(".");
		}

		public ICommand CmdStartFinish { get; private set; }
		public ICommand CmdReset { get; private set; }
		public ICommand CmdClear { get; private set; }
		public ICommand CmdGpsReset { get; private set; }
		public ICommand CmdBack { get; private set; }
		public ICommand CmdTurn { get; private set; }
		public ICommand CmdDel { get; private set; }
	}
}
