using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using CsharpBooru.SQL;
using CsharpBooru.ViewModels;
using System.Linq;

namespace CsharpBooru.Component;
public class Tag_Component (int idTag) {

	private readonly Tag? tag = TagsManager.GetTag(idTag);

	private const int sizeHeight = 22;

	public StackPanel Component () {
		StackPanel sp = new() {
			Orientation = Orientation.Horizontal,
			Height = sizeHeight,
		};

		Button btnDescription = Description()!;
		if(btnDescription != null) sp.Children.Add(btnDescription);

		sp.Children.Add(Name());

		foreach (int aliasId in ConvertUtils.StringToIntList(tag?.Aliases ?? "")) {
			Tag? alias = TagsManager.GetTag(aliasId);
			if (alias == null) continue;

			sp.Children.Add(new TextBlock {
				Height = sizeHeight,
				Margin = new Thickness(2, 0, 2, 0),
				Text = "↔",
				VerticalAlignment = VerticalAlignment.Center,
				Foreground = Brushes.Gray,
			});
			sp.Children.Add(AliasName(alias));
		}

		sp.Children.Add(Count());

		return sp;
	}

	private static Button AliasName (Tag alias) {
		Button btn = new() {
			Height = sizeHeight,
			Margin = new Thickness(0, 0, 3, 0),
			Background = Brushes.Transparent,
			Content = new TextBlock {
				Text = alias.Name.Replace('_', ' '),
				FontSize = 12,
				TextWrapping = TextWrapping.Wrap,
				Foreground = alias.SpecificTags switch {
					"Tag" => Brushes.Blue,
					"Artist" => Brushes.OrangeRed,
					"Character" => Brushes.Green,
					"Copyright" => Brushes.Magenta,
					"Species" => Brushes.Red,
					_ => Brushes.Black
				},
				TextDecorations = alias.Obsolete == "1" ? TextDecorations.Strikethrough : null,
				ClipToBounds = false,
			}
		};

		btn.Click += (_, _) => {
			SearchSQL.querySearch = alias.Name;
			MainWindowViewModel.Main?.PostGrid();
		};
		btn.ContextMenu = ContextMenu(alias.Id);

		return btn;
	}

	private Button? Description () {
		if (tag == null || tag.Description == null || tag.Description.Length == 0) return null;
		Button btn = new() {
			Height = sizeHeight,
			Background = Brushes.Transparent,
			Content = new TextBlock {
				Text = "?",
				FontSize = 12,
				Foreground = Brushes.Blue,
				TextAlignment = TextAlignment.Center,
				TextWrapping = TextWrapping.Wrap,
				ClipToBounds = false,
			},
		};

		btn.Click += (_, _) => MainWindowViewModel.Main?.Wiki(tag.Id);

		return btn;
	}

	public Button Name () {
		Button btn = new() {
			Height = sizeHeight,
			Margin = new Thickness(0, 0, 3, 0),
			Background = Brushes.Transparent,
			Content = new TextBlock() {
				Text = tag?.Name.Replace('_',' '),
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
				TextDecorations = tag?.Obsolete == "1" ? TextDecorations.Strikethrough : null,
				ClipToBounds = false,
			}
		};

		btn.Click += (_, _) => {
			SearchSQL.querySearch = tag?.Name ?? "";
			MainWindowViewModel.Main?.PostGrid();
		};

		btn.ContextMenu = ContextMenu(tag!.Id);

		return btn;
	}

	public TextBlock Count () => new() {
		Height = sizeHeight,
		Margin = new Thickness(0, 10, 3, 0),
		Text = ((tag?.Count ?? 0) + ConvertUtils.StringToIntList(tag?.Aliases ?? "")
			.Sum(TagsManager.GetTagUsage)).ToString(),
		FontSize = 12,
		TextWrapping = TextWrapping.Wrap,
		Foreground = Brushes.Gray,
		ClipToBounds = false,
	};

	public static ContextMenu ContextMenu (int id) {

		var openWikiItem = new MenuItem {
			Header = "Open Wiki"
		}; openWikiItem.Click += (_, _) => MainWindowViewModel.Main?.Wiki(id);

		var relatedPostsItem = new MenuItem {
			Header = "Check out the related posts."
		}; relatedPostsItem.Click += (_, _) => {
			SearchSQL.querySearch = TagsManager.GetTag(id).Name;
			MainWindowViewModel.Main?.PostGrid();
		};
		var editItem = new MenuItem {
			Header = "Edit Tag"
		}; editItem.Click += (_, _) => {
			MainWindowViewModel.Main?.EditWiki(id);
		};
		var deleteItem = new MenuItem {
			Header = "Delete Tag",
			Foreground = Brushes.Red
		}; deleteItem.Click += (_, _) => {
			TagsManager.RemoveTag(id);
			MainWindowViewModel.Main?.TagPage();
		};

		ContextMenu cm = new() {
			Items = {
				openWikiItem, relatedPostsItem,
				new Separator(),
				editItem,
			}
		};

		bool hasAliases = ConvertUtils.StringToIntList(TagsManager.GetTag(id)?.Aliases ?? "").Count > 0;
		if (TagsManager.GetTagUsage(id) == 0 && !hasAliases) {
			cm.Items.Add(new Separator());
			cm.Items.Add(deleteItem);
		}

		return cm;
	}
}
