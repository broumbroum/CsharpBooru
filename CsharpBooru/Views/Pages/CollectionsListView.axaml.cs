using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.VisualTree;
using CsharpBooru.Component;
using CsharpBooru.Setting;
using CsharpBooru.ViewModels;
using System;
using System.Collections.Generic;
using CsharpBooru.SQL;
using System.IO;

namespace CsharpBooru.Views.Pages;

public partial class CollectionsListView : UserControl {

	public int currentPage = 0;
	public int pageSize = 20;
	public int totalPages = 0;
	private List<int> CollectionList = [];

	public CollectionsListView() { 
		InitializeComponent();

		InitializaPage();

	}

	private void InitializaPage (int _currentPage = 0) {
		CollectionList.Clear();
		for (int i = 1; i < CollectionsManager.GetCount() +1; i++) {
			CollectionList.Add(i);
		}
		currentPage = _currentPage;

		LoadPage();
	}

	private void LoadPage () {
		ListCollections.Children.Clear();

		pageSize = SettingValue.PostPerPage;
		if (CollectionList.Count == 0) return;
		
		int totalCollections = CollectionList.Count;
		totalPages = (int)Math.Ceiling(totalCollections / (double)pageSize);

		int start = currentPage * pageSize;
		int end = start + pageSize;
		if (start < 0) start = 0;

		for (int i = start; i < end ; i++) {
			if (i >= totalCollections) break;
			ListCollections.Children.Add(ButtonListCollections(CollectionList[i]));
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

	private void AddCollection (object? sender, RoutedEventArgs e) {
		if (TextAddCollection.Text == "") return;

		Collection collection = new(0, TextAddCollection.Text ?? "New Collection", []);
		CollectionsManager.AddCollection(collection);

		InitializaPage();

	}

	private Button ButtonListCollections (int idCollections) {
		Collection collection = CollectionsManager.GetCollection(idCollections);

		StackPanel sp_all = new() {
			Orientation = Orientation.Horizontal
		};
		sp_all.Children.Add(Miniature(ref collection));
		sp_all.Children.Add(Text(ref collection));

		Button btn = new() {
			//Width = 800,
			HorizontalAlignment = HorizontalAlignment.Stretch,
			Height = 105,

			Margin = new Thickness(20, 5, 20, 0),

			Content = sp_all
		};

		btn.Click += (s, e) => {
			var window = this.FindAncestorOfType<Window>();

			if (window?.DataContext is MainWindowViewModel vm) vm.CollectionsWiew(idCollections);
		};

		btn.ContextMenu = ContextMenuCollections(idCollections);

		return btn;
	}

	private ContextMenu ContextMenuCollections (int index) {
		ContextMenu contextMenu = new();

		var openItem = new MenuItem {
			Header = "Open Collection"
		}; openItem.Click += (_, _) => {
			var window = this.FindAncestorOfType<Window>();
			if (window?.DataContext is MainWindowViewModel vm) vm.CollectionsWiew(index);
		};
		var deleteItem = new MenuItem() { 
			Header = "Delete Collection" 
		};
		deleteItem.Click += (s, e) => {
			System.Diagnostics.Debug.WriteLine("Collection " + index + " deleted");
			CollectionsManager.RemoveCollection(index);
			InitializaPage();
		};

		return new ContextMenu {
			Items = {
				openItem, deleteItem
			}
		};
	}

	private static Image Miniature (ref Collection collection) {
		if(collection == null || collection.Posts.Count == 0) return new Image();

		int id0 = Convert.ToInt32(collection.Posts[0]);

		Post post = PostsManager.GetPost(id0);
		if (post == null) return new Image();

		string pathImage = ThumbnailsManager.GetThumbnailPath(post.Filename);
		if(File.Exists(pathImage) == false) return new Image();

		return new() {
			Width = 100,
			Height = 100,
			Source = new Bitmap(pathImage)
		};
	}

	private static TextBlock Text (ref Collection collection) {
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

		tb.Margin = new(10, 10, 10, 10);
		tb.TextWrapping = TextWrapping.Wrap;

		return tb;
	}
}