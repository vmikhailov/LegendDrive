using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using LegendDrive.Model;
using LegendDrive.Model.RaceModel;
using Xamarin.Forms;

namespace LegendDrive
{
	public class RightPanelBuilder : IViewBuilder
	{
		GlobalModel model;
		public RightPanelBuilder(GlobalModel model)
		{
			this.model = model;
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
				ItemsSource = model.Race.Segments,
				RowHeight = UIConfiguration.ButtonHeight / 2,
				HasUnevenRows = true,
				//SeparatorVisibility = SeparatorVisibility.None,
				//BackgroundColor = Color.Red,
				ItemTemplate = new DataTemplate(() =>
				{
					// Create views with bindings for displaying each property.
					Func<string, int, LayoutOptions, View> newLabel = (prop, font, align) =>
					{
						var l = new Label()
						{
							TextColor = Color.White,
							HorizontalOptions = align,
							FontSize = font,
							FontFamily = "OpenSans"
						};
						l.SetBinding(Label.TextProperty, prop);
						l.SetBinding(Label.FontSizeProperty,
									 FuncBinding.Create<bool, double>("IsCurrent", x => x ? font*1.5 : font));
						//l.SetBinding(Label.TextColorProperty,
						//             FuncBinding.Create<bool, Color>("IsCurrent", x => x ? Color.Red : Color.White));
						//l.SetBinding(Label.BackgroundColorProperty,
						//             FuncBinding.Create<bool, Color>("IsCurrent", x => x ? UIConfiguration.ButtonColor : Color.White));
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
						BackgroundColor = Color.Red.WithSaturation(0.1),
						Orientation = StackOrientation.Horizontal,
						Children =
						{
							//newLabel("No", segmentListNoFontSize, LayoutOptions.Center),
							centerStack(newLabel("No", UIConfiguration.SegmentListNoFontSize, LayoutOptions.CenterAndExpand), UIConfiguration.SegmentListNoSize),
							newBox(),
							centerStack(newLabel("Distance", UIConfiguration.SegmentListDistanceFontSize, LayoutOptions.CenterAndExpand), UIConfiguration.SegmentListDistanceSize),
							newBox(),
							centerStack(newLabel("Speed", UIConfiguration.SegmentListSpeedFontSize, LayoutOptions.CenterAndExpand), UIConfiguration.SegmentListSpeedSize),
							newBox(),
							centerStack(newLabel("TimeoutStr", UIConfiguration.SegmentListTimeoutFontSize, LayoutOptions.CenterAndExpand), UIConfiguration.SegmentListTimeoutSize),
						},
					};
					stack.SetBinding(VisualElement.BackgroundColorProperty,
					                 FuncBinding.Create<RaceSegment, Color>(".", x =>
									 {
										 if (x == null) return Color.Black;
										 if (x.IsCurrent) return UIConfiguration.ButtonColor;
										 return x.Passed ? UIConfiguration.EnabledCounterBackground : Color.Black;
									 }));

					return new ViewCell { View = stack };
				}),
				BindingContext = model.Race
			};
			lv.SeparatorVisibility = SeparatorVisibility.None;

			lv.SetBinding(ListView.SelectedItemProperty,
						  new Binding("CurrentSegment", BindingMode.OneWay));

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
					var item = ni.OfType<RaceSegment>()
					              .Union(ni.OfType<IEnumerable<RaceSegment>>().SelectMany(y => y))
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
				TextColor = UIConfiguration.ButtonColor,
				VerticalTextAlignment = TextAlignment.Center,
				BindingContext = model.Numpad
			};
			lbl.SetBinding(Label.TextProperty, new Binding("NewDataText", BindingMode.TwoWay));

			var grid = new Grid();
			//grid.BackgroundColor = Xamarin.Forms.Color.Blue;

			grid.Padding = new Thickness(0);

			grid.ColumnSpacing = -1;
			grid.RowSpacing = -2;
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
				FontSize = size,
				BorderRadius = 0,
				Margin = new Thickness(-2, -4),
				BorderColor = Color.Red,
				BorderWidth = 5,
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

