using System;
using LegendDrive.Counters;
using LegendDrive.Model.RaceModel;
using Xamarin.Forms;

namespace LegendDrive.Model.ViewModel
{
	public class RaceSegmentViewModel : 
		BaseBindingObject<RaceSegmentViewModel>, 
		IViewModel<RaceSegment>
	{
		RaceSegment segment;
		public RaceSegmentViewModel()
		{
		}

		public RaceSegment Model
		{
			get
			{
				return segment;
			}
			set
			{
				segment = value;
				segment.PropertyChanged += Segment_PropertyChanged;
			}
		}

		void Segment_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if (e.PropertyName == nameof(RaceSegment.IsCurrent))
			{
				RaisePropertyChanged(nameof(BackgroundColor));
				RaisePropertyChanged(nameof(FontSizeNo));
				RaisePropertyChanged(nameof(FontSizeLength));
				RaisePropertyChanged(nameof(FontSizeSpeed));
				RaisePropertyChanged(nameof(FontSizeTimeout));
				RaisePropertyChanged(nameof(ListItemHeight));
			}

			if (e.PropertyName == nameof(RaceSegment.Passed))
			{
				RaisePropertyChanged(nameof(BackgroundColor));
			}

			if (e.PropertyName == nameof(RaceSegment.No))
			{
				RaisePropertyChanged(nameof(No));
			}

			if (e.PropertyName == nameof(RaceSegment.Length))
			{
				RaisePropertyChanged(nameof(Length));
			}

			if (e.PropertyName == nameof(RaceSegment.Speed))
			{
				RaisePropertyChanged(nameof(Speed));
			}

			if (e.PropertyName == nameof(RaceSegment.Timeout))
			{
				RaisePropertyChanged(nameof(Timeout));
			}
		}

		public int No => segment.No;
		public double Length => segment.Length;
		public double Speed => segment.Speed;
		public string Timeout
		{
			get
			{
				var minutes = (int)segment.Timeout.TotalMinutes;
				return (minutes == 0 ? String.Empty : minutes.ToString("g")).PadLeft(3);
			}
		}

		private double GetFontSize(double baseSize)
		{
			return segment.IsCurrent? baseSize * 1.5 : baseSize;
		}

		public double FontSizeNo => GetFontSize(UIConfiguration.SegmentListNoFontSize);

		public double FontSizeLength => GetFontSize(UIConfiguration.SegmentListLengthFontSize);

		public double FontSizeSpeed => GetFontSize(UIConfiguration.SegmentListSpeedFontSize);

		public double FontSizeTimeout => GetFontSize(UIConfiguration.SegmentListTimeoutFontSize);

		public int ListItemHeight => segment.IsCurrent ? UIConfiguration.LargeButtonHeight : 32;

		public Color BackgroundColor =>
				segment.IsCurrent ? UIConfiguration.ButtonColor : 
		        (segment.Passed ? UIConfiguration.EnabledCounterBackground : Color.Black);

	}
}
