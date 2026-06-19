using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.VisualTree;
using CsharpBooru.Component;
using CsharpBooru.SQL;
using CsharpBooru.ViewModels;
using CsharpBooru.ViewModels.Pages;
using System;
using System.Collections.Generic;
using System.IO;

namespace CsharpBooru.Views.Pages;

public partial class CollectionsListView : UserControl {

	public int currentPage = 0,totalPages = 0;
	private readonly List<int> CollectionIds = [];

	private CollectionsListViewModel? Vm => DataContext as CollectionsListViewModel;

	public CollectionsListView() { 
		InitializeComponent();

		DataContextChanged += (s, e) => InitializePage(Vm?.CurrentPage ?? 0);

	}

	//Initializes the page by rebuilding the collection list and loading the first page
	private void InitializePage (int pageIndex = 0) {
		CollectionIds.Clear();
		for (int i = 1; i < CollectionsManager.GetCount() +1; i++) {
			CollectionIds.Add(i);
		}
		currentPage = pageIndex;

		LoadCurrentPage();
	}

	// Loads the current page: clears UI, builds buttons, and sets up pagination
	private void LoadCurrentPage () {
		CollectionsPanel.Children.Clear();

		GridList_Component gridList = new (CollectionsPanel);
		gridList.OnCreateButton += id => CreateCollectionButton(CollectionIds[id]);
		gridList.Ascending(ref currentPage, ref totalPages, CollectionsManager.GetCount());

		BuildPagination_Component.Component([PaginationTopPanel, PaginationBottomPanel], currentPage, totalPages, page => {
			currentPage = page;
			MainWindowViewModel.Main?.navigationHistory.AddPage("CollectionsList&" + currentPage);
			LoadCurrentPage();
		});
	}

	// Adds a new collection using the text input
	private void OnAddCollectionClicked (object? sender, RoutedEventArgs e) {
		if (NewCollectionTextBox.Text == "") return;

		Collection collection = new(0, NewCollectionTextBox.Text ?? "New Collection", []);
		CollectionsManager.AddCollection(collection);

		InitializePage(currentPage);

	}

	#region Create UI
	// Creates a button representing a collection entry
	private Button CreateCollectionButton (int collectionId) {
		Collection collection = CollectionsManager.GetCollection(collectionId);

		DockPanel contentPanel = new();
		contentPanel.Children.Add(CreateThumbnailImage(collection));
		contentPanel.Children.Add(CreateCollectionTextBlock(collection));

		Button btn = new() {
			HorizontalAlignment = HorizontalAlignment.Stretch,
			Height = 105,

			Margin = new Thickness(20, 5, 20, 0),

			Content = contentPanel
		};

		btn.Click += (s, e) => MainWindowViewModel.Main?.CollectionsWiew(collectionId);
		btn.ContextMenu = CreateCollectionContextMenu(collectionId);

		return btn;
	}
	
	// Builds the context menu for each button collection
	private ContextMenu CreateCollectionContextMenu (int collectionId) {
		ContextMenu contextMenu = new();

		var openMenuItem = new MenuItem {
			Header = "Open Collection"
		}; openMenuItem.Click += (_, _) => MainWindowViewModel.Main?.CollectionsWiew(collectionId);
		var deleteMenuItem = new MenuItem() { 
			Header = "Delete Collection",
			Foreground = Brushes.Red
		};
		deleteMenuItem.Click += (s, e) => {
			CollectionsManager.RemoveCollection(collectionId);
			InitializePage(currentPage);
		};

		return new ContextMenu {
			Items = {
				openMenuItem,
				new Separator(),
				deleteMenuItem
			}
		};
	}
	
	// Returns the thumbnail image of the first post in the collection
	private static Image CreateThumbnailImage (Collection collection) {
		if(collection == null || collection.Posts.Count == 0) return new Image();

		int firstPostId = Convert.ToInt32(collection.Posts[0]);

		Post post = PostsManager.GetPost(firstPostId);
		if (post == null) return new Image();

		string pathImage = ThumbnailsManager.GetThumbnailPath(post.Filename);
		if(File.Exists(pathImage) == false) return new Image();

		return new() {
			Width = 100,
			Height = 100,
			Source = new Bitmap(pathImage)
		};
	}

	// Builds the text block containing collection name, ID, and number of posts
	private static TextBlock CreateCollectionTextBlock (Collection collection) {
		TextBlock tb = new() {
			Inlines = [],
			Height = 100,
		};
		
		tb.Inlines.Add(new Run { 
			Text = "" + collection?.Name ?? "/!\\ collection = null /!\\", 
			FontSize = 15, 
			FontWeight = FontWeight.Bold,
		});
		tb.Inlines.Add(new Run { 
			Text = "\nID : " + collection?.Id ?? "null",
			FontSize = 12, 
			FontWeight = FontWeight.Normal 
		});
		tb.Inlines.Add(new Run {
			Text = "\nPage : " + collection?.Posts.Count ?? "null",
			FontSize = 12,
			FontWeight = FontWeight.Normal
		});

		tb.Margin = new(10);
		tb.TextWrapping = TextWrapping.Wrap;

		return tb;
	}
	#endregion
}