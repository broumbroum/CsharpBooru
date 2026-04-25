using Avalonia.Controls;
using Avalonia;
using Avalonia.Layout;
using CsharpBooru.SQL;

namespace CsharpBooru.Component;
public static class Search_Component {

	public static Button Component(ref StackPanel sp) {

		TextBox tb = TextBox_Component();
		Button btn = Button_Component();

		sp.Children.Add(tb);
		sp.Children.Add(btn);

		return btn;
	}

	private static TextBox TextBox_Component () {
		TextBox tb = new() {
			PlaceholderText = "Search...",
			Text = SearchSQL.querySearch,

			MinWidth = 200, MinHeight = 30,
			Width = 800, Height = 30,
			MaxHeight = 30,

			HorizontalAlignment = HorizontalAlignment.Stretch,

			Margin = new Thickness(0, 10, 10, 0),

		};

		tb.TextChanged += OnSearchChanged;

		return tb;
	}

	private static void OnSearchChanged (object? sender, TextChangedEventArgs e) {
		if (sender is TextBox tb) {
			SearchSQL.querySearch = tb.Text ?? "";
			SearchSQL.SearchPosts(SearchSQL.querySearch);
		}
	}

	private static Button Button_Component () {
		Button btn = new() {
			MaxWidth = 100, MaxHeight = 40,
			Width = 100, Height = 40,
			MinWidth = 100, MinHeight = 40,
			
			Margin = new Thickness(10, 10, 10, 0),

			HorizontalContentAlignment = HorizontalAlignment.Center, VerticalContentAlignment = VerticalAlignment.Center,
			
			Content = new TextBlock {
				Text = "Search",

				HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center
			}
		};

		return btn;
	}
}
