
namespace CsharpBooru.ViewModels.Pages;
public class CollectionsListViewModel : ViewModelBase {

	public int CurrentPage { get; } = 0;

	public CollectionsListViewModel (int currentPage = 0) {
		CurrentPage = currentPage;
	}
}
