using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using CsharpBooru.Component;
using CsharpBooru.SQL;
using CsharpBooru.ViewModels;
using CsharpBooru.ViewModels.Pages;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CsharpBooru.Views.Pages;

public partial class EditPostView : UserControl {

	private string path = "",tag = "", rating = "none", note = "";
	private List<string> sources = [];
	private List<int> relatedsID = [];

	private EditPostViewModel? Vm => DataContext as EditPostViewModel;

	public EditPostView () { 
		InitializeComponent();

		DataContextChanged += OnDataContextChanged;

		RatingBox.SelectionChanged += (_, _) => {
			var valeur = (RatingBox.SelectedItem as ComboBoxItem)?.Content;
			rating = valeur?.ToString()?.ToLower() ?? "none";
		};

		Search_Component.Component(Serched).Click += (_,_) => {
			LoadPagePost();
			SearchSQL.SearchPosts(SearchSQL.querySearch);
		};

	}

	//Initializes UI fields when editing an existing post.
	//Called automatically when DataContext changes.
	private void OnDataContextChanged (object? sender, EventArgs e) {
		if (Vm == null) {
			return;
		} else if (Vm.EditMod == true) {
			Post post = PostsManager.GetPost(Vm.IdItem);
			if (post == null) return;

			path = DataBase.FilesPath + post.Filename;
			PathText.Text = path;

			tag = " ";
			for (int i = 0; i < post.Tags.Count; i++) {
				Tag t = TagsManager.GetTag(post.Tags[i]);
				tag +=  t.Name + " ";
			}
			TagBox.Text = tag;

			rating = post.Rating;
			RatingBox.SelectedIndex = rating switch {
				"none" => 0,
				"safe" => 1,
				"questionable" => 2,
				"explicit" => 3,
				"borderline" => 4,
				_ => 0,
			};

			note = post.Note;
			NoteText.Text = note;

			sources = post.Sources;
			string _source = "";
			for (int i = 0; i < sources.Count; i++) {
				_source = _source + sources[i] + "\n";
			}
			SourceBox.Text = _source;

			relatedsID = post.Related;
			RelatedTextUpdate();
		}
	}

	//Opens a file picker to select a new image/file for the post.
	private async void OnSelectFileClick (object? sender, RoutedEventArgs e) {

		var topLevel = TopLevel.GetTopLevel(this);
		if (topLevel == null) return;

		var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions {
			AllowMultiple = false,
			Title = "Select a File"
		});

		if (files.Count == 0) return;

		string path = files[0].Path.LocalPath;

		this.path = path;
		PathText.Text = path;

	}

	//Updates tag string when user edits the TagBox.
	private void OnTagChanged (object? sender, TextChangedEventArgs e) {
		if (sender is TextBox tb) tag = tb.Text ?? "";
	}
	
	//Updates the list of sources when user edits the SourceBox.
	//Each line becomes one source entry.
	private void OnSourceChanged (object? sender, TextChangedEventArgs e) {
		string s = SourceBox.Text ?? "";
		sources = [.. s.Split('\n', StringSplitOptions.RemoveEmptyEntries).Select(l => l.Trim()).Where(l => l.Length > 0)];
	}

	//Updates the note string when user edits the NoteText.
	private void OnNoteChanged (object? sender, TextChangedEventArgs e) {
		if (sender is TextBox tb) note = tb.Text ?? "";
	}

	//Switches to the "related posts" selection grid
	private void OnRelateClick (object? sender, RoutedEventArgs e) {
		RelatedPost.IsVisible = true;
		Main.IsVisible = false;
		LoadPagePost();
	}

	//Saves the post (new or edited) into the database.
	private void OnSavePostClick (object? sender, RoutedEventArgs e) {
		if (Vm == null) return;

		if (path == null || path == "") {
			Error.Text = "No file selected";
			return; 
		}
		if (tag == null || tag == "") {
			Error.Text = "No tag";
			return;
		}

		List<int> TagID = [];
		foreach (string t in tag.Split(' ', StringSplitOptions.RemoveEmptyEntries)) {
			TagID.Add(TagsManager.GetOrCreateTag(t));
		}

		if (Vm.EditMod == false) {
			Post _post = new (0, FileManager.SaveFile(this.path), TagID, sources, relatedsID, note, rating);
			PostsManager.AddPost(_post);
		} else {
			Post _post = new(Vm.IdItem, FileManager.SaveFile(this.path), TagID, sources, relatedsID, note, rating);
			PostsManager.UpdatePost(_post);
		}

		MainWindowViewModel.Main?.HomePage();
	}

	#region GridRelated
	private int
		_currentPage = 0,
		_totalPages = 0;

	//Loads the paginated list of posts for selecting related posts.
	private void LoadPagePost () {
		List<int> postList = SearchSQL.SearchPosts(SearchSQL.querySearch);
		GridList_Component pgl = new(GridPost);
		
		pgl.OnCreateButton += id => {
			int postIndex = postList[id];
			Button btn = ButtonPost_Component.Component(postIndex);

			btn.Click += (_, _) => {
				if (Vm?.IdItem == postIndex) return;
				for (int r = 0; r < relatedsID.Count; r++) {
					if (relatedsID[r] == postIndex) {
						relatedsID.RemoveAt(r);
						goto Exite;
					}
				}
				relatedsID.Add(postIndex);
			Exite:;
				RelatedTextUpdate();
				ActionButton(postIndex, btn);
			};

			ActionButton(postIndex, btn);
			return btn;
		};
		
		pgl.Descending(ref _currentPage, ref _totalPages, postList.Count);

		BuildPagination_Component.Component([PaginationPanelTop, PaginationPanelDown], _currentPage, _totalPages, page => {
			_currentPage = page;
			LoadPagePost();
		});
	}

	//L'arri�re-plan du bouton est mis � jour en fonction de l'�tat de s�lection
	private void ActionButton (int index, Button btn) {
		btn.Background = new SolidColorBrush(Colors.Transparent);

		if (Vm?.IdItem == index) {
			btn.Background = new SolidColorBrush(Colors.Red);
			return;
		}

		for (int a = 0; a < relatedsID.Count; a++) {
			if (relatedsID[a] == index) {
				btn.Background = new SolidColorBrush(Colors.LightBlue);
				break;
			}
		}
	}

	//Returns to the main edit screen.
	private void RetunMain (object? sender, RoutedEventArgs e) {
		RelatedPost.IsVisible = false;
		Main.IsVisible = true;
	}

	//Updates the text showing selected related post IDs.
	private void RelatedTextUpdate () {
		string str = "";
		for (int i = 0; i < relatedsID.Count; i++) {
			str += "ID:" + relatedsID[i] + " ";
		}
		RelatedText.Text = str;
	}
	#endregion
}