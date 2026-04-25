using CsharpBooru.SQL;

namespace CsharpBooru.ViewModels.Pages;
public partial class ViewPostViewModel : ViewModelBase {

	public int Id { get; }

	public Collection Collection { get; }

	public int IdCollection { get; }

	private ViewPostViewModel () {
		Id = -1;
		Collection = null;
		IdCollection = -1;
	}

	public ViewPostViewModel (int id, Collection collection, int idCollection) {
		Id = id;
		Collection = collection;
		IdCollection = idCollection;
	}

}
