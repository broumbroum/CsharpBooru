namespace CsharpBooru.ViewModels.Pages;
public class EditPostViewModel : ViewModelBase {

	public bool EditMod { get; }
	public int IdItem { get; }

	private EditPostViewModel() { }

	public EditPostViewModel (bool editMod = false, int idItem = -1) {
		EditMod = editMod;
		IdItem = idItem;
	}
}
