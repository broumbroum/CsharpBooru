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

	private string path = "";
	private List<int> relatedsID = [];

	private EditPostViewModel? Vm => DataContext as EditPostViewModel;

	public EditPostView () { 
		InitializeComponent();

		DataContextChanged += OnDataContextChanged;

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

			//---- Tag ----//
			string tag = " ";
			for (int i = 0; i < post.Tags.Count; i++) {
				Tag t = TagsManager.GetTag(post.Tags[i]);
				tag +=  t.Name + " ";
			}
			TagBox.Text = tag;

			//---- Rating ----//
			RatingBox.SelectedIndex = post.Rating switch {
				"none" => 0,
				"safe" => 1,
				"questionable" => 2,
				"explicit" => 3,
				"borderline" => 4,
				_ => 0,
			};

			NoteText.Text = post.Note;

			//---- Sources ----//
			string _source = "";
			for (int i = 0; i < post.Sources.Count; i++) {
				_source = _source + post.Sources[i] + "\n";
			}
			SourceBox.Text = _source;

			//---- Relateds ----//
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

	//Switches to the "related posts" selection grid
	private void OnRelateClick (object? sender, RoutedEventArgs e) {
		RelatedPost.IsVisible = true;
		Main.IsVisible = false;
		LoadPagePost();
	}

	//Saves the post (new or edited) into the database.
	private void OnSavePostClick (object? sender, RoutedEventArgs e) {
		if (Vm == null) return;

		//---- Path ----//
		if (path == null || path == "") {
			Error.Text = "No file selected";
			return; 
		}

		//---- Tag ----//
		List<int> TagID = [];
		foreach (string t in TagBox?.Text?.Split(' ', StringSplitOptions.RemoveEmptyEntries) ?? []) {
			TagID.Add(TagsManager.GetOrCreateTag(t));
		}

		if (TagID.Count <= 0) {
			Error.Text = "No tag";
			return;
		}

		//---- Rating ----//
		string rating = RatingBox.SelectedIndex switch {
			0 => "none",
			1 => "safe",
			2 => "questionable",
			3 => "explicit",
			4 => "borderline",
			_ => "none",
		};

		//---- Sources ----//
		List<string> sources = [.. SourceBox.Text?.Split('\n', StringSplitOptions.RemoveEmptyEntries).Select(l => l.Trim()).Where(l => l.Length > 0)?? []];

		if (Vm.EditMod == false) {
			Post _post = new (0, FileManager.SaveFile(this.path), TagID, sources, relatedsID, NoteText.Text ?? "", rating);
			PostsManager.AddPost(_post);
		} else {
			Post _post = new(Vm.IdItem, FileManager.SaveFile(this.path), TagID, sources, relatedsID, NoteText.Text ?? "", rating);
			PostsManager.UpdatePost(_post);
		}

		MainWindowViewModel.Main?.navigationHistory.Back();
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

	//The button background is updated based on the selection state.
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