
namespace CsharpBooru.ViewModels.Pages;
internal class ViewCollectionsViewModel : ViewModelBase {
	public int IdCollection { get; }
	public int CurrentPage { get; }

	private ViewCollectionsViewModel () { }

	public ViewCollectionsViewModel (int idCollection, int currentPage = 0) { 
		IdCollection = idCollection; 
		CurrentPage = currentPage;
	}
}
