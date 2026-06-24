using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using CsharpBooru.SQL;
using CsharpBooru.ViewModels;

namespace CsharpBooru.Component;
public class Tag_Component (int idTag) {

	private readonly Tag? tag = TagsManager.GetTag(idTag);

	const int s = 28;

	public StackPanel Component () {
		StackPanel sp = new() {
			Orientation = Orientation.Horizontal,
			Height = s,
		};

		Button btnDescription = Description()!;
		if(btnDescription != null) sp.Children.Add(btnDescription);

		sp.Children.Add(Name());
		sp.Children.Add(Count());

		return sp;
	}

	private Button? Description () {
		if (tag == null || tag.Description == null || tag.Description.Length == 0) return null;
		Button btn = new() {
			Height = s,
			Background = Brushes.Transparent,
			Content = new TextBlock {
				Text = "?",
				FontSize = 12,
				Foreground = Brushes.Blue,
				TextAlignment = TextAlignment.Center,
				TextWrapping = TextWrapping.Wrap,
				},
		};

		btn.Click += (_, _) => MainWindowViewModel.Main?.Wiki(tag.Id);

		return btn;
	}

	public Button Name () {
		Button btn = new() {
			Height = s,
			Margin = new Thickness(0, 0, 3, 0),
			Background = Brushes.Transparent,
			Content = new TextBlock() {
				Text = tag?.Name,
				FontSize = 12,
				TextWrapping = TextWrapping.Wrap,
				Foreground = tag?.SpecificTags switch {
					"Tag" => Brushes.Blue,
					"Artist" => Brushes.OrangeRed,
					"Character" => Brushes.Green,
					"Copyright" => Brushes.Magenta,
					"Species" => Brushes.Red,
					_ => Brushes.Black
				},
			}
		};

		btn.Click += (_, _) => {
			SearchSQL.querySearch = tag?.Name ?? "";
			MainWindowViewModel.Main?.PostGrid();
		};

		return btn;
	}

	public TextBlock Count () => new() {
		Height = s,
		Margin = new Thickness(0, 10, 3, 0),
		Text = tag?.Count + "",
		FontSize = 12,
		TextWrapping = TextWrapping.Wrap,
		Foreground = Brushes.Gray,
	};
}
