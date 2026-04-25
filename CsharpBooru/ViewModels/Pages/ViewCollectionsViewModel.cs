
namespace CsharpBooru.ViewModels.Pages;
internal class ViewCollectionsViewModel : ViewModelBase {
	public int IdCollection { get; }

	private ViewCollectionsViewModel () { }

	public ViewCollectionsViewModel (int idCollection) => IdCollection = idCollection;
}
