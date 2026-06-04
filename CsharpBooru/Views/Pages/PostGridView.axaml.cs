using Avalonia.Controls;
using Avalonia.Media;
using CsharpBooru.Component;
using CsharpBooru.SQL;
using CsharpBooru.ViewModels;
using CsharpBooru.ViewModels.Pages;

using System.Collections.Generic;

namespace CsharpBooru.Views.Pages;

public partial class PostGridView : UserControl{

	private int 
		_currentPage = 0,
		_totalPages = 0;

	private PostGridViewModel? Vm => DataContext as PostGridViewModel;

	public PostGridView(){

		InitializeComponent();

		Search_Component.Component(SearchPanel).Click += (_, _) => {
			MainWindowViewModel.main.PostGrid();
		};

		this.DataContextChanged += (_, _) => {
			_currentPage = Vm?.CurrentPage ?? 0;
			LoadPage();
		};
		
	}

	private void LoadPage () {
		GridList_Component pgl = new(GridPost);
		List<int> postList = SearchSQL.SearchPosts(SearchSQL.querySearch);
		
		pgl.OnCreateButton += (int id) => {
			int postIndex = postList[id];
			Button btnP = ButtonPost_Component.Component(postIndex);
			btnP.Click += (_, _) => MainWindowViewModel.main.ViewImage(postIndex);
			btnP.ContextMenu = ContextMenuPost(postIndex);
			return btnP;
		};

		pgl.Descending(ref _currentPage, ref _totalPages, postList.Count);

		if (postList.Count == 0) {
			TextBlock tb = new() {
				Text = "\n\n\n\n\n No posts found.",
				HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
				VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
				FontSize = 16, FontWeight = FontWeight.Bold,
				Foreground = Brushes.Red,
			};
			GridPost.Children.Add(tb);
		} else {
			BuildPagination_Component.Component([PaginationPanelTop, PaginationPanelDown], _currentPage, _totalPages, page => {
				_currentPage = page;
				MainWindowViewModel.main.navigationHistory.AddPage("PostGrid&" + _currentPage + "&" + SearchSQL.querySearch);
				LoadPage();
			});
		}
	}

	private ContextMenu ContextMenuPost (int index) {

		var openItem = new MenuItem {
			Header = "Open Post"
		}; openItem.Click += (_, _) => {
			MainWindowViewModel.main.ViewImage(index);
		};

		var editItem = new MenuItem {
			Header = "Edit Post"
		}; editItem.Click += (_, _) => {
			MainWindowViewModel.main.EditPost(true, index);
		};

		var deleteItem = new MenuItem {
			Header = new TextBlock {
				Text = "Delete Post",
				Foreground = Brushes.Red
			}
		}; deleteItem.Click += (_, _) => {
			PostsManager.RemovePost(index);
			MainWindowViewModel.main.PostGrid();
		};

		var regenerateThumbnailsItem = new MenuItem {
			Header = "Regenerate Thumbnails"
		}; regenerateThumbnailsItem.Click += (_, _) => {
			ThumbnailsManager.RegenerateThumbnails(index);
			MainWindowViewModel.main.PostGrid();
		};

		return new ContextMenu {
			Items = {
				openItem,
				new Separator(),
				editItem,regenerateThumbnailsItem,
				new Separator(),
				deleteItem,
			}
		};
	}
}