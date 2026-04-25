using CsharpBooru.ViewModels.Pages;
using ReactiveUI;
using CsharpBooru.SQL;

namespace CsharpBooru.ViewModels;

public partial class MainWindowViewModel : ViewModelBase {

	public static MainWindowViewModel main;

	public MainWindowViewModel () {
		main = this;
	}

	public ViewModelBase
		topBar = new TopBarViewModel(),
		currentPage = new HomeViewModel();

	public ViewModelBase TopBar {
		get => topBar;
		set => topBar = value;
	}


	public ViewModelBase CurrentPage {
		get => currentPage;
		set => this.RaiseAndSetIfChanged(ref currentPage, value);
	}


	public void HomePage () => CurrentPage = new HomeViewModel();
	public void TagPage () => CurrentPage = new TagsListViewModel();
	public void ViewImage (int id, Collection collection = null, int idCollection = -1) => CurrentPage = new ViewPostViewModel(id, collection, idCollection);
	public void PostGrid () => CurrentPage = new PostGridViewModel();
	public void CollectionsList () => CurrentPage = new CollectionsListViewModel();
	public void EditPost (bool editMode = false, int idItem = 0) => CurrentPage = new EditPostViewModel(editMode, idItem);
	public void CollectionsWiew (int id) => CurrentPage = new ViewCollectionsViewModel(id);
	public void Setting () => CurrentPage = new SettingViewModel();
}
