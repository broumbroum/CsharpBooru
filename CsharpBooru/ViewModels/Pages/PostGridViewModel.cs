namespace CsharpBooru.ViewModels.Pages;
public class PostGridViewModel : ViewModelBase {

	public int CurrentPage { get; } = 0;

	public PostGridViewModel (int currentPage = 0) {
		CurrentPage = currentPage;
	}
}
