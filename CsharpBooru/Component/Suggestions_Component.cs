using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;
using Avalonia.Media;
using CsharpBooru.SQL;
using System;
using System.Linq;


namespace CsharpBooru.Component;
public class Suggestions_Component {
	private static readonly string[] RatingSuggestions = [
		"rating:none",
		"rating:safe",
		"rating:questionable",
		"rating:explicit",
		"rating:borderline",
	];
	private static readonly string[] ExtensionSuggestions = [
		"extension:png",
		"extension:jpg",
		"extension:jpeg",
		"extension:ico",
		"extension:webp",
		"extension:tiff",
		"extension:tif",
		"extension:gif",
		"extension:mp4",
		"extension:avi",
		"extension:webm",
		"extension:mkv",
	];

	public static void AddSuggestions (TextBox tb, Panel container) {
		Popup suggestions = Suggestions_Popup(tb);

		tb.TextChanged += (_, _) => UpdateSuggestions(tb, suggestions);
		tb.SizeChanged += (_, _) => suggestions.Width = tb.Bounds.Width;
		container.Children.Add(suggestions);
	}

	private static Popup Suggestions_Popup (TextBox tb) => new() {
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
		string[]? commandSuggestions = null;
		if ("rating".StartsWith(currentTag, StringComparison.OrdinalIgnoreCase) ||
			currentTag.StartsWith("rating:", StringComparison.OrdinalIgnoreCase)) {
			commandSuggestions = RatingSuggestions;
		} else if ("extension".StartsWith(currentTag, StringComparison.OrdinalIgnoreCase) ||
			currentTag.StartsWith("extension:", StringComparison.OrdinalIgnoreCase)) {
			commandSuggestions = ExtensionSuggestions;
		}

		if (commandSuggestions != null) {
			var commandMatches = commandSuggestions
				.Where(command => command.StartsWith(currentTag, StringComparison.OrdinalIgnoreCase))
				.ToList();

			foreach (string command in commandMatches) {
				Button suggestion = new() {
					HorizontalContentAlignment = HorizontalAlignment.Left,
					HorizontalAlignment = HorizontalAlignment.Stretch,
					Background = Brushes.Transparent,
					Content = command
				};

				suggestion.Click += (_, _) => OnSuggestionSelected(tb, popup, command);
				list.Children.Add(suggestion);
			}

			popup.IsOpen = commandMatches.Count > 0;
			return;
		}

		var matches = TagsManager.GetAllTags()
			.Where(tag => tag.Name.StartsWith(currentTag, StringComparison.OrdinalIgnoreCase))
			.OrderBy(tag => tag.Name)
			.Take(10)
			.ToList();

		foreach (Tag tag in matches) {
			TextBlock nameTextBlock = new() {
				Text = tag.Name,
				HorizontalAlignment = HorizontalAlignment.Stretch,
				TextDecorations = tag.Obsolete == "1" ? TextDecorations.Strikethrough : null,
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

			suggestion.Click += (_, _) => OnSuggestionSelected(tb, popup, tag.Name);

			list.Children.Add(suggestion);
		}

		popup.IsOpen = matches.Count > 0;
	}

	private static void OnSuggestionSelected (TextBox tb, Popup popup, string value) {
		string currentText = tb.Text ?? "";
		int prefixLength = currentText.LastIndexOf(' ') + 1;
		tb.Text = currentText[..prefixLength] + value + " ";
		tb.CaretIndex = tb.Text.Length;
		popup.IsOpen = false;
	}

}
