using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;
using Avalonia.Media;
using CsharpBooru.SQL;
using System;
using System.Linq;


namespace CsharpBooru.Component;
public class Suggestions_Component {

	public static void AddSuggestions (TextBox tb, Panel container) {
		Popup suggestions = Suggestions_Popup(tb);

		tb.TextChanged += (_, _) => UpdateSuggestions(tb, suggestions);
		tb.SizeChanged += (_, _) => suggestions.Width = tb.Bounds.Width;
		container.Children.Add(suggestions);
	}

	private static Popup Suggestions_Popup (TextBox tb) => new Popup {
		PlacementTarget = tb,
		HorizontalOffset = 0,
		VerticalOffset = 2,
		Width = tb.Bounds.Width,
		MinWidth = 200,
		MaxHeight = 240,
		IsLightDismissEnabled = true,
		Child = new Border {
			Background = new SolidColorBrush(Color.FromRgb(239, 239, 239)),
			BorderBrush = Brushes.Black,
			BorderThickness = new Avalonia.Thickness(1),
			Child = new StackPanel(),
		}
	};

	private static void UpdateSuggestions (TextBox tb, Popup popup) {
		string text = tb.Text ?? "";
		int lastSpace = text.LastIndexOf(' ');
		string currentTag = text[(lastSpace + 1)..];
		if (currentTag.Length == 0 || popup.Child is not Border { Child: StackPanel list }) {
			popup.IsOpen = false;
			return;
		}

		popup.IsOpen = false;
		list.Children.Clear();

		var matches = TagsManager.GetAllTags()
			.Where(tag => tag.Name.StartsWith(currentTag, StringComparison.OrdinalIgnoreCase))
			.OrderBy(tag => tag.Name)
			.Take(10)
			.ToList();

		foreach (Tag tag in matches) {
			TextBlock nameTextBlock = new() {
				Text = tag.Name,
				HorizontalAlignment = HorizontalAlignment.Stretch,
				Foreground = tag.SpecificTags switch {
					"Tag" => Brushes.Blue,
					"Artist" => Brushes.OrangeRed,
					"Copyright" => Brushes.Magenta,
					"Character" => Brushes.Green,
					"Species" => Brushes.Red,
					_ => Brushes.Black
				},
			};

			TextBlock countTextBlock = new() {
				Text = "" + tag.Count,
				HorizontalAlignment = HorizontalAlignment.Right,
				Foreground = Brushes.Black,
			};

			Grid grid = new() {
				ColumnDefinitions = {
					new ColumnDefinition(GridLength.Star),
					new ColumnDefinition(new GridLength(50))
				}
			};

			Grid.SetColumn(nameTextBlock, 0);
			Grid.SetColumn(countTextBlock, 1);
			grid.Children.Add(nameTextBlock);
			grid.Children.Add(countTextBlock);

			Button suggestion = new() {
				HorizontalContentAlignment = HorizontalAlignment.Left,
				HorizontalAlignment = HorizontalAlignment.Stretch,
				Background = Brushes.Transparent,
				Content = grid
			};

			suggestion.Click += (_, _) => OnSuggestionSelected(tb, popup, tag);

			list.Children.Add(suggestion);
		}

		popup.IsOpen = matches.Count > 0;
	}

	private static void OnSuggestionSelected (TextBox tb, Popup popup, Tag tag) {
		string currentText = tb.Text ?? "";
		int prefixLength = currentText.LastIndexOf(' ') + 1;
		tb.Text = currentText[..prefixLength] + tag.Name + " ";
		tb.CaretIndex = tb.Text.Length;
		popup.IsOpen = false;
	}

}
