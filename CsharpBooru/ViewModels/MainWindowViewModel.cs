using CsharpBooru.ViewModels.Pages;
using ReactiveUI;
using CsharpBooru.SQL;

namespace CsharpBooru.ViewModels;

public partial class MainWindowViewModel : ViewModelBase {

	public static MainWindowViewModel main;

	public MainWindowViewModel () {
		main = this;
	}

	public readonly NavigationHistory navigationHistory = new();

	#region Pages
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
	#endregion


	public void HomePage () {
		navigationHistory.AddPage("HomePage");
		CurrentPage = new HomeViewModel(); 
	}
	public void TagPage () { 
		navigationHistory.AddPage("TagPage");
		CurrentPage = new TagsListViewModel(); 
	}
	public void ViewImage (int id, int? idCollection = null, int index = -1) { 
		Collection? collection = null;
		if (idCollection != null) collection = CollectionsManager.GetCollection(System.Convert.ToInt32(idCollection?? 0));

		navigationHistory.AddPage("ViewImage&" + id + "&" + idCollection + "&" + index);
		CurrentPage = new ViewPostViewModel(id, collection, index); 
	}
	public void PostGrid (int currenPagePost = 0) {
		navigationHistory.AddPage("PostGrid&" + currenPagePost + "&" + SearchSQL.querySearch);
		CurrentPage = new PostGridViewModel(currenPagePost);
	}
	public void CollectionsList () {
		navigationHistory.AddPage("CollectionsList");
		CurrentPage = new CollectionsListViewModel();
	}
	public void EditPost (bool editMode = false, int idItem = 0) { 
		navigationHistory.AddPage("EditPost&" + editMode.ToString() + "&" + idItem);
		CurrentPage = new EditPostViewModel(editMode, idItem); 
	}
	public void CollectionsWiew (int id) { 
		navigationHistory.AddPage("CollectionsWiew&" + id);
		CurrentPage = new ViewCollectionsViewModel(id); 
	}
	public void Setting () { 
		navigationHistory.AddPage("Setting");
		CurrentPage = new SettingViewModel(); 
	}
}
