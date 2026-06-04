using CsharpBooru.SQL;

namespace CsharpBooru.ViewModels.Pages;

public partial class TagsListViewModel : ViewModelBase{

	public static string Titre => "Tags : " + TagsManager.GetCount();

	public TagsListViewModel () {}

}
