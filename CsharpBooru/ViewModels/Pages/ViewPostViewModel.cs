using CsharpBooru.SQL;

namespace CsharpBooru.ViewModels.Pages;
public partial class ViewPostViewModel : ViewModelBase {

	public int Id { get; }

	public Collection Collection { get; }

	public int index { get; }

	private ViewPostViewModel () {
		Id = -1;
		Collection = null;
		index = -1;
	}

	public ViewPostViewModel (int id, Collection collection, int idCollection) {
		Id = id;
		Collection = collection;
		index = idCollection;
	}

}
