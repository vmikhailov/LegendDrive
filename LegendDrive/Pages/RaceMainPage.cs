using LegendDrive.Messaging;
using LegendDrive.Model;
using Xamarin.Forms;

namespace LegendDrive
{

	public class RaceMainPage : ContentPage
	{
		public RaceMainPage(GlobalModel model)
		{
			var grid = new Grid();
			grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
			grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
			grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(2) });
			grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(UIConfiguration.PanelWidth) });
			grid.ColumnSpacing = 0;
			grid.RowSpacing = 0;

			grid.Children.Add(new LeftPanelBuilder(model).Build(), 0, 0);
			grid.Children.Add(new BoxView() { Color = UIConfiguration.ButtonColor, WidthRequest = 2, HeightRequest = -1 }, 1, 0);
			grid.Children.Add(new RightPanelBuilder(model).Build(), 2, 0);

			Content = grid;
			BackgroundColor = Color.Black;

			MessagingHub.Subscribe<GlobalCommand>(this, QueueType.AskConfirmation, (cmd) => ProcessCommand(cmd));
			MessagingHub.Subscribe<string>(this, QueueType.Gesture, (msg) => ShowMessage(msg));
			//Navigation.PushModalAsync(cp);
		}

		private async void ProcessCommand(GlobalCommand cmd)
		{
			if (cmd.Code == GlobalCommandCodes.AskConfirmation)
			{
				var answer = await DisplayAlert("Alert", cmd.Message, "Yes", "No");
				//var answer = await DisplayAlert("Alert", cmd.Message, "Yes", "No");
				if (answer)
				{
					MessagingHub.Send(QueueType.Confirmed, GlobalCommand.ReplyConfirmation(cmd.CommandToConfirm));
				}
				else
				{
					MessagingHub.Send(QueueType.Canceled, GlobalCommand.ReplyConfirmation(cmd.CommandToConfirm));
				}
			}
		}

		void ShowMessage(string msg)
		{
			DisplayAlert("Message", msg, "Cancel");
		}
	}
}
