using System;
using System.Collections.Generic;
using System.Linq;
using LegendDrive.Counters;
using LegendDrive.Model;
using LegendDrive.Model.RaceModel;
using Xamarin.Forms;
using LegendDrive.Messaging;
using LegendDrive.Counters.Interfaces;
using LegendDrive.Model.ViewModel;
using System.Collections.ObjectModel;

namespace LegendDrive
{
	public class LeftPanelBuilder : BaseBindingObject<LeftPanelBuilder>, IViewBuilder
	{
		GlobalModel model;
		public LeftPanelBuilder(GlobalModel model)
		{
			this.model = model;
		}

		public View Build()
		{
			return BuildLeftPanel();
		}

		private View BuildLeftPanel()
		{
			var normalCommand = new Command((param) => { MessagingHub.Send(QueueType.Click, (GlobalCommand)param); });

			var btnFunc = new Func<string, GlobalCommand, Button>((text, cmdCode) =>
			{
				var btn = new Button()
				{
					Text = text,
					WidthRequest = UIConfiguration.LargeButtonWidth,
					HeightRequest = UIConfiguration.LargeButtonHeight,
					BackgroundColor = UIConfiguration.ButtonColor,
					FontSize = UIConfiguration.LargeButtonFontSize * 2,
					BindingContext = model.Race,
					Margin = new Thickness(2, 0, 2, 0),
					BorderRadius = 0,
					TextColor = UIConfiguration.CounterColors[CounterColor.White],
					CommandParameter = cmdCode
				};
				btn.Command = normalCommand;
				return btn;
			});


			var startFinishButton = btnFunc(null, GlobalCommand.StartFinish);
			startFinishButton.SetBinding(Button.TextProperty, 
			                             FuncBinding.Create<bool, string>("IsRunning", x => x ? "Finish" : "Start"));

			var resetButton = btnFunc("Reset", GlobalCommand.ResetAll);

			var clearButton = btnFunc("Clear", GlobalCommand.ClearAll);
			clearButton.SetBinding(VisualElement.IsEnabledProperty, FuncBinding.Create<bool, bool>("IsRunning", x => !x));

			var gpsButton = btnFunc("GPS", GlobalCommand.GPSReset);

			var backButton = btnFunc("Back", GlobalCommand.Back);
			backButton.SetBinding(VisualElement.IsEnabledProperty, "CanGoBack");

			var turnButton = btnFunc("Turn", GlobalCommand.Turn);
			turnButton.WidthRequest = UIConfiguration.LargeButtonWidth * 3;
			turnButton.SetBinding(VisualElement.IsEnabledProperty, "IsRunning");
			turnButton.SetBinding(Button.TextProperty,
			                      FuncBinding.Create<ObservableCollection<TurnInfo>, string>(
				                      "Turns", x => x.Count > 1 ? $"Turn ({x.Count - 1})" : "Turn"));


			var deleteButton = btnFunc("Del", GlobalCommand.DelSegment);
			deleteButton.WidthRequest = UIConfiguration.LargeButtonWidth;
			deleteButton.SetBinding(VisualElement.IsEnabledProperty, "CanDelete");

			var grid = new Grid();
			grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(UIConfiguration.LargeButtonHeight) });
			grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
			grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(UIConfiguration.LargeButtonHeight) });
			grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
			grid.ColumnSpacing = 0;
			grid.RowSpacing = 2;

			grid.Children.Add(BuildButtonsPanel(startFinishButton, resetButton, clearButton, gpsButton, backButton), 0, 0);

			grid.Children.Add(new CountersPanelBuilder(model).Build(), 0, 1);

			grid.Children.Add(BuildButtonsPanel(turnButton, deleteButton), 0, 2);

			return grid;
		}

		private View BuildButtonsPanel(params View[] views)
		{
			var grid = new Grid();
			grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(UIConfiguration.LargeButtonHeight) });
			int i = 0;
			var totalWidth = views.Sum(x => x.WidthRequest);
			foreach(var view in views)
			{
				grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(view.WidthRequest/totalWidth, GridUnitType.Star) });
				grid.Children.Add(view, i++, 0);
			}
			grid.ColumnSpacing = 0;
			grid.RowSpacing = -2;
			return grid;
		}
	}
}

