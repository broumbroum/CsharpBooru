using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Media;
using Avalonia.Layout;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using CsharpBooru.ViewModels;
using Avalonia;
using CsharpBooru.SQL;
using Avalonia.Data;
using System.Collections.Generic;
using CsharpBooru.Component;

namespace CsharpBooru.Views.Pages;

public partial class TagsListView : UserControl {

	public ObservableCollection<Tag> AllTags { get; set; } = new();
	public ObservableCollection<Tag> FilteredTags { get; set; } = new();

	private const int PageSize = 100;
	private int currentPage = 0;
	private int totalPages = 1;
	private List<Tag> currentResults = new();

	public TagsListView () {
		InitializeComponent();

		this.AttachedToVisualTree += (_, _) => {
			SetupColumns();

			AllTags = new ObservableCollection<Tag>(TagsManager.GetAllTags());

			DataGridControl.ItemsSource = FilteredTags;
			UpdateResults(AllTags);
			DataGridControl.Margin = new Thickness(25,10,25,25);
		};
	}

	#region Build UI
	private void SetupColumns () {
		if (DataGridControl.Columns.Count > 0) return;

		DataGridTemplateColumn idColumn = new() {
			Header = "ID",
			CanUserResize = true,
			CellTemplate = new FuncDataTemplate<Tag>((tag, _) => {
				TextBlock tb = new() {
					Text = tag.Id.ToString(),
					VerticalAlignment = VerticalAlignment.Center,
					HorizontalAlignment = HorizontalAlignment.Right,
					Margin = new Thickness(5, 0, 10, 0)
				};
				return tb;
			}),
		};

		// Name
		DataGridTemplateColumn nameColumn = new() {
			Header = "Name",
			SortMemberPath = "Name",
			CanUserResize = true,
			Width = new DataGridLength(250),
			CellTemplate = new FuncDataTemplate<Tag>((tag, _) => {

				TextBlock tb = new() {
					Text = tag.Name,
					Foreground = GetColorForName(tag.SpecificTags)
				};
				Button bt = new() {
					HorizontalAlignment = HorizontalAlignment.Stretch,
					Content = tb
				};
				bt.Click += (_, _) => {
					SearchSQL.querySearch = tag.Name;
					MainWindowViewModel.main.PostGrid();
				};
				return bt;
			})
		};

		//Category
		DataGridTextColumn categoryColumn = new() {
			Header = "Category",
			CanUserResize = true,
			Binding = new Binding {
				Path = "SpecificTags"
			}
		};
		DataGridTemplateColumn countColumn = new() {
			Header = "Count",
			CanUserResize = true,
			CellTemplate = new FuncDataTemplate<Tag>((tag, _) => {
				TextBlock tb = new() {
					Text = tag.Count.ToString(),
					VerticalAlignment = VerticalAlignment.Center,
					HorizontalAlignment = HorizontalAlignment.Right,
					Margin = new Thickness(5, 0, 10, 0)
				};
				return tb;
			}),
			SortMemberPath = "Count"
		};

		//Change Category
		DataGridTemplateColumn changeCategoryColumu = new() {
			Header = "Change Category",
			CanUserResize = true,

			CellTemplate = new FuncDataTemplate<Tag>((tag, _) => {

				Button tagB = ButtonChangeCategory("Tag");
				tagB.Click += (_, _) => {
					TagsManager.UpdateTag(new Tag(tag.Id, tag.Name, "Tag"));
					MainWindowViewModel.main.TagPage();
				};
				Button artistB = ButtonChangeCategory("Artist");
				artistB.Click += (_, _) => {
					TagsManager.UpdateTag(new Tag(tag.Id, tag.Name, "Artist"));
					MainWindowViewModel.main.TagPage();
				};
				Button characterB = ButtonChangeCategory("Character");
				characterB.Click += (_, _) => {
					TagsManager.UpdateTag(new Tag(tag.Id, tag.Name, "Character"));
					MainWindowViewModel.main.TagPage();
				};
				Button copyrightB = ButtonChangeCategory("Copyright");
				copyrightB.Click += (_, _) => {
					TagsManager.UpdateTag(new Tag(tag.Id, tag.Name, "Copyright"));
					MainWindowViewModel.main.TagPage();
				};
				Button speciesB = ButtonChangeCategory("Species");
				speciesB.Click += (_, _) => {
					TagsManager.UpdateTag(new Tag(tag.Id, tag.Name, "Species"));
					MainWindowViewModel.main.TagPage();
				};

				StackPanel sp = new() {
					Orientation = Orientation.Horizontal,
				};

				sp.Children.Add(tagB);
				sp.Children.Add(artistB);
				sp.Children.Add(characterB);
				sp.Children.Add(copyrightB);
				sp.Children.Add(speciesB);
				return sp;
			})
		};


		
		DataGridControl.Columns.Add(idColumn);
		DataGridControl.Columns.Add(nameColumn);
		DataGridControl.Columns.Add(categoryColumn);
		DataGridControl.Columns.Add(countColumn);
		DataGridControl.Columns.Add(changeCategoryColumu);
	}

	private void LoadPage () {
		FilteredTags.Clear();

		if (currentResults.Count == 0) {
			BuildPagination_Component.Component(PaginationTop, currentPage, totalPages, page => {
				currentPage = page;
				LoadPage();
			});
			return;
		}

		totalPages = (int)Math.Ceiling(currentResults.Count / (double)PageSize);

		int start = currentPage * PageSize;
		int end = start + PageSize;

		if (start < 0) start = 0;

		for (int i = start; i < end; i++) {
			if (i >= currentResults.Count) break;
			FilteredTags.Add(currentResults[i]);
		}

		BuildPagination_Component.Component(PaginationTop, currentPage, totalPages, page => {
			currentPage = page;
			LoadPage();
		});
		BuildPagination_Component.Component(PaginationDown, currentPage, totalPages, page => {
			currentPage = page;
			LoadPage();
		});
	}
	#endregion

	#region Search
	private void OnSearchChanged (object? sender, TextChangedEventArgs e) {
		if (sender is TextBox tb) SearchByName(tb.Text ?? "");
	}

	public void SearchByName (string query) {

		IEnumerable<Tag> results = string.IsNullOrWhiteSpace(query)
		? AllTags
		: AllTags.Where(tag => tag.Name.Contains(query, StringComparison.OrdinalIgnoreCase));

		UpdateResults(results);
	}

	private void UpdateResults (IEnumerable<Tag> results) {
		currentResults = results.ToList();
		currentPage = 0;
		LoadPage();
	}
	#endregion

	#region UI
	private static IBrush GetColorForName (string category) => category switch {
		"Tag" => Brushes.Blue,
		"Artist" => Brushes.OrangeRed,
		"Character" => Brushes.Green,
		"Copyright" => Brushes.Magenta,
		"Species" => Brushes.Red,
		_ => Brushes.Black,
	};

	private static Button ButtonChangeCategory (string text) {
		Button bt = new() {
			HorizontalAlignment = HorizontalAlignment.Stretch,
			Content = new TextBlock() {
				TextAlignment = TextAlignment.Center,
				Text = text,
			}
		};
		return bt;
	}
	#endregion

}