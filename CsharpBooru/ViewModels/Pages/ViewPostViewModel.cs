using CsharpBooru.SQL;

namespace CsharpBooru.ViewModels.Pages;
public partial class ViewPostViewModel (int id, Collection collection, int idCollection) : ViewModelBase {

	public int Id { get; } = id;
	public Collection Collection { get; } = collection;
	public int Index { get; } = idCollection;
}
