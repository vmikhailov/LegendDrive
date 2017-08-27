using System;
using System.Collections.Generic;
using System.Linq;
using LegendDrive.Counters.Interfaces;
using LegendDrive.Messaging;
using LegendDrive.Model;
using LegendDrive.Model.ViewModel;
using Xamarin.Forms;

namespace LegendDrive
{
	public class CountersPanelBuilder : IViewBuilder
	{
		GlobalModel model;
		public CountersPanelBuilder(GlobalModel model)
		{
			this.model = model;
		}

		public View Build()
		{
			return BuildCountersGroupPanel();
		}

		private View BuildCountersGroupPanel()
		{
			var grid = new Grid();
			grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
			grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(0) });
			foreach (var g in model.CountersGroup.Groups)
			{
				//var rowCount = (c.Count - 1) / 8 + 1;
				grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(g.Weight, GridUnitType.Star) });
				grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(0) });
			}
			grid.ColumnSpacing = 0;
			grid.RowSpacing = 0;

			int i = 0;
			grid.Children.Add(BuildSplitter(), 0, i++);
			foreach (var c in model.CountersGroup.Groups)
			{
				grid.Children.Add(BuildCountersPanel(c.Counters), 0, i++);
				grid.Children.Add(BuildSplitter(), 0, i++);
			}
			return grid;
		}

		private View BuildSplitter()
		{
			return new BoxView() { Color = UIConfiguration.ButtonColor, WidthRequest = -1, HeightRequest = 0 };
		}

		private View BuildCountersPanel(IEnumerable<IRaceCounter> counters)
		{
			return CountersPanelBuilderHelper.BuildAdaptivePanel(
				counters.Select(x => BuildCounterPanel(new RaceCounterViewModel(x))).ToList());
		}

		private View BuildCounterPanel(RaceCounterViewModel counter)
		{
			var tapRecognizer = new TapGestureRecognizer();
			tapRecognizer.NumberOfTapsRequired = 2;
			tapRecognizer.Command = counter.TapCommand;

			var labelHeader = new Label()
			{
				TextColor = UIConfiguration.CounterColors[CounterColor.White],
				HorizontalOptions = LayoutOptions.StartAndExpand,
				Margin = new Thickness(5, 0, 0, 0),
				FontSize = 15,
				FontFamily = "OpenSans",
				BindingContext = counter,
			}.With(x =>
			{
				x.SetBinding(Label.TextProperty, "Name");
				x.SetBinding(Label.TextColorProperty, "Color");
			});

			var lableValue = new Label()
			{
				TextColor = UIConfiguration.CounterColors[CounterColor.White],
				HorizontalOptions = LayoutOptions.Center,
				VerticalOptions = LayoutOptions.Center,
				Margin = new Thickness(0, 10, 0, 0),
				FontFamily = "OpenSans",
				BindingContext = counter
			}.With(x =>
			{
				x.SetBinding(Label.TextColorProperty, "Color");
				x.SetBinding(Label.TextProperty, "Value");
				x.SetBinding(Label.FontSizeProperty, "FontSize");
				x.SetBinding(Label.FontAttributesProperty, "FontAttributes");
			});

			var labelDebug = new Label()
			{
				TextColor = UIConfiguration.CounterColors[CounterColor.White],
				HorizontalOptions = LayoutOptions.EndAndExpand,
				Margin = new Thickness(5, 0, 0, 0),
				FontSize = 12,
				FontFamily = "OpenSans",
				IsVisible = model.ShowDebugInfo,
				//BackgroundColor = Color.Red,
				BindingContext = counter,
			}.With(x =>
			{
				x.SetBinding(Label.TextProperty, "DebugString");
			});

			var relative = new RelativeLayout()
			{
				Padding = new Thickness(0, 0),
				Margin = new Thickness(2),
				HorizontalOptions = LayoutOptions.FillAndExpand,
				VerticalOptions = LayoutOptions.FillAndExpand,
				BindingContext = counter
			};

			relative.SetBinding(VisualElement.BackgroundColorProperty, "BackgroundColor");

			relative.Children.Add(labelHeader,
								  Constraint.RelativeToParent((parent) => 0),
								  Constraint.RelativeToParent((parent) => 0));

			relative.Children.Add(labelDebug,
			                      Constraint.Constant(0),
								  Constraint.RelativeToParent((parent) => parent.Height - labelDebug.Height - 2),
			                      Constraint.RelativeToParent((parent) => parent.Width - 2));
			                      //Constraint.Constant(50));

			relative.Children.Add(lableValue,
								  Constraint.RelativeToParent((parent) => 0),
								  Constraint.RelativeToParent((parent) => 0),
								  Constraint.RelativeToParent((parent) => parent.Width),
								  Constraint.RelativeToParent((parent) => parent.Height));
			relative.GestureRecognizers.Add(tapRecognizer);

			//var frame = new Frame()
			//{
			//	Padding = new Thickness(2),
			//	Margin = new Thickness(2),
			//	HorizontalOptions = LayoutOptions.FillAndExpand,
			//	VerticalOptions = LayoutOptions.FillAndExpand,
			//	Content = relative,
			//	BindingContext = counter
			//};
			//frame.SetBinding(VisualElement.BackgroundColorProperty, FuncBinding.Create(".", counterColorFunc));

			return relative;
		}
	}
}

