using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Layout;
using CsharpBooru.SQL;

namespace CsharpBooru.Component;
public static class Search_Component {

	public static Button Component(Grid grid) {

		TextBox tb = TextBox_Component();
		Button btn = Button_Component();
		btn.Click += (_, e) => OnSearchChanged(tb, e);

		grid.MaxWidth = 910;
		if (grid.ColumnDefinitions.Count == 0) {
			grid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
			grid.ColumnSpacing = 10;
			grid.ColumnDefinitions.Add(new ColumnDefinition(100d, GridUnitType.Pixel));
		}

		Grid.SetColumn(tb, 0);
		Grid.SetColumn(btn, 1);

		grid.Children.Add(tb);
		grid.Children.Add(btn);

		return btn;
	}

	private static TextBox TextBox_Component () {
		TextBox tb = new() {
			PlaceholderText = "Search...",
			Text = SearchSQL.querySearch,

			MinWidth = 200, MinHeight = 30,
			Height = 30,
			MaxWidth = 800, MaxHeight = 30,
			

			HorizontalAlignment = HorizontalAlignment.Stretch,

		};

		return tb;
	}

	private static void OnSearchChanged (object? sender, RoutedEventArgs e) {
		if (sender is TextBox tb) {
			SearchSQL.querySearch = tb.Text ?? "";
			SearchSQL.SearchPosts(SearchSQL.querySearch);
		}
	}

	private static Button Button_Component () {
		Button btn = new() {
			Width = 100, Height = 40,
			HorizontalContentAlignment = HorizontalAlignment.Center, VerticalContentAlignment = VerticalAlignment.Center,
			
			Content = new TextBlock {
				Text = "Search",
				HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center
			}
		};

		return btn;
	}
}
