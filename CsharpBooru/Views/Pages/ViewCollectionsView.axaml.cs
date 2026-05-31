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
	private Collection collection;

	public ViewCollectionsView() {
		InitializeComponent();
		DataContextChanged += (s, e) => LoadPageView(Vm?.CurrentPage ?? 0);

		Search_Component.Component(Serched).Click += (_, _) => {
			SearchSQL.SearchPosts(SearchSQL.querySearch);
			LoadPageAdd();
		};

	}

	#region VIEW COLLECTION
	private List<Button> listButton = [];
	private int totalPagesCollection = 0, currentPageCollection = 0;

	public void LoadPageView(int _currentPage = 0) {
		currentPageCollection = _currentPage;
		CollectionsListe.Children.Clear();
		
		if (Vm == null) return;
		collection = CollectionsManager.GetCollection(Vm.IdCollection);
		NameTextBox.Text = collection.Name;

		LoadCollection();

		BuildPagination_Component.Component(PaginationWTop, currentPageCollection, totalPagesCollection, page => {
			currentPageCollection = page;
			MainWindowViewModel.main.navigationHistory.AddPage("CollectionsWiew&" + collection.Id + "&" + page);
			LoadPageView(page);
		});

		BuildPagination_Component.Component(PaginationWDown, currentPageCollection, totalPagesCollection, page => {
			currentPageCollection = page;
			MainWindowViewModel.main.navigationHistory.AddPage("CollectionsWiew&" + collection.Id + "&" + page);
			LoadPageView(page);
		});
	}

	public void LoadCollection () {
		listButton = [];
		for (int i = 0; i < collection.Posts.Count; i++) {
			Button btnP = ButtonPost_Component.Component(Convert.ToInt32(collection.Posts[i]));
			int viewPostID = Convert.ToInt32(collection.Posts[i]), positionList = i;

			btnP.Click += (_, _) => {
				MainWindowViewModel.main.ViewImage(viewPostID, collection.Id, positionList);
			};
			btnP.ContextMenu = ContextMenuPost(viewPostID, positionList);
			listButton.Add(btnP);
		}
		listButton.Add(ButtonAdd());

		int pageSize = SettingValue.PostPerPage;
		totalPagesCollection = (int)Math.Ceiling(listButton.Count / (double)pageSize);

		int start = currentPageCollection * pageSize;
		int end = start + pageSize;
		if (start < 0) start = 0;

		for (int i = start; i < end; i++) {
			if (i >= listButton.Count) break;
			CollectionsListe.Children.Add(listButton[i]);
		}
	}

	public ContextMenu ContextMenuPost (int id, int positionList) {
		MenuItem openItem = new () { Header = "Open Post"}; 
		openItem.Click += (_, _) => {
			MainWindowViewModel.main.ViewImage(id);
		};

		MenuItem MoveLeft = new() { Header = "Move Left" };
		MoveLeft.Click += (_, _) => {
			if(Vm == null) return;
			collection = CollectionsManager.GetCollection(Vm.IdCollection);

			if(positionList <= 0 || positionList >= collection.Posts.Count) return;
			(collection.Posts[positionList - 1], collection.Posts[positionList]) = (collection.Posts[positionList], collection.Posts[positionList - 1]);
			
			CollectionsManager.UpdateCollection(collection);
			MainWindowViewModel.main.CollectionsWiew(Vm.IdCollection);
		};

		MenuItem MoveRight = new() { Header = "Move Right" };
		MoveRight.Click += (_, _) => {
			if(Vm == null) return;
			collection = CollectionsManager.GetCollection(Vm.IdCollection);

			if(positionList < 0 || positionList >= collection.Posts.Count - 1) return;
			(collection.Posts[positionList + 1], collection.Posts[positionList]) = (collection.Posts[positionList], collection.Posts[positionList + 1]);
			
			CollectionsManager.UpdateCollection(collection);
			MainWindowViewModel.main.CollectionsWiew(Vm.IdCollection);
		};

		MenuItem regenerateThumbnailsItem = new() { Header = "Regenerate Thumbnails" };
		regenerateThumbnailsItem.Click += (_, _) => {
			ThumbnailsManager.RegenerateThumbnails(id);
			MainWindowViewModel.main.CollectionsWiew(Vm?.IdCollection ?? 0);
		};

		return new() {
			Items = {
				openItem,
				new Separator(),
				MoveLeft,MoveRight,regenerateThumbnailsItem
			}
		};
	}

	public Button ButtonAdd () {
		Button buttonAdd = new() {
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

		buttonAdd.Click += (_, _) => {
			AddCollection.IsVisible = true;
			ViewCollection.IsVisible = false;
			LoadPageAdd();
		};

		return buttonAdd;
	}
	#endregion

	#region Change Name Collection
	public void ChangeName (object? sender, RoutedEventArgs e) {
		if (Vm == null) return;
		collection.Name = NameTextBox.Text ?? "Collection " + collection.Id;
		CollectionsManager.UpdateCollection(collection);
	}

	public void OnNameChange (object? sender, TextChangedEventArgs e) {
		string s = NameTextBox.Text ?? "Collection " + collection.Id;
		collection.Name = s;
	}
	#endregion

	#region ADD COLLECTION
	private int currentPage_AddCollection = 0, totalPages_AddCollection = 0;

	private void LoadPageAdd () {
		List<int> postList = SearchSQL.SearchPosts(SearchSQL.querySearch);
		GridList_Component pgl = new(GridPost);
		
		pgl.OnCreateButton += (int id) => {
			int postIndex = postList[id];
			Button btn = ButtonPost_Component.Component(postIndex);

			btn.Click += (_, _) => {
				for (int cp = 0; cp < collection.Posts.Count; cp++) {
					if (collection.Posts[cp] == postIndex) {
						collection.Posts.RemoveAt(cp);
						return;
					}
				}
				collection.Posts.Add(postIndex);
				ActionButton(postIndex, btn);
			};
			ActionButton(postIndex, btn);
			return btn;
		};

		pgl.Descending(ref currentPage_AddCollection, ref totalPages_AddCollection, postList.Count);

		BuildPagination_Component.Component(PaginationPanelTop, currentPage_AddCollection, totalPages_AddCollection, page => {
			currentPage_AddCollection = page;
			LoadPageAdd();
		});

		BuildPagination_Component.Component(PaginationPanelDown, currentPage_AddCollection, totalPages_AddCollection, page => {
			currentPage_AddCollection = page;
			LoadPageAdd();
		});
	}

	public void ActionButton (int index, Button btn) {
		btn.Background = new SolidColorBrush(Colors.Transparent);

		for (int cp = 0; cp < collection.Posts.Count; cp++) {
			if (collection.Posts[cp] == index) {
				btn.Background = new SolidColorBrush(Colors.LightBlue);
				break;
			}
		}
	}

	public void RetunWiewCollection(object? sender, RoutedEventArgs e) {
		AddCollection.IsVisible = false;
		ViewCollection.IsVisible = true;

		if (Vm != null) CollectionsManager.UpdateCollection(collection);

		LoadPageView();
	}
	#endregion
}