namespace CsharpBooru.ViewModels.Pages;
internal class ViewCollectionsViewModel (int idCollection, int currentPage = 0) : ViewModelBase {
	public int IdCollection { get; } = idCollection;
	public int CurrentPage { get; } = currentPage;
}
