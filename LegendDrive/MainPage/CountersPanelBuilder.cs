using System;
using System.Collections.Generic;
using System.Linq;
using LegendDrive.Counters.Interfaces;
using LegendDrive.Model;
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
			foreach (var c in model.CountersGroup.Counters.Values)
			{
				//var rowCount = (c.Count - 1) / 8 + 1;
				grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
				grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(0) });
			}
			grid.ColumnSpacing = 0;
			grid.RowSpacing = 0;

			int i = 0;
			grid.Children.Add(BuildSplitter(), 0, i++);
			foreach (var c in model.CountersGroup.Counters.Values)
			{
				grid.Children.Add(BuildCountersPanel(c), 0, i++);
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
			return CountersPanelBuilderHelper.BuildAdaptivePanel(counters.Select(x => BuildCounterPanel(x)).ToList());
		}

		private View BuildCounterPanel(IRaceCounter counter)
		{
			Func<IRaceCounter, Color> counterColorFunc = x => 
			{
				if (!x.IsRunning) return UIConfiguration.DisabledCounterBorder;
				if (x.IsCritical) return UIConfiguration.CriticalCounterBorder;
				if (x.IsImportant) return UIConfiguration.ImportantCounterBorder;
				return UIConfiguration.CounterColors[x.Color];
			};

			Func<CounterSize, double> counterFontSizeFunc = x =>
			{
				return UIConfiguration.CounterFontSizes[x];
			};

			var l1 = new Label()
			{
				TextColor = Color.White,
				HorizontalOptions = LayoutOptions.StartAndExpand,
				Margin = new Thickness(5, 0, 0, 0),
				FontSize = 15,
				FontFamily = "OpenSans",
				BindingContext = counter,
			};
			l1.SetBinding(Label.TextProperty, "Name");
			l1.SetBinding(Label.TextColorProperty,
						  FuncBinding.Create(".", counterColorFunc));

			var l2 = new Label()
			{
				TextColor = Color.White,
				HorizontalOptions = LayoutOptions.Center,
				VerticalOptions = LayoutOptions.Center,
				Margin = new Thickness(0,10,0,0),
				FontFamily = "OpenSans",
				BindingContext = counter
			};
			l2.SetBinding(Label.TextColorProperty, 
			              FuncBinding.Create(".", counterColorFunc));
			l2.SetBinding(Label.TextProperty, 
			              new Binding(".", converter: new CounterToStringConverter()));
			l2.SetBinding(Label.FontSizeProperty, FuncBinding.Create("Size", counterFontSizeFunc));
			l2.SetBinding(Label.FontAttributesProperty, 
			              FuncBinding.Create<CounterSize, FontAttributes>("Size", 
                              x => x >= CounterSize.XXL ? FontAttributes.Bold : FontAttributes.None));

			var relative = new RelativeLayout()
			{
				Padding = new Thickness(0, 0),
				HorizontalOptions = LayoutOptions.FillAndExpand,
				VerticalOptions = LayoutOptions.FillAndExpand,
				WidthRequest = -1,
				HeightRequest = -1,
				BindingContext = counter,
			};
			relative.SetBinding(VisualElement.BackgroundColorProperty,
							 	FuncBinding.
								Create<bool, Color>("IsRunning", (x) => x ?
									   UIConfiguration.EnabledCounterBackground :
									   UIConfiguration.DisabledCounterBackground));

			relative.Children.Add(l1,
								  Constraint.RelativeToParent((parent) => 0),
								  Constraint.RelativeToParent((parent) => 0));

			relative.Children.Add(l2,
								  Constraint.RelativeToParent((parent) => 0),
								  Constraint.RelativeToParent((parent) => 0),
								  Constraint.RelativeToParent((parent) => parent.Width),
								  Constraint.RelativeToParent((parent) => parent.Height));


			var frame = new Frame()
			{
				Padding = new Thickness(2),
				Margin = new Thickness(2),
				HorizontalOptions = LayoutOptions.FillAndExpand,
				VerticalOptions = LayoutOptions.FillAndExpand,
				Content = relative,
				BindingContext = counter

			};
			frame.SetBinding(VisualElement.BackgroundColorProperty, FuncBinding.Create(".", counterColorFunc));

			return frame;
		}
	}
}

