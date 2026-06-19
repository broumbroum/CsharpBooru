using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using CsharpBooru.Component;
using CsharpBooru.Setting;
using CsharpBooru.SQL;
using CsharpBooru.ViewModels;
using CsharpBooru.ViewModels.Pages;
using System;
using System.Collections.Generic;

namespace CsharpBooru.Views.Pages;

public partial class ViewCollectionsView : UserControl {

	private ViewCollectionsViewModel? Vm => DataContext as ViewCollectionsViewModel;
	private Collection? collection;

	public ViewCollectionsView () {
		InitializeComponent();

		// Reload the page when the DataContext changes
		DataContextChanged += (s, e) => LoadCollectionPage(Vm?.CurrentPage ?? 0);

		Search_Component.Component(Serched).Click += (_, _) => {
			SearchSQL.SearchPosts(SearchSQL.querySearch);
			LoadAddCollectionPage();
		};

	}

	#region VIEW COLLECTION
	private List<Button> _postButtons = [];
	private int _totalPages = 0, _currentPage = 0;

	// Load the main collection view page
	public void LoadCollectionPage(int _currentPage = 0) {
		this._currentPage = _currentPage;
		CollectionsListe.Children.Clear();
		
		if (Vm == null) return;
		collection = CollectionsManager.GetCollection(Vm.IdCollection);
		NameTextBox.Text = collection.Name;

		BuildCollectionButtons();

		BuildPagination_Component.Component([PaginationWTop, PaginationWDown], this._currentPage, _totalPages, page => {
			this._currentPage = page;
			MainWindowViewModel.Main?.navigationHistory.AddPage("CollectionsWiew&" + collection.Id + "&" + page);
			LoadCollectionPage(page);
		});
	}

	// Build the list of post buttons for the current collection
	public void BuildCollectionButtons () {
		if(collection == null) return;

		_postButtons = [];
		for (int i = 0; i < collection.Posts.Count; i++) {
			Button postButton = ButtonPost_Component.Component(Convert.ToInt32(collection.Posts[i]));
			int postId = Convert.ToInt32(collection.Posts[i]), postIndex = i;

			postButton.Click += (_, _) => {
				MainWindowViewModel.Main?.ViewImage(postId, collection.Id, postIndex);
			};
			postButton.ContextMenu = CreatePostContextMenu(postId, postIndex);
			_postButtons.Add(postButton);
		}
		_postButtons.Add(CreateAddButton());

		int pageSize = SettingValue.PostPerPage;
		_totalPages = (int)Math.Ceiling(_postButtons.Count / (double)pageSize);

		int start = _currentPage * pageSize;
		int end = start + pageSize;
		if (start < 0) start = 0;

		for (int i = start; i < end; i++) {
			if (i >= _postButtons.Count) break;
			CollectionsListe.Children.Add(_postButtons[i]);
		}
	}

	// Context menu for each post inside the collection
	public ContextMenu CreatePostContextMenu (int id, int positionList) {
		MenuItem openItem = new () { Header = "Open Post"}; 
		openItem.Click += (_, _) => {
			MainWindowViewModel.Main?.ViewImage(id);
		};

		MenuItem moveLeftItem = new() { Header = "Move Left" };
		moveLeftItem.Click += (_, _) => {
			if(Vm == null) return;
			collection = CollectionsManager.GetCollection(Vm.IdCollection);

			if(positionList <= 0 || positionList >= collection.Posts.Count) return;
			(collection.Posts[positionList - 1], collection.Posts[positionList]) = (collection.Posts[positionList], collection.Posts[positionList - 1]);
			
			CollectionsManager.UpdateCollection(collection);
			MainWindowViewModel.Main?.CollectionsWiew(Vm.IdCollection);
		};

		MenuItem moveRightItem = new() { Header = "Move Right" };
		moveRightItem.Click += (_, _) => {
			if(Vm == null) return;
			collection = CollectionsManager.GetCollection(Vm.IdCollection);

			if(positionList < 0 || positionList >= collection.Posts.Count - 1) return;
			(collection.Posts[positionList + 1], collection.Posts[positionList]) = (collection.Posts[positionList], collection.Posts[positionList + 1]);
			
			CollectionsManager.UpdateCollection(collection);
			MainWindowViewModel.Main?.CollectionsWiew(Vm.IdCollection);
		};

		MenuItem regenThumbsItem = new() { Header = "Regenerate Thumbnails" };
		regenThumbsItem.Click += (_, _) => {
			ThumbnailsManager.RegenerateThumbnails(id);
			MainWindowViewModel.Main?.CollectionsWiew(Vm?.IdCollection ?? 0);
		};

		return new() {
			Items = {
				openItem,
				new Separator(),
				moveLeftItem,moveRightItem,regenThumbsItem
			}
		};
	}

