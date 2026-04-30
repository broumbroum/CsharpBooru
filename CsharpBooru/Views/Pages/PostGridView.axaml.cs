using Avalonia.Controls;
using CsharpBooru.Component;
using CsharpBooru.ViewModels;
using CsharpBooru.SQL;
using Avalonia.Media;
using CsharpBooru.ViewModels.Pages;

namespace CsharpBooru.Views.Pages;

public partial class PostGridView : UserControl{

	private int 
		_currentPage = 0,
		_totalPages = 0;

	private PostGridViewModel? Vm => DataContext as PostGridViewModel;

	public PostGridView(){

		InitializeComponent();

		Search_Component.Component(ref SearchPanel).Click += (_, _) => {
			MainWindowViewModel.main.PostGrid();
		};

		this.DataContextChanged += (_, _) => {
			_currentPage = Vm?.CurrentPage ?? 0;
			LoadPage();
		};
		
	}

	private void LoadPage () {

		PostGridList_Component pgl = new(GridPost, Click, ContextMenuPost);
		pgl.Descending(ref _currentPage, ref _totalPages);


		BuildPagination_Component.Component(PaginationPanelTop, _currentPage, _totalPages, page => {
			_currentPage = page;
			LoadPage();
		});

		BuildPagination_Component.Component(PaginationPanelDown, _currentPage, _totalPages, page => {
			_currentPage = page;
			LoadPage();
		});
	}

	private void Click (int index) {
		MainWindowViewModel.main.ViewImage(index);
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