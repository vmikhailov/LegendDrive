using LegendDrive.Model;
using Xamarin.Forms;

namespace LegendDrive
{

	public class MainPage : ContentPage
	{
		public MainPage(GlobalModel model)
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

			var b = new Button
			{
				Text = "sdadasadsdasd",
				WidthRequest = 300,
				HeightRequest = 300
			};

			var s = new StackLayout()
			{
				Children =
				{
					b
				}
			};

			var cp = new ContentPage()
			{
				Content = b
			};
			b.Clicked += (sender, e) => { Navigation.PopModalAsync(true); };

			Navigation.PushModalAsync(cp);
		}
	}
}