	// Save the new name when the user confirms
	public void OnChangeNameClicked (object? sender, RoutedEventArgs e) {
		if (Vm == null || collection == null) return;
		collection.Name = NameTextBox.Text ?? "Collection " + collection.Id;
		CollectionsManager.UpdateCollection(collection);
	}

	// Update the name live as the user types
	public void OnChangeNameTextBox (object? sender, TextChangedEventArgs e) {
		if (collection == null) return;

		string newName = NameTextBox.Text ?? "Collection " + collection.Id;
		collection.Name = newName;
	}
	#endregion

	// Button used to add posts to the collection
	public Button CreateAddButton () {
		Button addButton = new() {
			Content = new TextBlock {
				Text = "+",
				VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
				HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
				FontSize = 24,
				FontWeight = FontWeight.ExtraBold
			},
			Width = 200,
			Height = 200,
			Margin = new Thickness(10),
			Background = Brushes.Transparent,
		};

		addButton.Click += (_, _) => {
			AddCollection.IsVisible = true;
			ViewCollection.IsVisible = false;
			LoadAddCollectionPage();
		};

		return addButton;
	}

	#region ADD COLLECTION
	private int currentPage_AddCollection = 0, totalPages_AddCollection = 0;

	// Load the Add Collection page (list of all posts)
	private void LoadAddCollectionPage () {
		if (collection == null) return;

		List<int> postList = SearchSQL.SearchPosts(SearchSQL.querySearch);
		GridList_Component gridList = new(GridPost);
		
		gridList.OnCreateButton += id => {
			int postIndex = postList[id];
			Button btn = ButtonPost_Component.Component(postIndex);

			btn.Click += (_, _) => {
				for (int cp = 0; cp < collection.Posts.Count; cp++) {
					if (collection.Posts[cp] == postIndex) {
						collection.Posts.RemoveAt(cp);
						UpdatePostButtonState(postIndex, btn);
						return;
					}
				}
				collection.Posts.Add(postIndex);
				UpdatePostButtonState(postIndex, btn);
			};

			UpdatePostButtonState(postIndex, btn);
			return btn;
		};

		gridList.Descending(ref currentPage_AddCollection, ref totalPages_AddCollection, postList.Count);

		BuildPagination_Component.Component([PaginationPanelTop, PaginationPanelDown], currentPage_AddCollection, totalPages_AddCollection, page => {
			currentPage_AddCollection = page;
			LoadAddCollectionPage();
		});
	}

	// Highlight the button if the post is already part of the collection
	public void UpdatePostButtonState (int index, Button btn) {
		if (collection == null) return;

		btn.Background = new SolidColorBrush(Colors.Transparent);

		for (int cp = 0; cp < collection.Posts.Count; cp++) {
			if (collection.Posts[cp] == index) {
				btn.Background = new SolidColorBrush(Colors.LightBlue);
				return;
			}
		}
	}

	// Return to the main collection view
	public void OnReturnToCollectionViewClicked (object? sender, RoutedEventArgs e) {
		if (collection == null) return;

		AddCollection.IsVisible = false;
		ViewCollection.IsVisible = true;

		if (Vm != null) CollectionsManager.UpdateCollection(collection);

		LoadCollectionPage();
	}
	#endregion
}