namespace CsharpBooru.ViewModels.Pages;
public class EditPostViewModel (bool editMod = false, int idItem = -1) : ViewModelBase {

	public bool EditMod { get; } = editMod;
	public int IdItem { get; } = idItem;
}
