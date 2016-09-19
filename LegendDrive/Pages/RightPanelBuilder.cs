using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using LegendDrive.Counters.Interfaces;
using LegendDrive.Model;
using LegendDrive.Model.RaceModel;
using LegendDrive.Collections;
using Xamarin.Forms;
using LegendDrive.Model.ViewModel;

namespace LegendDrive
{
	public class RightPanelBuilder : IViewBuilder
	{
		GlobalModel model;
		RaceViewModel racevm;
		public RightPanelBuilder(GlobalModel model)
		{
			this.model = model;
			racevm = new RaceViewModel(model.Race);
		}

		public View Build()
		{
			var grid = new Grid();
			grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
			grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(UIConfiguration.ButtonHeight * 5.6) });
			grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

			grid.Children.Add(BuildIntervalList(), 0, 0);
			grid.Children.Add(BuildNumPad(), 0, 1);
			grid.RowSpacing = 0;

			return grid;
		}


		private View BuildIntervalList()
		{
			var lv = new ListView()
			{
				ItemsSource = racevm.Segments,
				RowHeight = UIConfiguration.ButtonHeight / 2,
				HasUnevenRows = true,
				//SeparatorVisibility = SeparatorVisibility.None,
				//BackgroundColor = Color.Red,
				ItemTemplate = new DataTemplate(() =>
				{
					// Create views with bindings for displaying each property.
					Func<string, string, LayoutOptions, View> newLabel = (prop, fontProp, align) =>
					{
						var l = new Label()
						{
							TextColor = UIConfiguration.CounterColors[CounterColor.White],
							HorizontalOptions = align,
							FontFamily = "OpenSans"
						};
						l.SetBinding(Label.TextProperty, prop);
						l.SetBinding(Label.FontSizeProperty, fontProp);
						return l;
					};

					Func<View> newBox = () =>
						new BoxView()
						{
							Color = UIConfiguration.ButtonColor,
							WidthRequest = 2,
							HeightRequest = 20
						};

					Func<View, int, View> centerStack = (view, size) =>
						new StackLayout
						{
							VerticalOptions = LayoutOptions.Center,
							HorizontalOptions = LayoutOptions.Center,
							Spacing = 0,
							Padding = new Thickness(0, 0, 2, 0),
							Children = { view },
							WidthRequest = size
						};

					var stack = new StackLayout
					{
						Padding = new Thickness(2, 0),
						Spacing = 0,
						Orientation = StackOrientation.Horizontal,
						Children =
						{
							centerStack(newLabel(nameof(RaceSegmentViewModel.No), 
							                     nameof(RaceSegmentViewModel.FontSizeNo),
							                     LayoutOptions.CenterAndExpand), UIConfiguration.SegmentListNoSize),
							newBox(),
							centerStack(newLabel(nameof(RaceSegmentViewModel.Length), 
							                     nameof(RaceSegmentViewModel.FontSizeLength),
							                     LayoutOptions.CenterAndExpand), UIConfiguration.SegmentListLengthSize),
							newBox(),
							centerStack(newLabel(nameof(RaceSegmentViewModel.Speed), 
							                     nameof(RaceSegmentViewModel.FontSizeSpeed), 
							                     LayoutOptions.CenterAndExpand), UIConfiguration.SegmentListSpeedSize),
							newBox(),
							centerStack(newLabel(nameof(RaceSegmentViewModel.Timeout), 
							                     nameof(RaceSegmentViewModel.FontSizeTimeout), 
							                     LayoutOptions.CenterAndExpand), UIConfiguration.SegmentListTimeoutSize),
						},
					};
					stack.SetBinding(VisualElement.HeightRequestProperty, nameof(RaceSegmentViewModel.ListItemHeight));
					stack.SetBinding(VisualElement.BackgroundColorProperty, nameof(RaceSegmentViewModel.BackgroundColor));
					                
					return new ViewCell { View = stack };
				}),
				BindingContext = racevm
			};
			//lv.SeparatorVisibility = SeparatorVisibility.None;
			//lv.SetBinding(ListView.ItemsSourceProperty, nameof(RaceViewModel.Segments));
			lv.SetBinding(ListView.SelectedItemProperty, nameof(RaceViewModel.CurrentSegment));

			lv.ItemSelected += (sender, e) =>
			{
				if (e.SelectedItem != null)
				{
					lv.ScrollTo(e.SelectedItem, ScrollToPosition.Center, true);
				}
			};

			var coll = lv.ItemsSource as INotifyCollectionChanged;

			coll.CollectionChanged += (sender, e) =>
			{
				if (e.Action == NotifyCollectionChangedAction.Add)
				{
					var ni = e.NewItems;
					var item = ni.OfType<RaceSegmentViewModel>()
					              .Union(ni.OfType<IEnumerable<RaceSegmentViewModel>>().SelectMany(y => y))
					              .LastOrDefault();
					if (item != null)
					{
						lv.ScrollTo(item, ScrollToPosition.MakeVisible, false);
					}
				}
			};


			//lv.BindingContext(ListView.
			return lv;
		}

		private View BuildNumPad()
		{
			var b1 = new BoxView()
			{
				Color = UIConfiguration.ButtonColor,
				WidthRequest = UIConfiguration.PanelWidth,
				HeightRequest = 2,

			};
			var b2 = new BoxView()
			{
				Color = UIConfiguration.ButtonColor,
				WidthRequest = UIConfiguration.PanelWidth,
				HeightRequest = 2
			};

			var lbl = new Label()
			{
				//HeightRequest = buttonHeight,
				FontSize = UIConfiguration.ButtonFontSize * 0.8,
				TextColor = UIConfiguration.CounterColors[CounterColor.White],
				Margin = new Thickness(5,0,0,0),
				VerticalTextAlignment = TextAlignment.Center,
				BindingContext = model.Numpad
			};
			lbl.SetBinding(Label.TextProperty, new Binding(nameof(NumpadModel.NewDataText), BindingMode.TwoWay));

			var grid = new Grid();
			//grid.BackgroundColor = Xamarin.Forms.Color.Blue;

			grid.Padding = new Thickness(0);

			grid.ColumnSpacing = 0;
			grid.RowSpacing = 0;
			var cfg = model.Numpad.NumpadConfiguration;

			for (int i = 0; i < cfg.X; i++)
			{
				grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(UIConfiguration.ButtonWidth) });
			}

			for (int i = 0; i < cfg.Y; i++)
			{
				grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(UIConfiguration.ButtonHeight) });
			}

			foreach (var b in cfg.Buttons)
			{
				grid.Children.Add(NewButton(b), b.X, b.Y);
			}

			var stack = new StackLayout
			{
				VerticalOptions = LayoutOptions.Center,
				HorizontalOptions = LayoutOptions.Center,
				Orientation = StackOrientation.Vertical,
				Spacing = 0,
				//BackgroundColor = Color.Red,
				Children = { b1, lbl, b2, grid }
			};

			return stack;
		}

		private View NewButton(NumpadButton b, double size = UIConfiguration.ButtonFontSize)
		{
			var btn = new Button()
			{
				Text = b.Text,
				BackgroundColor = UIConfiguration.ButtonColor,
				TextColor = UIConfiguration.CounterColors[CounterColor.White],
				FontSize = size,
				BorderRadius = 0,
				Margin = new Thickness(2, 2, 2, b.Y == 4 ? 0 : 2),
				HorizontalOptions = LayoutOptions.FillAndExpand,
				VerticalOptions = LayoutOptions.FillAndExpand
			};
			btn.Clicked += (sender, e) =>
			{
				b.ExecuteCommand(btn, b.Command);
			};
			return btn;
		}

	}
}

